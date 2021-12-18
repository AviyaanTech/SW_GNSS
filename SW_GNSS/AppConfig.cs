using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_GNSS
{
	public static class AppConfig
	{
		public static string TempPath { get; private set; }
		public static string TempSwMapsPath { get; private set; }
		public static string TempBasePath { get; private set; }
		public static string TempRoverPath { get; private set; }
		public static string CorsPath { get; private set; }

		public static List<string> Antennas = new List<string>();

		public static void Initialize()
		{
			ReadAntennaFile();
		}

		static void ReadAntennaFile()
		{
			var lines = System.IO.File.ReadAllLines("RTKLIB/ngs14.atx");

			var toExclude = new string[] { "BLOCK", "GLONASS", "GALILEO", "QZSS", "BEIDOU", "IRNSS" };
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Contains("TYPE / SERIAL NO"))
				{
					var ant = lines[i].Split(' ')[0];
					var include = true;
					foreach (var s in toExclude)
					{
						if (ant.Contains(s))
						{
							include = false;
							break;
						}
					}
					if (include)
						Antennas.Add(ant);
				}
			}
		}

	}
}
