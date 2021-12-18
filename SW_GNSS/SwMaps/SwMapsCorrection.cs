using SW_GNSS.Processing;
using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwCad.Projections;

namespace SW_GNSS.SwMaps
{
	class SwMapsCorrection
	{
		SwMapsProject Project;
		GnssSolutionSet SolutionSet;

		public SwMapsCorrection(SwMapsProject project, GnssSolutionSet solution)
		{
			Project = project;
			SolutionSet = solution;
		}

		public List<SwMapsCorrectionPoint> GetCorrections()
		{
			var ret = new List<SwMapsCorrectionPoint>();

			foreach (var f in Project.Features)
			{
				var layerName = Project.GetLayer(f.LayerID).Name;
				foreach (var pt in f.Points)
				{
					if (pt.FixID == 0) continue;

					var corrPt = new SwMapsCorrectionPoint()
					{
						PointUUID = pt.ID,
						FeatureID = f.UUID,
						Index = pt.Seq,
						LayerName = layerName,
						FieldLatitude = pt.Latitude,
						FieldLongitude = pt.Longitude,
						FieldElevation = pt.Elevation,
						FieldOrthoHeight = pt.OrthoHeight,
						Time = pt.Time,
						StartTime = pt.StartTime,
						InstrumentHeight = pt.InstrumentHeight
					};

					corrPt.FeatureName = string.IsNullOrWhiteSpace(f.Name) ? f.FeatureID.ToString() : f.Name;
					var solPt = SolutionSet.GetBestSolutionPoint(pt.StartTime, pt.Time);
					if (solPt == null) continue;

					corrPt.ProcessedLatitude = solPt.Latitude;
					corrPt.ProcessedLongitude = solPt.Longitude;
					corrPt.ProcessedElevation = solPt.Elevation - corrPt.InstrumentHeight;
					corrPt.AverageCount = solPt.PointCount;

					corrPt.SDE = solPt.SDE;
					corrPt.SDN = solPt.SDN;
					corrPt.SDU = solPt.SDU;

					if (corrPt.FieldOrthoHeight != 0)
					{
						corrPt.ProcessedOrthoHeight = corrPt.FieldOrthoHeight - corrPt.FieldElevation + corrPt.ProcessedElevation;
					}
					corrPt.FixID = solPt.FixQuality.NmeaFixQuality();

					var proj = TransverseMercatorProjection.UTMLon(corrPt.FieldLongitude);
					var fieldUtm = proj.LatLngToXY(corrPt.FieldLatitude, corrPt.FieldLongitude);
					var corrUtm = proj.LatLngToXY(corrPt.ProcessedLatitude, corrPt.ProcessedLongitude);

					corrPt.DeltaE = Math.Round(corrUtm.X - fieldUtm.X, 3);
					corrPt.DeltaN = Math.Round(corrUtm.Y - fieldUtm.Y, 3);
					corrPt.DeltaU = Math.Round(corrPt.ProcessedElevation - corrPt.FieldElevation, 3);

					ret.Add(corrPt);
				}
			}

			return ret;
		}
	}
}
