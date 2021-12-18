using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwCad.TileLayers
{ 
	public class TileDownloader
	{
		public static bool HasWebException = false;

		public byte[] DownloadTile(string url)
		{

			try
			{
				using (var client = new MyWebClient())
				{
					client.Proxy = null;
					client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
					var result = client.DownloadData(url);
					if (result != null)
					{
						return result;
					}
				}
			}
			catch (Exception ex)
			{
			}
			return null;
		}


		public async Task<Tile> DownloadTileAsync(string url, Tile t, CancellationToken ct)
		{
			if (HasWebException) return null;

			Console.WriteLine($"Tile request {t.X},{t.Y},{t.Z}");
			try
			{
				using (var client = new MyWebClient())
				{
					client.Proxy = null;
					client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

					var result = await client.DownloadDataTaskAsync(url);
					if (result != null)
					{
						return new Tile(t.X, t.Y, t.Z, result);
					}
				}
			}
			catch (Exception ex)
			{
				HasWebException = true;
				//Console.WriteLine(ex.Message);
			}
			return null;
		}
	}

	class MyWebClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri uri)
		{
			WebRequest w = base.GetWebRequest(uri);
			w.Timeout = 5000;
			return w;
		}
	}

}
