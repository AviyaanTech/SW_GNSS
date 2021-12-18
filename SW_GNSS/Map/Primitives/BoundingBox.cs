using SkiaSharp;
using SwCad.Algorithms;
using System;
using System.Collections.Generic;

namespace SwCad.Primitives
{
	public class BoundingBox
	{

		public double MinX { get; set; }
		public double MinY { get; set; }
		public double MaxX { get; set; }
		public double MaxY { get; set; }

		public double Width => MaxX - MinX;
		public double Height => MaxY - MinY;

		public double Left => MinX;
		public double Right => MaxX;
		public double Bottom => MinY;
		public double Top => MaxY;

		public Vector2D Center => new Vector2D((MinX + MaxX) / 2, (MinY + MaxY) / 2);

		public Vector2D TopLeft => new Vector2D(Left, Top);
		public Vector2D BottomLeft => new Vector2D(Left, Bottom);
		public Vector2D TopRight => new Vector2D(Right, Top);
		public Vector2D BottomRight => new Vector2D(Right, Bottom);

		public static BoundingBox FromPoints(IEnumerable<Vector2D> Points)
		{
			var ret = new BoundingBox();
			foreach (var pt in Points) ret.AddPoint(pt);
			return ret;
		}

		public static BoundingBox FromPoints(params Vector2D[] Points)
		{
			var ret = new BoundingBox();
			foreach (var pt in Points) ret.AddPoint(pt);
			return ret;
		}

		public static BoundingBox FromPoints(List<Vector3D> Points)
		{
			var ret = new BoundingBox();
			foreach (var pt in Points) ret.AddPoint(pt);
			return ret;
		}

		public BoundingBox(Vector2D point, double radius)
		{
			MinX = point.X - radius;
			MinY = point.Y - radius;
			MaxX = point.X + radius;
			MaxY = point.Y + radius;
		}

		public BoundingBox(Vector2D p1, Vector2D p2)
		{
			if (p1.X > p2.X)
			{
				MinX = p2.X;
				MaxX = p1.X;
			}
			else
			{
				MinX = p1.X;
				MaxX = p2.X;
			}

			if (p1.Y > p2.Y)
			{
				MinY = p2.Y;
				MaxY = p1.Y;
			}
			else
			{
				MinY = p1.Y;
				MaxY = p2.Y;
			}
		}

		public BoundingBox(Vector3D p1, Vector3D p2)
		{
			if (p1.X > p2.X)
			{
				MinX = p2.X;
				MaxX = p1.X;
			}
			else
			{
				MinX = p1.X;
				MaxX = p2.X;
			}

			if (p1.Y > p2.Y)
			{
				MinY = p2.Y;
				MaxY = p1.Y;
			}
			else
			{
				MinY = p1.Y;
				MaxY = p2.Y;
			}
		}

		public BoundingBox(double min_x, double min_y, double max_x, double max_y)
		{
			MinX = min_x;
			MaxX = max_x;
			MinY = min_y;
			MaxY = max_y;
		}

		public BoundingBox()
		{
			MinX = double.PositiveInfinity;
			MinY = double.PositiveInfinity;
			MaxX = double.NegativeInfinity;
			MaxY = double.NegativeInfinity;
		}

		public void AddPoint(Vector2D pt)
		{
			if (pt.X >= MaxX)
				MaxX = pt.X;
			if (pt.X <= MinX)
				MinX = pt.X;

			if (pt.Y >= MaxY)
				MaxY = pt.Y;
			if (pt.Y <= MinY)
				MinY = pt.Y;
		}

		public void AddPoint(Vector3D pt)
		{
			if (pt.X >= MaxX)
				MaxX = pt.X;
			if (pt.X <= MinX)
				MinX = pt.X;

			if (pt.Y >= MaxY)
				MaxY = pt.Y;
			if (pt.Y <= MinY)
				MinY = pt.Y;
		}

