using SkiaSharp;
using SW_GNSS.Map;
using SwCad.Primitives;
using SwCad.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwCad.TileLayers
{
	public class OnlineTileLayer : BaseTileLayer
	{
		UrlTileProvider provider;

		public event EventHandler Updated;

		Task DownloadTask;
		CancellationTokenSource cts = new CancellationTokenSource();

		public override BoundingBox ImageExtents
		{
			get
			{
				var ws = WebMercatorProjection.WorldSize;
				var p1 = new Vector2D(-ws, -ws);
				var p2 = new Vector2D(ws, ws);
				return BoundingBox.FromPoints(p1, p2);
			}
		}

		public OnlineTileLayer(MapView view, string layer, UrlTileProvider prov, bool active) : base(view, layer)
		{
			tiles = new List<Tile>();
			provider = prov;
			provider.TileReady += TileReady;
			Visible = active;
			IsOnline = true;
		}

		public void SetProvider(UrlTileProvider prov)
		{
			provider = prov;
			provider.TileReady += TileReady;
			tiles.Clear();
			Update(true);
		}

		public UrlTileProvider GetProvider()
		{
			return provider;
		}

		private void TileReady(object sender, List<Tile> tile)
		{
			lock (tiles)
			{
				foreach (Tile t in tile)
				{
					if (t == null) continue;
					if (!tiles.Contains(t)) tiles.Add(t);
				}
			}
			Updated?.Invoke(this, null);
			View.Redraw();
		}


		public override void Update(bool download)
		{
			if (View.Extents == null) return;
			cts.Cancel();
			cts = new CancellationTokenSource();
			if (!Visible) return;

			List<Tile> req = GetRequiredTiles(provider.MaxZoomLevel);
			View.IsLoadingRaster = true;
			DownloadTask = provider.RequestTiles(req, cts.Token, download);
			DownloadTask.ContinueWith((t) =>
			{
				Cleanup();
				View.IsLoadingRaster = false;
			});
		}

		public override async Task<Tile> GetTileAsync(int x, int y, int z, CancellationToken ct)
		{
			var tx = await provider.GetTileAsync(new Tile(x, y, z), ct);
			return tx;
		}

		public override byte[] GetTileBitmap(int x, int y, int z)
		{
			return provider.GetTile(x, y, z);
		}
	}
}
