using SW_GNSS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwCad.TileLayers
{
	public class UrlTileProvider
	{

		public static UrlTileProvider GoogleMap = new UrlTileProvider("https://mts{i}.google.com/vt/lyrs=m@186112443&hl=en&src=app&x={x}&y={y}&z={z}&s=Galile", "GoogleMapTiles.db",21);
		public static UrlTileProvider GoogleSatellite = new UrlTileProvider("https://mts{i}.google.com/vt/lyrs=s@186112443&hl=en&src=app&x={x}&y={y}&z={z}&s=Galile", "GoogleSatelliteTiles.db", 21);
		public static UrlTileProvider GoogleHybrid = new UrlTileProvider("https://mts{i}.google.com/vt/lyrs=y&x={x}&y={y}&z={z}&hl=en&s=Galile", "GoogleHybridTiles.db", 21);
		public static UrlTileProvider GoogleTerrain = new UrlTileProvider("https://mts{i}.google.com/vt/lyrs=r&x={x}&y={y}&z={z}&s=Galile&hl=en&apistyle=s.t%3A3", "GoogleTerrainTiles.db", 21);

		public static UrlTileProvider BingMap = new UrlTileProvider("http://t{i}.tiles.virtualearth.net/tiles/r{q}.jpeg?g=854&mkt=en-US&token=Anz84uRE1RULeLwuJ0qKu5amcu5rugRXy1vKc27wUaKVyIv1SVZrUjqaOfXJJoI0", "BingMapTiles.db", 19);
		public static UrlTileProvider BingSatellite = new UrlTileProvider("http://t{i}.tiles.virtualearth.net/tiles/a{q}.jpeg?g=854&mkt=en-US&token=Anz84uRE1RULeLwuJ0qKu5amcu5rugRXy1vKc27wUaKVyIv1SVZrUjqaOfXJJoI0", "BingSatelliteTiles.db", 19);
		public static UrlTileProvider BingHybrid = new UrlTileProvider("http://t{i}.tiles.virtualearth.net/tiles/h{q}.jpeg?g=854&mkt=en-US&token=Anz84uRE1RULeLwuJ0qKu5amcu5rugRXy1vKc27wUaKVyIv1SVZrUjqaOfXJJoI0", "BingHybridTiles.db", 19);

		public static UrlTileProvider OpenStreetMap = new UrlTileProvider("https://{a}.tile.openstreetmap.org/{z}/{x}/{y}.png", "OSM.db", 21);


		const int MAX_DOWNLOADS = 16;
		Queue<Tile> DownloadQueue = new Queue<Tile>();


		private static string TileXYToQuadKey(int tileX, int tileY, int Z)
		{
			StringBuilder quadKey = new StringBuilder();
			for (int i = Z; i > 0; i--)
			{
				char digit = '0';
				int mask = 1 << (i - 1);
				if ((tileX & mask) != 0)
				{
					digit++;
				}
				if ((tileY & mask) != 0)
				{
					digit++;
					digit++;
				}
				quadKey.Append(digit);
			}
			return quadKey.ToString();
		}

		public event EventHandler<List<Tile>> TileReady;

		public string Url { get; set; }
		int i = 0;
		char a = 'a';
		public virtual string GetTileUrl(int X, int Y, int Z)
		{
			int minusY = (int)Math.Pow(2, Z) - Y - 1;
			var ret = Url;
			var qkey = TileXYToQuadKey(X, Y, Z);
			ret = ret.Replace("{x}", X.ToString());
			ret = ret.Replace("{y}", Y.ToString());
			ret = ret.Replace("{z}", Z.ToString());
			ret = ret.Replace("{q}", qkey.ToString());
			ret = ret.Replace("{i}", i.ToString());
			ret = ret.Replace("{-y}", minusY.ToString());
			ret = ret.Replace("{a}", a.ToString());
			i++;
			if (i >= 4) i = 0;

			a++;
			if (a == 'd') a = 'a';

			return ret;
		}
		public string CachePath { get; set; }
		public int MaxZoomLevel { get; set; }

		TileDownloader Downloader;
		TileCache Cache;

		string CacheDir =>Path.Combine(Workspace.AppTempDir, "TileCache");
		public UrlTileProvider(string url, string cacheFile, int maxZoom)
		{
			Url = url;
			MaxZoomLevel = maxZoom;
			CachePath = cacheFile;
			System.IO.Directory.CreateDirectory(CacheDir);

			Cache = new TileCache(Path.Combine(CacheDir,CachePath));
			Downloader = new TileDownloader();
		}

		public byte[] GetTile(int x,int y, int z)
		{
			var ti = Cache.GetTile(x,y,z);
			if (ti != null) return ti;
			return Downloader.DownloadTile(GetTileUrl(x,y,z));
		}

		public async Task< Tile> GetTileAsync(Tile tl, CancellationToken ct)
		{
			Tile ti = await Cache.GetTileAsync(tl.X, tl.Y, tl.Z);
			if (ti != null) return ti;
			return await Downloader.DownloadTileAsync(GetTileUrl(tl.X, tl.Y, tl.Z), tl, ct);
		}

		public async Task RequestTiles(List<Tile> tlist, CancellationToken ct, bool download)
		{
			List<Task<Tile>> DownloadTasks = new List<Task<Tile>>();
			List<Tile> ToDownload = new List<Tile>();
			List<Tile> CachedTiles = new List<Tile>();
			TileDownloader.HasWebException = false;

			foreach (Tile tl in tlist)
			{
				try
				{
					if (ct.IsCancellationRequested) return;
					Tile ti = await Cache.GetTileAsync(tl.X, tl.Y, tl.Z);
					if (ti != null)
					{
						CachedTiles.Add(ti);
						if (CachedTiles.Count == 2)
						{
							TileReady?.Invoke(this, CachedTiles);
							CachedTiles.Clear();
						}
					}
					else
					{
						ToDownload.Add(tl);
					}
				}
				catch { }
			}
			TileReady?.Invoke(this, CachedTiles);
			if (!download) return;
			try
			{
				foreach (Tile tl in ToDownload)
				{
					if (ct.IsCancellationRequested) return;

					DownloadTasks.Add(Downloader.DownloadTileAsync(GetTileUrl(tl.X, tl.Y, tl.Z), tl, ct));
					if (DownloadTasks.Count == MAX_DOWNLOADS)
					{
						try
						{
							//Task.WaitAll(DownloadTasks.ToArray());
							await Task.Run(() => Task.WaitAll(DownloadTasks.ToArray()));
							List<Tile> tiles = (from tx in DownloadTasks select tx.Result).ToList();
							TileReady?.Invoke(this, tiles);
							await Cache.SaveTilesAsync(tiles);
							DownloadTasks.Clear();
						}
						catch { }
					}

				}

				if (ct.IsCancellationRequested) return;
				try
				{
					await Task.Run(() => Task.WaitAll(DownloadTasks.ToArray()));
					//Task.WaitAll(DownloadTasks.ToArray());

				}
				catch { }


				List<Tile> tiles1 = (from tx in DownloadTasks select tx.Result).ToList();
				TileReady?.Invoke(this, tiles1);
				await Cache.SaveTilesAsync(tiles1);

				DownloadTasks.Clear();
			}
			catch (TaskCanceledException)
			{
				return;
			}


		}

	}
}
