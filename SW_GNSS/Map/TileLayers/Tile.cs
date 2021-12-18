using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwCad.Projections;
using SkiaSharp;
using SwCad.Entities;
using System.IO;
using SW_GNSS.Map;

namespace SwCad.TileLayers
{
	public class Tile : IEquatable<Tile>
	{
		public static readonly double WorldSize = WebMercatorProjection.WorldSize;
		public int X { get; private set; }
		public int Y { get; private set; }
		public int Z { get; private set; }
		public byte[] data;
		public readonly SKBitmap Bitmap;
		private readonly SKShader shader;
		private readonly SKPaint Paint;
		private readonly SKPaint OutlinePaint = new SKPaint() { Color = SKColors.Red, StrokeWidth = 2, Style = SKPaintStyle.Stroke };
		private readonly SKPaint TextPaint = new SKPaint() { Color = SKColors.Red, StrokeWidth = 2, Style = SKPaintStyle.Fill, TextSize = 16f, TextAlign = SKTextAlign.Center, IsAntialias = true };
		public double TileSize => 2 * WorldSize / Math.Pow(2, Z);

		private static SKPoint[] TextureCoords = new SKPoint[] { new SKPoint(0, 0), new SKPoint(256, 0), new SKPoint(256, 256), new SKPoint(0, 256) };

		public BoundingBox Rectangle { get; private set; }

		public static BoundingBox GetTileBounds(int x, int y, int z)
		{
			var tileSize = 2 * WorldSize / (1 << z);
			var left = -WorldSize + x * tileSize;
			var top = WorldSize - (y) * tileSize;
			var right = left + tileSize;
			var bottom = top - tileSize;
			return new BoundingBox(left, bottom, right, top);
		}

		public static Tile XYtoTile(Vector2D xy, int k)
		{
			int x = (int)Math.Floor((xy.X + Tile.WorldSize) / (2 * Tile.WorldSize) * Math.Pow(2, k));
			if (x < 0)
			{
				x = 0;
			}
			if (x > Math.Pow(2, k) - 1)
			{
				x = (int)Math.Pow(2, k) - 1;
			}


			int y = (int)Math.Floor((Tile.WorldSize - xy.Y) / (2 * Tile.WorldSize) * Math.Pow(2, k));
			if (y < 0)
			{
				y = 0;
			}
			if (y > Math.Pow(2, k) - 1)
			{
				y = (int)Math.Pow(2, k) - 1;
			}

			return new Tile(x, y, k);
		}

		private static UInt16[] TriangleIndices = new UInt16[] { 0, 1, 2, 0, 2, 3 };

		public Tile(int x, int y, int z, byte[] img = null)
		{
			X = x;
			Y = y;
			Z = z;
			data = (byte[])img?.Clone();

			var left = -WorldSize + X * TileSize;
			var top = WorldSize - (Y) * TileSize;
			var right = left + TileSize;
			var bottom = top - TileSize;

			Rectangle = new BoundingBox(left, bottom, right, top);
			
			if (data != null)
			{
				try
				{
					Bitmap = SKBitmap.Decode(data);
					if (Bitmap == null)
					{
						Console.WriteLine(Encoding.UTF8.GetString(data));
					}
					else
					{
						shader = SKShader.CreateBitmap(Bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
						Paint = new SKPaint()
						{
							Shader = shader
						};
					}
				}
				catch
				{
					Paint = new SKPaint();
				}
			}
		}
		public bool EqualsTile(Tile t)
		{
			return (X == t.X && Y == t.Y && Z == t.Z);
		}

		public List<Tile> Split()
		{
			List<Tile> ret = new List<Tile>
			{
				new Tile(X * 2, Y * 2, Z + 1),
				new Tile(X * 2 + 1, Y * 2, Z + 1),
				new Tile(X * 2, Y * 2 + 1, Z + 1),
				new Tile(X * 2 + 1, Y * 2 + 1, Z + 1)
			};
			return ret;
		}

		public bool Equals(Tile t)
		{
			return (X == t.X && Y == t.Y && Z == t.Z);
		}

		public Vector2D TopLeftCoordinates => Rectangle.TopLeft;
		public Vector2D BottomLeftCoordinates => Rectangle.BottomLeft;
		public Vector2D TopRightCoordinates => Rectangle.TopRight;
		public Vector2D BottomRightCoordinates => Rectangle.BottomRight;
		public Vector2D CenterCoordinates => Rectangle.Center;


		internal Tile CloneWithoutData()
		{
			return new Tile(X, Y, Z);
		}

		internal List<Vector2D> GetDrawnPoints(MapView view)
		{
			var points = new List<Vector2D>();

			var tl = new Vector2D(Rectangle.Left, Rectangle.Top);
			var tr = new Vector2D(Rectangle.Right, Rectangle.Top);
			var bl = new Vector2D(Rectangle.Left, Rectangle.Bottom);
			var br = new Vector2D(Rectangle.Right, Rectangle.Bottom);

			points.Add(tl);
			points.Add(tr);
			points.Add(br);
			points.Add(bl);
			return points;
		}

		List<Vector2D> drawnPoints = null;
		static string crs;
		public static BoundingBox ViewExtents { get; private set; }

		public static bool DEBUG_TILE = false;

		public void Draw(MapView view, SKCanvas canvas)
		{
			if (Bitmap == null) return;
			var rect = view.Transform.Transform(Rectangle).ToSKRect();
			canvas.DrawBitmap(Bitmap, rect, null);
			if (DEBUG_TILE)
			{
				canvas.DrawRect(rect, OutlinePaint);
				canvas.DrawText($"{X},{Y},{Z}", rect.MidX, rect.MidY, TextPaint);
			}
		}

	}

}
