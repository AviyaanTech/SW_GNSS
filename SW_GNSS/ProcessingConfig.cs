using Newtonsoft.Json;
using SwCad.Primitives;
using SwCad.Projections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_GNSS
{
	public class ProcessingConfig
	{
		public int ElevationMask { get; set; } = 10;
		public int SnrMask { get; set; } = 0;

		public double BaseLatitude { get; set; }
		public double BaseLongitude { get; set; }
		public double BaseElevation { get; set; }

		public double BaseDeltaE { get; set; }
		public double BaseDeltaN { get; set; }
		public double BaseDeltaU { get; set; }

		public double RoverDeltaE { get; set; }
		public double RoverDeltaN { get; set; }
		public double RoverDeltaU { get; set; }

		public string BaseAntennaModel { get; set; } = "TWIVP6050_CONE";
		public string RoverAntennaModel { get; set; } = "";

		public bool UseRinexHeaderPosition { get; set; } = false;

		[JsonIgnore]
		public Vector3D RoverDelta => new Vector3D(RoverDeltaE, RoverDeltaN, RoverDeltaU);
		
		[JsonIgnore]
		public Vector3D BaseDelta => new Vector3D(BaseDeltaE, BaseDeltaN, BaseDeltaU);
		
		[JsonIgnore]
		public Vector3D BaseECEF => Ellipsoid.WGS84.ToEcefCoordinate(BaseLatitude, BaseLongitude, BaseElevation);

		public bool EnableGPS { get; set; } = true;
		public bool EnableGLO { get; set; } = true;
		public bool EnableGAL { get; set; } = true;
		public bool EnableQZS { get; set; } = true;
		public bool EnableBDS { get; set; } = true;
		public bool EnableSBAS { get; set; } = true;
		public bool EnableIRNSS { get; set; } = true;
		public bool StaticSingle { get; set; } = true;

		[JsonIgnore]
		public int NavSys
		{
			get
			{
				var ret = 0;
				if (EnableGPS) ret += 1;
				if (EnableSBAS) ret += 2;
				if (EnableGLO) ret += 4;
				if (EnableGAL) ret += 8;
				if (EnableQZS) ret += 16;
				if (EnableBDS) ret += 32;
				if (EnableIRNSS) ret += 64;
				return ret;
			}
		}

		public void SaveConfig()
		{
			var fileName = Path.Combine(Path.GetTempPath(), "SWGNSS", "config.json");
			var json = JsonConvert.SerializeObject(this);
			File.WriteAllText(fileName,json);
		}

		public static ProcessingConfig LoadConfig()
		{
			var fileName = Path.Combine(Path.GetTempPath(), "SWGNSS", "config.json");
			if (File.Exists(fileName) == false)
			{
				var ret = new ProcessingConfig();
				ret.SaveConfig();
				return ret;
			}
			var json = File.ReadAllText(fileName);
			return JsonConvert.DeserializeObject<ProcessingConfig>(json);
		}


	}
}
