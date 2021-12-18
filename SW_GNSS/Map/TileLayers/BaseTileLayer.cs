using SkiaSharp;
using SW_GNSS.Map;
using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwCad.TileLayers
{
	public abstract class BaseTileLayer:RasterLayer
	{
		public List<Tile> tiles=new List<Tile>();

		public int DrawnTileCount;
		protected List<Tile> ToDelete = new List<Tile>();

		public bool IsOnline = false;

		public BaseTileLayer(MapView view, string layer) : base(view, layer)
		{
		}

		protected void Cleanup()
		{
			lock (tiles)
			{
				if (ToDelete.Count == 0) return;
				tiles.RemoveAll((t) => ToDelete.Contains(t));
				ToDelete.Clear();
			}
		}

		public abstract void Update(bool download);

		public override void Draw(SKCanvas c)
		{
			if (!Visible) return;

			lock (tiles)
			{
				DrawnTileCount = 0;

				try
				{
					tiles.RemoveAll((t) => t == null);
					tiles.Sort((t1, t2) => t1.Z.CompareTo(t2.Z));

					foreach (Tile tile in tiles)
					{
						if (View.Extents.Intersects(tile.Rectangle) == false) continue;
						if (tile.Z == View.MapZoomLevel - 1)
						{
							var split = tile.Split();
							int foundCount = 0;
							foreach (Tile t1 in split)
							{
								bool found = false;
								foreach (Tile t in tiles)
								{
									if (t.EqualsTile(t1))
									{
										found = true;
										foundCount += 1;
										break;
									}
								}
								if (!found) break;
							}
							if (foundCount == 4) continue;
						}
						if (tile.Z > View.MapZoomLevel + 1) continue;
						DrawnTileCount++;

						tile.Draw(View, c);
						//c.DrawBitmap(tile.Bitmap, View.Transform.Transform(tile.Rectangle).ToSKRect());

					}
				}
				catch
				{
					Console.WriteLine("Error drawing tile");
				};
			}

			Console.WriteLine($"{DrawnTileCount} Tiles drawn.");
		}

		public abstract Task<Tile> GetTileAsync(int x, int y, int z, CancellationToken ct);

		public abstract byte[] GetTileBitmap(int x, int y, int z);
		/// <summary>
		/// Updates the tiles to delete, and returns the new tiles required
		/// </summary>
		/// <returns></returns>
		protected List<Tile> GetRequiredTiles(int maxZoomLevel)
		{
			ToDelete.Clear();
			if (!Visible) return null;

			List<Tile> req = new List<Tile>();

			var extents = View.Extents;

			Vector2D topleft = new Vector2D(extents.MinX, extents.MaxY);
			Vector2D bottomRight = new Vector2D(extents.MaxX, extents.MinY);

			int k = Math.Min(View.MapZoomLevel, maxZoomLevel);


			Tile t1 = Tile.XYtoTile(topleft, k);
			Tile t2 = Tile.XYtoTile(bottomRight, k);

			int fromX = Math.Min(t1.X, t2.X);
			int fromY = Math.Min(t1.Y, t2.Y);
			int toX = Math.Max(t1.X, t2.X);
			int toY = Math.Max(t1.Y, t2.Y);

			for (int j = fromY; j <= toY; j++)
			{
				for (int i = fromX; i <= toX; i++)
				{
					Tile tl = new Tile(i, j, k);
					req.Add(tl);
				}
			}

			int centerX = (t1.X + t2.X) / 2;
			int centerY = (t1.Y + t2.Y) / 2;

			ToDelete = new List<Tile>();


			lock (tiles)
			{

				try
				{
					foreach (Tile drawnTile in tiles)
					{
						if (req.Contains(drawnTile))
						{
							req.Remove(drawnTile);
						}
						else
						{
							if (IsOnline)
							{
								if (drawnTile.Z >= k)
								{
									ToDelete.Add(drawnTile);
								}
								else if (drawnTile.Z < k - 2)
								{
									ToDelete.Add(drawnTile);
								}
								else if (!(extents.Intersects(drawnTile.Rectangle)))
								{
									ToDelete.Add(drawnTile);
								}
							}
							else
							{
								ToDelete.Add(drawnTile);
							}
							
						}
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
				}
			}

			return req;
		}
	}
}
