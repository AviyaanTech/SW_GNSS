using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SwCad.Primitives
{
	public class Vector2D : IEquatable<Vector2D>
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double M { get; set; }// Additional value

		[Browsable(false)]
		public float fX => (float)X;

		[Browsable(false)]
		public float fY => (float)Y;


		public Vector2D(double x, double y)
		{
			X = x;
			Y = y;
		}

		public Vector2D(System.Drawing.Point pt)
		{
			X = pt.X;
			Y = pt.Y;
		}

		public Vector2D()
		{
			X = 0;
			Y = 0;
		}

		[Browsable(false)]
		public double Magnitude => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));

		[Browsable(false)]
		public double MagnitudeSquare => X * X + Y * Y;

		[Browsable(false)]
		public double Angle
		{
			get
			{
				return Math.Atan2(Y, X);
			}
		}

		public static Vector2D operator +(Vector2D v1, Vector2D v2) => new Vector2D(v1.X + v2.X, v1.Y + v2.Y);
		public static Vector2D operator -(Vector2D v1, Vector2D v2) => new Vector2D(v1.X - v2.X, v1.Y - v2.Y);
		public static Vector2D operator *(double v1, Vector2D v2) => new Vector2D(v1 * v2.X, v1 * v2.Y);
		public static Vector2D operator *(Vector2D v2, double v1) => new Vector2D(v1 * v2.X, v1 * v2.Y);
		public static Vector2D operator /(Vector2D v2, double s) => new Vector2D(v2.X / s, v2.Y / s);

		public override string ToString()
		{
			return $"({X},{Y})";
		}

		public double Dot(Vector2D other) => this.X * other.X + this.Y * other.Y;

		[Browsable(false)]
		public Vector2D Unit => this / Magnitude;

		public Vector2D Rotate(double deg)
		{
			double rad = MathHelper.ToRadians(deg);
			double rx = Math.Cos(rad) * this.X - Math.Sin(rad) * this.Y;
			double ry = Math.Sin(rad) * this.X + Math.Cos(rad) * this.Y;
			return new Vector2D(rx, ry) { M = this.M };
		}

		public Vector2D Transform(double m11, double m12, double m21, double m22)
		{
			var x = m11 * this.X + m12 * this.Y;
			var y = m21 * this.X + m22 * this.Y;
			return new Vector2D(x, y) { M = this.M };
		}

		public Vector2D Clone()
		{
			return new Vector2D(X, Y) { M = this.M };
		}

		public double Cross(Vector2D v1)
		{
			return this.X * v1.Y - this.Y * v1.X;
		}

		public override bool Equals(object obj)
		{
			if (obj is Vector2D v)
			{
				return (Math.Abs(v.X - X) < 1E-8 && Math.Abs(v.Y - Y) < 1E-8);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}


		public static Vector2D CreateFromPolar(double mag, double angleRad)
		{
			return new Vector2D(Math.Cos(angleRad), Math.Sin(angleRad)) * mag;
		}


		public SKPoint ToSKPoint()
		{
			return new SKPoint(fX, fY);
		}

		public Vector3D ToVector3D(double elevation = 0)
		{
			return new Vector3D(X, Y, elevation) { M = this.M };
		}

		public System.Drawing.Point ToPoint()
		{
			return new System.Drawing.Point((int)X, (int)Y);
		}

		public bool Equals(Vector2D other)
		{
			return (X == other.X && Y == other.Y);
		}

		public byte[] Serialize()
		{
			var ret = new List<byte>();
			ret.AddRange(BitConverter.GetBytes(X));
			ret.AddRange(BitConverter.GetBytes(Y));
			return ret.ToArray();
		}

		public Vector2D(byte[] s)
		{
			X = BitConverter.ToDouble(s, 0);
			Y = BitConverter.ToDouble(s, 8);
		}

		public static byte[] Serialize(List<Vector2D> points)
		{
			var ret = new List<byte>();

			for (int i = 0; i < points.Count; i++)
			{
				ret.AddRange(BitConverter.GetBytes(points[i].X));
				ret.AddRange(BitConverter.GetBytes(points[i].Y));
			}
			return ret.ToArray();
		}

		public static List<Vector2D> Deserialize(byte[] points)
		{
			var ret = new List<Vector2D>();
			for (int i = 0; i < points.Length; i += 16)
			{
				var x = BitConverter.ToDouble(points, i);
				var y = BitConverter.ToDouble(points, i + 8);
				ret.Add(new Vector2D(x, y));
			}
			return ret;
		}
	}
}