		public void AddBoundary(BoundingBox b)
		{
			if (b.MinX < MinX)
				MinX = b.MinX;
			if (b.MinY < MinY)
				MinY = b.MinY;
			if (b.MaxX > MaxX)
				MaxX = b.MaxX;
			if (b.MaxY > MaxY)
				MaxY = b.MaxY;
		}
		
		internal void Translate(Vector2D tvec)
		{
			MinX += tvec.X;
			MinY += tvec.Y;
			MaxX += tvec.X;
			MaxY += tvec.Y;
		}

		internal void Scale(double factor)
		{
			var dx = Width * (factor - 1);
			var dy = Height * (factor - 1);
			MinX -= dx / 2;
			MaxX += dx / 2;
			MinY -= dy / 2;
			MaxY += dy / 2;
		}

		internal BoundingBox GetScaled(double factor)
		{
			var dx = Width * (factor - 1);
			var dy = Height * (factor - 1);
			var minX = MinX - dx / 2;
			var maxX = MaxX + dx / 2;
			var minY = MinY - dy / 2;
			var maxY = MaxY + dy / 2;

			return new BoundingBox(minX, minY, maxX, maxY);
		}
		public bool ContainsPoint(double x, double y)
		{
			return (x >= MinX & x < MaxX & y >= MinY & y <= MaxY);
		}

		public bool ContainsPoint(Vector2D p)
		{
			return (p.X >= MinX & p.X < MaxX & p.Y >= MinY & p.Y <= MaxY);
		}

		public bool ContainsPoint(Vector3D p)
		{
			return (p.X >= MinX & p.X < MaxX & p.Y >= MinY & p.Y <= MaxY);
		}

		public bool ContainsPoint(Vector2D p, double buffer)
		{
			double bx = Width * (buffer - 1);
			double by = Height * (buffer - 1);

			return (p.X >= MinX - bx & p.X < MaxX + bx & p.Y >= MinY - by & p.Y <= MaxY + by);
		}
		public bool ContainsPoint(Vector3D p, double buffer)
		{
			double bx = Width * (buffer - 1);
			double by = Height * (buffer - 1);

			return (p.X >= MinX - bx & p.X < MaxX + bx & p.Y >= MinY - by & p.Y <= MaxY + by);
		}


		public bool IntersectsLine(Vector2D p1, Vector2D p2)
		{
			if (ContainsPoint(p1) || ContainsPoint(p2)) return true;
			if (GeometryFunctions.DoLineSegmentsIntersect(p1, p2, TopLeft, TopRight)) return true;
			if (GeometryFunctions.DoLineSegmentsIntersect(p1, p2, TopRight, BottomRight)) return true;
			if (GeometryFunctions.DoLineSegmentsIntersect(p1, p2, BottomRight, BottomLeft)) return true;
			if (GeometryFunctions.DoLineSegmentsIntersect(p1, p2, TopLeft, BottomLeft)) return true;
			return false;
		}
		public bool Intersects(BoundingBox b)
		{
			if ((MinX > b.MaxX | b.MinX > MaxX))
				return false;
			if ((MinY > b.MaxY | b.MinY > MaxY))
				return false;
			return true;
		}

		public bool ContainsRect(BoundingBox b)
		{
			if (b == null) return false;
			if (b.MinX > MinX && b.MaxX < MaxX)
				if (b.MinY > MinY && b.MaxY < MaxY)
					return true;
			return false;
		}

		public BoundingBox Offset(Vector2D translation)
		{
			return new BoundingBox(MinX + translation.X, MinY + translation.Y, MaxX + translation.X, MaxY + translation.Y);
		}

		public SKRect ToSKRect()
		{
			return new SKRect((float)MinX, (float)MinY, (float)MaxX, (float)MaxY);
		}

		internal BoundingBox Clone()
		{
			return new BoundingBox(MinX, MinY, MaxX, MaxY);
		}
	}
}