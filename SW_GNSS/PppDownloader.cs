using ICSharpCode.SharpZipLib.Lzw;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SW_GNSS
{
	class PppDownloader
	{
		public event EventHandler<string> ReportProgress;

		string BaseUrl = "http://navigation-office.esa.int/products/gnss-products/";

		public DataDownloadSource IgsRapidSP3 = new DataDownloadSource("IGS_RAPID_SP3", "{wn}/esr{wn}{dow}.sp3.Z", 24);
		public DataDownloadSource IgsRapidCLK = new DataDownloadSource("IGS_RAPID_CLK", "{wn}/esr{wn}{dow}.clk.Z", 24);

		public DataDownloadSource IgsFinalSP3 = new DataDownloadSource("IGS_FINAL_SP3", "{wn}/esa{wn}{dow}.sp3.Z", 24);
		public DataDownloadSource IgsFinalCLK = new DataDownloadSource("IGS_FINAL_CLK", "{wn}/esa{wn}{dow}.clk.Z", 24);

		public DataDownloadSource EsocFinalSP3 = new DataDownloadSource("ESOC_FINAL_SP3", "{wn}/ESA0MGNFIN_{yyyy}{doy}0000_01D_05M_ORB.SP3.gz", 24);
		public DataDownloadSource EsocFinalCLK = new DataDownloadSource("ESOC_FINAL_CLK", "{wn}/ESA0MGNFIN_{yyyy}{doy}0000_01D_30S_CLK.CLK.gz", 24);

		public List<string> GetFileUris(DateTime startTime, DateTime endTime, DataDownloadSource source)
		{
			var startTimeUtc = new DateTimeOffset(startTime, TimeSpan.Zero);
			var endTimeUtc = new DateTimeOffset(endTime, TimeSpan.Zero);

			var fileList = new List<string>();
			var dt = startTime;

			var FileLengthSeconds = source.FileLengthHours * 3600;

			var FileStartTime = (long)Math.Floor(startTimeUtc.ToUnixTimeSeconds() / (double)FileLengthSeconds) * FileLengthSeconds;
			var FileEndTime = (long)Math.Ceiling(endTimeUtc.ToUnixTimeSeconds() / (double)FileLengthSeconds) * FileLengthSeconds;

			if (FileStartTime > FileEndTime)
			{
				var temp = FileEndTime;
				FileEndTime = FileStartTime;
				FileStartTime = temp;
			}

			for (long t = FileStartTime; t < FileEndTime; t += (long)FileLengthSeconds)
			{
				var fileDateTime = DateTimeOffset.FromUnixTimeSeconds(t);
				fileList.Add(GetFileName(source.Template, fileDateTime));
			}

			return fileList;
		}

		string GetFileName(string template, DateTimeOffset dt)
		{
			var gpst_seconds = dt.ToUnixTimeSeconds() - 315964800;
			var gps_weekNumber = gpst_seconds / 604800;
			var dayOfWeek = (gpst_seconds - gps_weekNumber * 604800) / 86400;

			var ret = template;
			ret = ret.Replace("{yyyy}", dt.Year.ToString("0000"));
			ret = ret.Replace("{yy}", (dt.Year % 1000).ToString("00"));
			ret = ret.Replace("{doy}", dt.DayOfYear.ToString("000"));
			ret = ret.Replace("{6hi}", (Math.Floor(dt.Hour / 6.0) + 1).ToString("0"));
			ret = ret.Replace("{6h}", (Math.Floor(dt.Hour / 6.0) * 6).ToString("00"));
			ret = ret.Replace("{12hi}", (Math.Floor(dt.Hour / 12.0) + 1).ToString("0"));
			ret = ret.Replace("{12h}", (Math.Floor(dt.Hour / 12.0) * 12).ToString("00"));

			ret = ret.Replace("{wn}", gps_weekNumber.ToString("0000"));
			ret = ret.Replace("{dow}", dayOfWeek.ToString());

			return ret;
		}

		public async Task<List<string>> Download(DateTime startTime, DateTime endTime)
		{
			//try ESOC First
			var ret = await DownloadDataFiles("ESOC", EsocFinalSP3, EsocFinalCLK, startTime, endTime);
			if (ret != null) return ret;

			//Then try IGS Final
			ret = await DownloadDataFiles("IGS_FINAL", IgsFinalSP3, IgsFinalCLK, startTime, endTime);
			if (ret != null) return ret;

			//Then try IGS Rapid
			ret = await DownloadDataFiles("IGS_RAPID", IgsRapidSP3, IgsRapidCLK, startTime, endTime);
			return ret;
		}

		private async Task<List<string>> DownloadDataFiles(string sourceName, DataDownloadSource sp3Source, DataDownloadSource clkSource, DateTime startTime, DateTime endTime)
		{
			var extractedFiles = new List<string>();

			var gzipFiles = new List<string>();
			var fileUris = GetFileUris(startTime, endTime, sp3Source);
			fileUris.AddRange(GetFileUris(startTime, endTime, clkSource));

			foreach (var f in fileUris)
			{
				string diskFile = await DownloadFileHttp(sourceName, f);
				if (diskFile == null) return null;
				gzipFiles.Add(diskFile);
			}

			foreach (var f in gzipFiles)
			{
				var extractedFile = Path.Combine(Workspace.PPPDir, sourceName, "EXTRACTED", Path.GetFileNameWithoutExtension(f));
				Directory.CreateDirectory(Path.GetDirectoryName(extractedFile));

				if (File.Exists(extractedFile)) File.Delete(extractedFile);

				if (f.ToLower().EndsWith(".gz"))
				{
					using (var inputFileStream = new FileStream(f, FileMode.Open))
					using (var gzipStream = new GZipStream(inputFileStream, CompressionMode.Decompress))
					using (var outputFileStream = new FileStream(extractedFile, FileMode.Create))
					{
						gzipStream.CopyTo(outputFileStream);
					}
				}
				else if (f.ToLower().EndsWith(".z"))
				{
					using (var inputFileStream = new FileStream(f, FileMode.Open))
					using (var gzipStream = new LzwInputStream(inputFileStream))
					using (var outputFileStream = new FileStream(extractedFile, FileMode.Create))
					{
						gzipStream.CopyTo(outputFileStream);
					}
				}

				extractedFiles.Add(extractedFile);
			}

			return extractedFiles;
		}

		private async Task<string> DownloadFileHttp(string sourceName, string fileUri)
		{
			var diskDestination = Path.Combine(Workspace.PPPDir, sourceName, "RAW", fileUri);
			var fileName = Path.GetFileName(diskDestination);

			Directory.CreateDirectory(Path.GetDirectoryName(diskDestination));

			if (File.Exists(diskDestination))
			{
				var info = new FileInfo(diskDestination);
				if (info.Length == 0)
				{
					File.Delete(diskDestination);
				}
			}

			if (File.Exists(diskDestination)) return diskDestination;

			try
			{
				var url = $"{BaseUrl}{fileUri}";

				using var wc = new WebClient();

				await wc.DownloadFileTaskAsync(new Uri(url), diskDestination, 0, new Progress<Tuple<long, int, long>>(t =>
				{
					Log("Downloading " + fileName + " " + t.Item2 + "%");
				}));


				return diskDestination;
			}
			catch
			{
				File.Delete(diskDestination);
				return null;
			}
		}

		private void Log(string message)
		{
			ReportProgress?.Invoke(this, message);
			Console.WriteLine(message);
		}
	}


	class DataDownloadSource
	{
		public string Template { get; set; }
		public int FileLengthHours { get; set; }
		public string Name { get; set; }

		public DataDownloadSource(string name, string template, int lengthHours)
		{
			Name = name;
			Template = template;
			FileLengthHours = lengthHours;
		}
	}

}
