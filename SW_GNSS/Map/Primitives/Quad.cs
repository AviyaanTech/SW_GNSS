using SkiaSharp;
using SwCad.Primitives;
using SwCad.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad.Primitives
{
	public class Quad
	{
		public Vector2D[] V = new Vector2D[4];
		public BoundingBox BoundingBox => BoundingBox.FromPoints(V[0], V[1], V[2], V[3]);

		public Quad() { }
		public Quad(BoundingBox b)
		{
			V[0] = b.TopLeft;
			V[1] = b.TopRight;
			V[2] = b.BottomRight;
			V[3] = b.BottomLeft;
		}

		public Quad(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3)
		{
			V[0] = new Vector2D(x0, y0);
			V[1] = new Vector2D(x1, y1);
			V[2] = new Vector2D(x2, y2);
			V[3] = new Vector2D(x3, y3);
		}

		public void Add(Vector2D v)
		{
			V[0].X += v.X;
			V[1].X += v.X;
			V[2].X += v.X;
			V[3].X += v.X;

			V[0].Y += v.Y;
			V[1].Y += v.Y;
			V[2].Y += v.Y;
			V[3].Y += v.Y;
		}

		public void Subtract(Vector2D v)
		{
			V[0].X -= v.X;
			V[1].X -= v.X;
			V[2].X -= v.X;
			V[3].X -= v.X;

			V[0].Y -= v.Y;
			V[1].Y -= v.Y;
			V[2].Y -= v.Y;
			V[3].Y -= v.Y;
		}
		public void Scale(double widthFactor, double heightFactor)
		{
			V[0].X *= widthFactor;
			V[1].X *= widthFactor;
			V[2].X *= widthFactor;
			V[3].X *= widthFactor;

			V[0].Y *= heightFactor;
			V[1].Y *= heightFactor;
			V[2].Y *= heightFactor;
			V[3].Y *= heightFactor;
		}
	}
}
