using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.IO;

namespace SW_GNSS.Processing
{
	public class SolutionFile
	{
		public string Path { get; private set; }
		public List<SolutionPoint> Points;
		public const string SolutionHeader = "%  UTC                  latitude(deg) longitude(deg)  height(m)   Q  ns   sdn(m)   sde(m)   sdu(m)  sdne(m)  sdeu(m)  sdun(m) age(s)  ratio";
		public SolutionFile(string path)
		{
			Path = path;
			var fileData = File.ReadAllLines(path);
			Points = new List<SolutionPoint>();

			foreach (var l in fileData)
			{
				if (l.StartsWith("%")) continue;
				var data = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				var Date = data[0].Split('/');
				var Time = data[1].Split(':');

				var year = Convert.ToInt32(Date[0]);
				var month = Convert.ToInt32(Date[1]);
				var day = Convert.ToInt32(Date[2]);
				var hour = Convert.ToInt32(Time[0]);
				var minutes = Convert.ToInt32(Time[1]);
				var seconds = Convert.ToDouble(Time[2]);
				var s = (int)Math.Floor(seconds);
				var ms = (int)((seconds - s) * 1000);

				var pt = new SolutionPoint();
				pt.SolutionLine = l;
				pt.Time = new DateTime(year, month, day, hour, minutes, s, ms);

				pt.Latitude = Convert.ToDouble(data[2]);
				pt.Longitude = Convert.ToDouble(data[3]);
				pt.Elevation = Convert.ToDouble(data[4]);
				pt.FixQuality = (FixQuality)Convert.ToInt32(data[5]);
				pt.NumSat = Convert.ToInt32(data[6]);
				pt.SDN = Convert.ToDouble(data[7]);
				pt.SDE = Convert.ToDouble(data[8]);
				pt.SDU = Convert.ToDouble(data[9]);
				pt.Ratio = Convert.ToDouble(data[14]);
				Points.Add(pt);
			}
		}
	}

	public class SolutionPoint
	{
		public DateTime Time;
		public double Latitude;
		public double Longitude;
		public double Elevation;
		public FixQuality FixQuality;
		public int NumSat;
		public double SDN;
		public double SDE;
		public double SDU;
		public double Ratio;
		public string SolutionLine;
	}

	public enum FixQuality
	{
		Fix = 1,
		Float = 2,
		SBAS = 3,
		DGPS = 4,
		Single = 5,
		PPP = 6
	}
}
