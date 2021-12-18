using System;

namespace SwCad.Primitives
{

	public class Vector3D
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public double M { get; set; } // Additional value
		public Vector3D()
		{
			X = 0;
			Y = 0;
			Z = 0;
		}
		public Vector3D(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public Vector3D(Vector2D xy, double z)
		{
			X = xy.X;
			Y = xy.Y;
			Z = z;
		}
		public static Vector3D operator +(Vector3D v1, Vector3D v2) => new Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
		public static Vector3D operator -(Vector3D v1, Vector3D v2) => new Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
		public static Vector3D operator *(double n, Vector3D v) => new Vector3D(n * v.X, n * v.Y, n * v.Z);
		public static Vector3D operator *(Vector3D v, double n) => new Vector3D(n * v.X, n * v.Y, n * v.Z);
		public static Vector3D operator /(Vector3D v, double n) => new Vector3D(v.X / n, v.Y / n, v.Z / n);

		public float fX => (float)X;
		public float fY => (float)Y;
		public float fZ => (float)Z;
		public override bool Equals(object obj)
		{
			if (obj is Vector3D v)
			{
				return v.X == X && v.Y == Y & v.Z == Z;
			}
			return base.Equals(obj);
		}

		public Vector3D Clone() => new Vector3D(X, Y, Z) { M = this.M };

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public double Magnitude => Math.Pow((Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2)), 0.5);
		public double MagnitudeSquare => (Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
		public Vector3D Unit => this / Magnitude;
		public Vector3D Reverse => new Vector3D(-X, -Y, -Z);
		public static Vector3D UnitX => new Vector3D(1, 0, 0);
		public static Vector3D UnitY => new Vector3D(0, 1, 0);
		public static Vector3D UnitZ => new Vector3D(0, 0, 1);

		public Vector3D Cross(Vector3D other)
		{
			double _x = Y * other.Z - Z * other.Y;
			double _y = Z * other.X - X * other.Z;
			double _z = X * other.Y - Y * other.X;
			return new Vector3D(_x, _y, _z);
		}


		public double Dot(Vector3D other) => X * other.X + Y * other.Y + Z * other.Z;
		public static bool operator ==(Vector3D v1, Vector3D v2)
		{
			if (v1 is null)
			{
				return (v2 is null);
			}

			if (v2 is null)
			{
				return (v1 is null);
			}

			return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
		}
		public static bool operator !=(Vector3D v1, Vector3D v2)
		{
			if (v1 is null)
			{
				return !(v2 is null);
			}

			if (v2 is null)
			{
				return !(v1 is null);
			}

			return v1.X != v2.X || v1.Y != v2.Y || v1.Z != v2.Z;
		}

		public override string ToString() => $"({X},{Y},{Z})";


		public Vector2D ProjectionXY => new Vector2D(X, Y) { M = this.M };
	}
}
