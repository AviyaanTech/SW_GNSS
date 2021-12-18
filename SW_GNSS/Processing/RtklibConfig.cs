using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_GNSS
{
	class RtklibConfig
	{
		public static string ConfigOutFile;
		static RtklibConfig()
		{
			ConfigOutFile = Path.Combine(Path.GetTempPath(), "SWGNSS", "rtklib.conf");
			Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "SWGNSS"));
		}


		public string ProcessingMode = "kinematic";
		public string ARMode = "fix-and-hold";

		public void WritePPPConfig(ProcessingConfig config, string dcbDir)
		{
			var dcbFile = Path.Combine(dcbDir, "*.DCB");

			var snrMaskString = "";

			for (int i = 0; i < 9; i++)
			{
				if (snrMaskString != "") snrMaskString += ",";
				snrMaskString += config.SnrMask;
			}

			var configFile = File.ReadAllText("./RTKLIB/ppp_template.conf");
			configFile = configFile.Replace("{posmode}", ProcessingMode);
			configFile = configFile.Replace("{elmask}", config.ElevationMask.ToString());
			configFile = configFile.Replace("{snrmask}", snrMaskString);

			configFile = configFile.Replace("{rover_snrmask}", (config.SnrMask == 0) ? "off" : "on");
			configFile = configFile.Replace("{base_snrmask}", (config.SnrMask == 0) ? "off" : "on");

			configFile = configFile.Replace("{navsys}", config.NavSys.ToString());
			configFile = configFile.Replace("{armode}", ARMode);
			configFile = configFile.Replace("{glo_ar}", "fix-and-hold");

			configFile = configFile.Replace("{solstatic}", config.StaticSingle ? "single" : "all");

			configFile = configFile.Replace("{antFile}", Path.GetFullPath(@"./RTKLIB/ngs14.atx"));
			configFile = configFile.Replace("{dcbFile}", dcbFile);

			File.WriteAllText(ConfigOutFile, configFile);
		}


		public void WriteConfig(ProcessingConfig config)
		{
			var snrMaskString = "";

			for (int i = 0; i < 9; i++)
			{
				if (snrMaskString != "") snrMaskString += ",";
				snrMaskString += config.SnrMask;
			}

			var configFile = File.ReadAllText("./RTKLIB/template.conf");
			configFile = configFile.Replace("{posmode}", ProcessingMode);
			configFile = configFile.Replace("{elmask}", config.ElevationMask.ToString());
			configFile = configFile.Replace("{snrmask}", snrMaskString);

			configFile = configFile.Replace("{rover_snrmask}", (config.SnrMask == 0) ? "off" : "on");
			configFile = configFile.Replace("{base_snrmask}", (config.SnrMask == 0) ? "off" : "on");

			if (ProcessingMode == "single")
			{
				configFile = configFile.Replace("{ionoopt}", "dual-freq");
			}
			else
			{
				configFile = configFile.Replace("{ionoopt}", "brdc");
			}
			configFile = configFile.Replace("{navsys}", config.NavSys.ToString());
			configFile = configFile.Replace("{armode}", ARMode);
			configFile = configFile.Replace("{glo_ar}", "fix-and-hold");

			configFile = configFile.Replace("{solstatic}", config.StaticSingle ? "single" : "all");

			configFile = configFile.Replace("{antRover}", config.RoverAntennaModel);
			configFile = configFile.Replace("{antRoverE}", config.RoverDeltaE.ToString("0.0000"));
			configFile = configFile.Replace("{antRoverN}", config.RoverDeltaN.ToString("0.0000"));
			configFile = configFile.Replace("{antRoverU}", config.RoverDeltaU.ToString("0.0000"));

			configFile = configFile.Replace("{antBase}", config.BaseAntennaModel);
			configFile = configFile.Replace("{antBaseE}", config.BaseDeltaE.ToString("0.0000"));
			configFile = configFile.Replace("{antBaseN}", config.BaseDeltaN.ToString("0.0000"));
			configFile = configFile.Replace("{antBaseU}", config.BaseDeltaU.ToString("0.0000"));


		

			configFile = configFile.Replace("{baseLat}", config.BaseLatitude.ToString());
			configFile = configFile.Replace("{baseLon}", config.BaseLongitude.ToString());
			configFile = configFile.Replace("{baseElv}", config.BaseElevation.ToString());
			configFile = configFile.Replace("{antFile}", Path.GetFullPath(@"./RTKLIB/ngs14.atx"));

			File.WriteAllText(ConfigOutFile, configFile);
		}
	}

}
