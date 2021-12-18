using SwCad.Projections;
using SwMapsLib.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SW_GNSS.Processing
{
	internal class GnssProcessing
	{
		public static CancellationTokenSource TokenSource = null;
		private CancellationToken Token;

		public event EventHandler<string> ReportProgress;

		public List<GnssInputFile> RoverFiles;
		public List<GnssInputFile> BaseFiles;

		private List<string> InputRoverObsFiles;
		private List<string> InputBaseObsFiles;
		private List<string> InputRinexNavFiles;
		private List<string> InputPPPFiles;
		private string DcbDir;

		public string ProcessingMode { get; set; } = "kinematic";
		public SwMapsProject SwMapsProject { get; set; }
		public bool UseSwMapsRawFiles { get; set; }

		ProcessingConfig config;

		public List<SolutionFile> SolutionFiles = new List<SolutionFile>();

		private void Log(string message)
		{
			ReportProgress?.Invoke(this, message);
			Console.WriteLine(message);
		}

		public async Task<bool> ProcessAsync()
		{
			TokenSource = new CancellationTokenSource();
			Token = TokenSource.Token;
			InputRoverObsFiles = new List<string>();
			InputBaseObsFiles = new List<string>();
			InputRinexNavFiles = new List<string>();
			InputPPPFiles = new List<string>();
			config = ProcessingConfig.LoadConfig();

			try
			{
				if (UseSwMapsRawFiles)
				{
					RoverFiles.Clear();
					foreach (var f in SwMapsProject.GnssRawDataFiles)
					{
						var fType = "";
						if (Path.GetExtension(f).ToUpper() == ".UBX") fType = "UBX";
						if (Path.GetExtension(f).ToUpper() == ".SBF") fType = "SBF";
						if (fType == "") continue;

						var inp = new GnssInputFile();
						inp.FileType = fType;
						inp.FullPath = f;
						inp.Name = Path.GetFileName(f);
					}
				}

				if (RoverFiles.Count == 0)
				{
					Workspace.ShowErrorBox("No Rover Files Found!");
					return false;
				}

				await ConvertRoverFiles();

				if (ProcessingMode.StartsWith("ppp"))
				{
					var ret = await GetPPPFiles();
					if (ret == false) return false;



				}
				else if (ProcessingMode != "single")
				{
					if (BaseFiles.Count == 0)
					{
						Workspace.ShowErrorBox("No Base Files Found!");
						return false;
					}
					await ConvertBaseFiles();
					if (config.UseRinexHeaderPosition && InputBaseObsFiles.Count > 0)
					{
						var baseObs = new RinexObsFile(InputBaseObsFiles.First());
						var llhPos = Ellipsoid.WGS84.ToLatLngHt(baseObs.ApproxPosition);
						config.BaseLatitude = llhPos.Latitude;
						config.BaseLongitude = llhPos.Longitude;
						config.BaseElevation = llhPos.Elevation;
						config.SaveConfig();
					}

				}

				var conf = new RtklibConfig();
				if (ProcessingMode == "ppp-kinematic")
					conf.ProcessingMode = "ppp-kine";
				else
					conf.ProcessingMode = ProcessingMode;

				if (ProcessingMode.StartsWith("ppp"))
					conf.WritePPPConfig(config, DcbDir);
				else
					conf.WriteConfig(config);

				var proc = new RtklibProcessing();
				proc.ReportProgress += ProgressListener;
				foreach (var rover in InputRoverObsFiles)
				{
					if (ProcessingMode.StartsWith("ppp"))
					{
						var outFile = await proc.ProcessAsync(rover, InputPPPFiles, InputRinexNavFiles, Token);
						SolutionFiles.Add(new SolutionFile(outFile));
					}
					else
					{
						var outFile = await proc.ProcessAsync(rover, InputBaseObsFiles, InputRinexNavFiles, Token);
						SolutionFiles.Add(new SolutionFile(outFile));
					}
				}

				TokenSource = null;
				return true;
			}
			catch (Exception ex)
			{
				TokenSource.Cancel();
				TokenSource = null;
				return false;
			}
		}


		private async Task<bool> GetPPPFiles()
		{
			var downloader = new PppDownloader();
			downloader.ReportProgress += ProgressListener;

			DateTime firstObs = DateTime.Now;
			foreach (var obs in InputRoverObsFiles)
			{
				var f = new RinexObsFile(obs);
				firstObs = f.FirstObservationTime;

				var files = await downloader.Download(f.FirstObservationTime, f.LastObservationTime);
				if (files == null)
				{
					Workspace.ShowErrorBox("PPP Data download failed!");
					return false;
				}

				InputPPPFiles.AddRange(files);
			}

			var dcbDownloader = new DcbDownloader();
			dcbDownloader.ReportProgress += ProgressListener;
			DcbDir = await dcbDownloader.Download(firstObs);

			if (DcbDir == null)
			{
				Workspace.ShowErrorBox("Failed to download DCB data!");
				return false;
			}

			return true;
		}
		private async Task ConvertRoverFiles()
		{
			Log("Converting Rover Files");

			foreach (var rv in RoverFiles)
			{
				if (rv.FileType == "RINEX OBS")
				{
					InputRoverObsFiles.Add(rv.FullPath);
				}
				else if (rv.FileType == "RINEX NAV")
				{
					InputRinexNavFiles.Add(rv.FullPath);
				}
				else
				{
					var converter = new RinexConverter();
					converter.AntennaModel = config.RoverAntennaModel;
					converter.AntennaDelta = config.RoverDelta;
					converter.ReportProgress += ProgressListener;
					converter.Format = rv.FileType.ToLower();
					converter.FilePath = rv.FullPath;

					var fileBaseName = Path.GetFileNameWithoutExtension(rv.FullPath);
					var outputDir = Path.GetDirectoryName(rv.FullPath);
					outputDir = Path.Combine(outputDir, fileBaseName + "_RINEX");


					var obsPath = Path.Combine(outputDir, fileBaseName + ".obs");
					var navPath = Path.Combine(outputDir, fileBaseName + ".nav");

					Directory.CreateDirectory(outputDir);
					if (File.Exists(navPath) && new FileInfo(navPath).Length > 100)
						InputRinexNavFiles.Add(navPath);

					if (File.Exists(obsPath) && new FileInfo(obsPath).Length > 100)
					{
						InputRoverObsFiles.Add(obsPath);
						continue;
					}

					converter.OutputDir = outputDir;

					await converter.ConvertToRinex(Token);


					if (File.Exists(navPath))
						InputRinexNavFiles.Add(navPath);

					if (File.Exists(obsPath))
						InputRoverObsFiles.Add(obsPath);


				}
			}
		}

		private async Task ConvertBaseFiles()
		{
			//Convert Base Files
			Log("Converting Base Files");

			foreach (var rv in BaseFiles)
			{
				if (rv.FileType == "RINEX OBS")
				{
					InputBaseObsFiles.Add(rv.FullPath);
				}
				else if (rv.FileType == "RINEX NAV")
				{
					InputRinexNavFiles.Add(rv.FullPath);
				}
				else
				{
					var converter = new RinexConverter();
					converter.AntennaModel = config.BaseAntennaModel;
					converter.AntennaDelta = config.BaseDelta;
					converter.ReportProgress += ProgressListener;
					converter.Format = rv.FileType.ToLower();
					converter.FilePath = rv.FullPath;

					var fileBaseName = Path.GetFileNameWithoutExtension(rv.FullPath);
					var outputDir = Path.GetDirectoryName(rv.FullPath);
					outputDir = Path.Combine(outputDir, fileBaseName + "_RINEX");

					converter.OutputDir = outputDir;

					await converter.ConvertToRinex(Token);

					var obsPath = Path.Combine(outputDir, fileBaseName + ".obs");
					var navPath = Path.Combine(outputDir, fileBaseName + ".nav");
					if (File.Exists(obsPath))
						InputBaseObsFiles.Add(obsPath);

					if (File.Exists(navPath))
						InputRinexNavFiles.Add(navPath);
				}
			}
		}




		private void ProgressListener(object sender, string e)
		{
			Log(e);
		}
	}
}
