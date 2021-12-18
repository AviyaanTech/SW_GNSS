using SW_GNSS.Processing;
using SwCad.Primitives;
using SwCad.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_GNSS.SwMaps
{
	class GnssSolutionSet
	{
		public List<SolutionPoint> Points { get; set; }

		public GnssSolutionSet(List<SolutionPoint> points)
		{
			Points = new List<SolutionPoint>(points);
			Sort();
		}

		public void Sort()
		{
			Points.Sort((p1, p2) => p1.Time.CompareTo(p2.Time));
		}

		/// <summary>
		/// Returns the list of points in between the start and end time
		/// </summary>
		/// <param name="startTime">The start time, unix milliseconds</param>
		/// <param name="endTime">The end time, unix milliseconds</param>
		/// <returns>The list of points</returns>
		public List<SolutionPoint> GetPoints(long startTime, long endTime)
		{
			var dt1 = DateTimeOffset.FromUnixTimeMilliseconds(startTime).UtcDateTime;
			var dt2 = DateTimeOffset.FromUnixTimeMilliseconds(endTime).UtcDateTime;

			if (dt1 == dt2)
			{
				dt1 = dt1.Subtract(TimeSpan.FromSeconds(1));
				dt2 = dt2.Add(TimeSpan.FromSeconds(1));
			}
			var c1 = BinarySearch(dt1, true);
			var c2 = BinarySearch(dt2, false);

			var ret = new List<SolutionPoint>();
			for (int i = c1; i <= c2; i++)
			{
				if (Points[i].Time >= dt1 && Points[i].Time <= dt2)
					ret.Add(Points[i]);
			}

			return ret;
		}

		/// <summary>
		/// Returns the best estimate of the solution point averaged between the start and end time
		/// </summary>
		/// <param name="startTime">The start time, unix milliseconds</param>
		/// <param name="endTime">The end time, unix milliseconds</param>
		/// <returns>the solution point</returns>
		public AveragedSolutionPoint GetBestSolutionPoint(long startTime, long endTime)
		{
			var points = GetPoints(startTime, endTime);
			if (points.Count == 0) return null;
			var ret = new AveragedSolutionPoint(points);
			if (ret.PointCount == 0) return null;
			return ret;
		}

		int BinarySearch(DateTime dt, bool greaterThan)
		{
			int a = 0;
			int b = Points.Count - 1;

			if (dt <= Points[a].Time) return a;
			if (dt >= Points[b].Time) return b;
			int c = (a + b) / 2;

			while (b - a > 2)
			{
				if (dt < Points[c].Time)
				{
					b = c;
				}
				else
				{
					a = c;
				}
				c = (a + b) / 2;
			}

			if (greaterThan)
			{
				if (Points[c].Time > dt)
					return c;
				else
					return c - 1;
			}
			else
			{
				if (Points[c].Time < dt)
					return c;
				else
					return c + 1;
			}
		}
	}

	class AveragedSolutionPoint
	{
		public double Latitude { get; private set; }
		public double Longitude { get; private set; }
		public double Elevation { get; private set; }
		public double SDN { get; private set; }
		public double SDE { get; private set; }
		public double SDU { get; private set; }
		public FixQuality FixQuality { get; private set; }
		public int PointCount { get; private set; }

		static Dictionary<FixQuality, int> QualityRanking;

		static AveragedSolutionPoint()
		{
			QualityRanking = new Dictionary<FixQuality, int>();
			QualityRanking.Add(FixQuality.Fix, 1);
			QualityRanking.Add(FixQuality.Float, 2);
			QualityRanking.Add(FixQuality.PPP, 3);
			QualityRanking.Add(FixQuality.DGPS, 4);
			QualityRanking.Add(FixQuality.SBAS, 5);
			QualityRanking.Add(FixQuality.Single, 6);
		}
		private void GetFixQuality(IEnumerable<SolutionPoint> points)
		{
			//make a pass and get the best quality rank
			var rank = QualityRanking[FixQuality.Single];
			FixQuality = FixQuality.Single;
			foreach (var pt in points)
			{
				if (QualityRanking[pt.FixQuality] < rank)
				{
					rank = QualityRanking[pt.FixQuality];
					FixQuality = pt.FixQuality;
				}
			}
		}

		private int UtmZone;
		private List<Vector3D> UtmPoints = new List<Vector3D>();

		private void GetUtmPoints(IEnumerable<SolutionPoint> points)
		{
			UtmPoints.Clear();
			var filteredPoints = points.Where(p => p.FixQuality == FixQuality).ToArray();
			if (filteredPoints.Count() == 0) return;

			UtmZone = TransverseMercatorProjection.GetUTMZone(filteredPoints.First().Longitude);
			var utm = TransverseMercatorProjection.UTM(UtmZone);

			foreach (var pt in filteredPoints)
			{
				var utmPt = utm.LatLngToXY(pt.Latitude, pt.Longitude);
				UtmPoints.Add(new Vector3D(utmPt, pt.Elevation));
			}
		}

		private Vector3D GetMean()
		{
			Vector3D averagePt = new Vector3D();
			foreach (var pt in UtmPoints)
			{
				averagePt += pt / UtmPoints.Count;
			}
			return averagePt;
		}

		private Vector3D GetStandardDeviation()
		{
			var averagePt = GetMean();
			Vector3D stdDev = new Vector3D();

			foreach (var pt in UtmPoints)
			{
				var diff = (pt - averagePt);
				diff.X *= diff.X;
				diff.Y *= diff.Y;
				diff.Z *= diff.Z;

				stdDev += diff / UtmPoints.Count;
			}

			stdDev.X = Math.Sqrt(stdDev.X);
			stdDev.Y = Math.Sqrt(stdDev.Y);
			stdDev.Z = Math.Sqrt(stdDev.Z);

			return stdDev;
		}

		private List<Vector3D> GetNonOutlyingPoints(double stdDevFactor = 3)
		{
			var selectedPoints = new List<Vector3D>();
			var averagePt = GetMean();
			var stdDev = GetStandardDeviation();

			foreach (var pt in UtmPoints)
			{
				if (Math.Abs(pt.X - averagePt.X) > stdDevFactor * stdDev.X) continue;
				if (Math.Abs(pt.Y - averagePt.Y) > stdDevFactor * stdDev.Y) continue;
				if (Math.Abs(pt.Z - averagePt.Z) > stdDevFactor * stdDev.Z) continue;

				selectedPoints.Add(pt);
			}

			return selectedPoints;
		}

		public AveragedSolutionPoint(IEnumerable<SolutionPoint> points)
		{

			GetFixQuality(points);
			GetUtmPoints(points);
			if (UtmPoints.Count == 0) return;
			UtmPoints = GetNonOutlyingPoints(3);

			var meanPt = GetMean();
			var stdDev = GetStandardDeviation();
			var ll = TransverseMercatorProjection.UTM(UtmZone).XYToLatLon(meanPt.ProjectionXY);

			this.Latitude = Math.Round(ll.Latitude, 9);
			this.Longitude = Math.Round(ll.Longitude, 9);
			this.Elevation = Math.Round(meanPt.Z, 3);
			this.SDE = Math.Round(stdDev.X, 4);
			this.SDN = Math.Round(stdDev.Y, 4);
			this.SDU = Math.Round(stdDev.Z, 4);
			this.PointCount = UtmPoints.Count;
		}
	}
}
