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
	class DcbDownloader
	{
		string BaseUrl = "http://ftp.aiub.unibe.ch/CODE";
		public event EventHandler<string> ReportProgress;

		public async Task<string> Download(DateTime dt)
		{
			int year = dt.Year;
			int month = dt.Month;
			for(int i = 0; i <= 6; i++)
			{
				month -= 1;
				if(month == 0)
				{
					month = 12;
					year -= 1;
				}

				var ret = await DownloadDcb(year, month);
				if (ret != null) return ret;
			}

			return null;
		}

		private async Task<string> DownloadDcb(int y, int m)
		{
			var fileNames = new List<string>();
			var year = y.ToString("0000");
			var year2D = (y % 100).ToString("00");
			var month2D = m.ToString("00");

			fileNames.Add($"P1C1{year2D}{month2D}.DCB");
			fileNames.Add($"P1P2{year2D}{month2D}_ALL.DCB");
			fileNames.Add($"P2C2{year2D}{month2D}_RINEX.DCB");

			var dir = Path.Combine(Workspace.DCBDir, year + month2D);
			Directory.CreateDirectory(dir);

			foreach (var f in fileNames)
			{
				var dcbDestination = Path.Combine(dir, f);
				var zipDestination = dcbDestination + ".Z";

				if (File.Exists(dcbDestination) == true) continue;

				try
				{
					var url = $"{BaseUrl}/{year}/{f}.Z";

					using var wc = new WebClient();

					await wc.DownloadFileTaskAsync(new Uri(url), zipDestination, 0, new Progress<Tuple<long, int, long>>(t =>
					{
						Log("Downloading " + f + " " + t.Item2 + "%");
					}));

					//extract 
					using (var inputFileStream = new FileStream(zipDestination, FileMode.Open))
					using (var gzipStream = new LzwInputStream(inputFileStream))
					using (var outputFileStream = new FileStream(dcbDestination, FileMode.Create))
					{
						gzipStream.CopyTo(outputFileStream);
					}

				}
				catch
				{
					return null;
				}
			}

			return dir;
		}

		private void Log(string message)
		{
			ReportProgress?.Invoke(this, message);
			Console.WriteLine(message);
		}
	}
}
