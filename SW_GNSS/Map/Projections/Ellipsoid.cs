using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwCad.Primitives;
namespace SwCad.Projections
{
	public class Ellipsoid
	{
		public double a { get; }
		public double b { get; }

		public double f => (a - b) / a;

		public Ellipsoid(double _a, double _b)
		{
			a = _a;
			b = _b;
		}

		private double ea => Math.Sqrt((a * a - b * b) / (a * a));
		private double eb => Math.Sqrt((a * a - b * b) / (b * b));

		#region "Predefined Ellipsoids"
		public static Ellipsoid WGS84 => new Ellipsoid(6378137.0, 6356752.314);
		public static Ellipsoid Everest1830 => new Ellipsoid(6377276.345, 6356075.413);
		#endregion

		#region "ECEF Transformations"
		public Vector3D ToEcefCoordinate(LatLng ll, double Ht = 0)
		{
			double lat_rad = ll.Latitude * Math.PI / 180;
			double lon_rad = ll.Longitude * Math.PI / 180;

			double N = a / Math.Sqrt(1 - Math.Pow(ea, 2) * Math.Pow(Math.Sin(lat_rad), 2));
			double x = (N + Ht) * Math.Cos(lat_rad) * Math.Cos(lon_rad);
			double y = (N + Ht) * Math.Cos(lat_rad) * Math.Sin(lon_rad);
			double z = ((Math.Pow(b, 2) / Math.Pow(a, 2)) * N + Ht) * Math.Sin(lat_rad);
			return new Vector3D(x, y, z);
		}

		public Vector3D ToEcefCoordinate(double latitude, double longitude, double Ht = 0)
		{
			double lat_rad = latitude * Math.PI / 180;
			double lon_rad = longitude * Math.PI / 180;

			double N = a / Math.Sqrt(1 - Math.Pow(ea, 2) * Math.Pow(Math.Sin(lat_rad), 2));
			double x = (N + Ht) * Math.Cos(lat_rad) * Math.Cos(lon_rad);
			double y = (N + Ht) * Math.Cos(lat_rad) * Math.Sin(lon_rad);
			double z = ((Math.Pow(b, 2) / Math.Pow(a, 2)) * N + Ht) * Math.Sin(lat_rad);
			return new Vector3D(x, y, z);
		}

		public Vector3D ToEcefCoordinate(LatLng ll)
		{
			double lat_rad = ll.Latitude * Math.PI / 180;
			double lon_rad = ll.Longitude * Math.PI / 180;

			double N = a / Math.Sqrt(1 - Math.Pow(ea, 2) * Math.Pow(Math.Sin(lat_rad), 2));
			double x = (N) * Math.Cos(lat_rad) * Math.Cos(lon_rad);
			double y = (N) * Math.Cos(lat_rad) * Math.Sin(lon_rad);
			double z = ((Math.Pow(b, 2) / Math.Pow(a, 2)) * N) * Math.Sin(lat_rad);
			return new Vector3D(x, y, z);
		}


		public LatLngHt ToLatLngHt(Vector3D ecef)
		{
			double p = Math.Sqrt(Math.Pow(ecef.X, 2) + Math.Pow(ecef.Y, 2));
			double theta = Math.Atan((ecef.Z * a) / (p * b));
			double lon_rad = Math.Atan(ecef.Y / ecef.X);
			double lat_rad = Math.Atan((ecef.Z + Math.Pow(eb, 2) * b * Math.Pow(Math.Sin(theta), 3)) / (p - Math.Pow(ea, 2) * a * Math.Pow(Math.Cos(theta), 3)));

			double N = a / Math.Sqrt(1 - Math.Pow(ea, 2) * Math.Pow(Math.Sin(lat_rad), 2));
			double h = p / Math.Cos(lat_rad) - N;
			return new LatLngHt(lat_rad * 180 / Math.PI, lon_rad * 180 / Math.PI, h);
		}

		public LatLng ToLatLng(Vector3D ecef)
		{
			double p = Math.Sqrt(Math.Pow(ecef.X, 2) + Math.Pow(ecef.Y, 2));
			double theta = Math.Atan((ecef.Z * a) / (p * b));
			double lon_rad = Math.Atan(ecef.Y / ecef.X);
			double lat_rad = Math.Atan((ecef.Z + Math.Pow(eb, 2) * b * Math.Pow(Math.Sin(theta), 3)) / (p - Math.Pow(ea, 2) * a * Math.Pow(Math.Cos(theta), 3)));
			return new LatLng(lat_rad * 180 / Math.PI, lon_rad * 180 / Math.PI);
		}
		#endregion
	}
}
