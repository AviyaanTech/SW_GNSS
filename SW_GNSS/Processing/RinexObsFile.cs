using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_GNSS.Processing
{
	class RinexObsFile
	{
		public string FilePath { get; private set; }
		public string Version { get; set; }
		public Vector3D ApproxPosition { get; private set; } // Approximate position in ECEF
		public Vector3D AntennaDelta { get; private set; } // Antenna Delta, ENU
		public DateTime FirstObservationTime { get; private set; }
		public DateTime LastObservationTime { get; private set; }
		public string AntennaType { get; set; }

		public RinexObsFile(string path)
		{
			FilePath = path;
			var reader = new StreamReader(path);
			string line = "";


			while (line.Trim() != "END OF HEADER" || reader.EndOfStream)
			{
				line = reader.ReadLine();
				var entries = line.Trim().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
				var headerLabel = line.Substring(60).Trim();

				if (headerLabel == "RINEX VERSION / TYPE")
					Version = entries[0];
				else if (headerLabel == "APPROX POSITION XYZ")
				{
					var x = Convert.ToDouble(entries[0]);
					var y = Convert.ToDouble(entries[1]);
					var z = Convert.ToDouble(entries[2]);
					ApproxPosition = new Vector3D(x, y, z);
				}
				else if (headerLabel == "ANTENNA: DELTA H/E/N")
				{
					var u = Convert.ToDouble(entries[0]);
					var e = Convert.ToDouble(entries[1]);
					var n = Convert.ToDouble(entries[2]);
					AntennaDelta = new Vector3D(e, n, u);
				}
				else if (headerLabel == "ANT # / TYPE")
				{
					AntennaType = entries[0];
				}
				else if (headerLabel == "TIME OF FIRST OBS" || headerLabel == "TIME OF LAST OBS")
				{
					var year = Convert.ToInt32(entries[0]);
					var month = Convert.ToInt32(entries[1]);
					var day = Convert.ToInt32(entries[2]);
					var hour = Convert.ToInt32(entries[3]);
					var minutes = Convert.ToInt32(entries[4]);
					var seconds = Convert.ToDouble(entries[5]);
					var s = (int)Math.Floor(seconds);
					var ms = Convert.ToInt32((seconds - s) * 1000);
					if (headerLabel == "TIME OF FIRST OBS") 
						FirstObservationTime = new DateTime(year, month, day, hour, minutes, s, ms,DateTimeKind.Utc);
					else
						LastObservationTime = new DateTime(year, month, day, hour, minutes, s, ms, DateTimeKind.Utc);

				}
			}

			reader.Close();

		}
	}
}
