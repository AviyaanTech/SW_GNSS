using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwCad.Primitives;

namespace SwCad.Projections
{
	public class TransverseMercatorProjection
	{
		const double pi = 3.14159265358979;
		public readonly double ScaleFactor = 0.9996;
		public readonly double CentralMeridian;
		Ellipsoid Ellipsoid;

		public readonly double FalseEasting;
		public readonly double FalseNorthing;

		public TransverseMercatorProjection(Ellipsoid ell, double CentralMeridian, double scale_factor, double falseNorthing = 10000000, double falseEasting = 500000)
		{
			Ellipsoid = ell;
			ScaleFactor = scale_factor;
			FalseNorthing = falseNorthing;
			FalseEasting = falseEasting;
			this.CentralMeridian = CentralMeridian;
		}

		public Vector2D LatLngToXY(LatLng ll)
		{
			var lat = DegToRad(ll.Latitude);
			var lon = DegToRad(ll.Longitude);
			var xy = MapLatLonToXY(lat, lon, DegToRad(CentralMeridian));
			xy[0] = xy[0] * ScaleFactor + FalseEasting;
			xy[1] = xy[1] * ScaleFactor;

			if (xy[1] < 0.0) xy[1] += FalseNorthing;
			return new Vector2D(xy[0], xy[1]);
		}

		public Vector2D LatLngToXY(double lat_deg,double lon_deg)
		{
			var lat = DegToRad(lat_deg);
			var lon = DegToRad(lon_deg);

			var xy = MapLatLonToXY(lat, lon, DegToRad(CentralMeridian));
			xy[0] = xy[0] * ScaleFactor + FalseEasting;
			xy[1] = xy[1] * ScaleFactor;

			if (xy[1] < 0.0) xy[1] += FalseNorthing;
			return new Vector2D(xy[0], xy[1]);
		}

		public LatLng XYToLatLon(Vector2D pt)
		{
			var x = pt.X;
			var y = pt.Y;

			x -= FalseEasting;
			if (y > FalseNorthing) y -= FalseNorthing;

			x /= ScaleFactor;
			y /= ScaleFactor;
			var ll = MapXYToLatLon(x, y, DegToRad(CentralMeridian));
			return new LatLng(RadToDeg(ll[0]), RadToDeg(ll[1]));
		}


		public List<Vector2D> SphericalMercatorToXY(List<Vector2D> sphericalMercatorCoordinates)
		{
			var ret = new List<Vector2D>();
			foreach(var pt in sphericalMercatorCoordinates)
				ret.Add(LatLngToXY(WebMercatorProjection.ToLatLng(pt)));

			return ret;
		}


		public List<Vector2D> XYToSphericalMercator(List<Vector2D> xyCoordinates)
		{
			var ret = new List<Vector2D>();
			foreach (var pt in xyCoordinates)
				ret.Add(WebMercatorProjection.ToWebMercator(XYToLatLon(pt)));

			return ret;
		}


		public Vector2D SphericalMercatorToXY(Vector2D pt)
		{
			return LatLngToXY(WebMercatorProjection.ToLatLng(pt));
		}


		public Vector2D XYToSphericalMercator(Vector2D xyCoordinates)
		{
			return WebMercatorProjection.ToWebMercator(XYToLatLon(xyCoordinates));
		}

		#region "Instance Creators"

		private static double UTMCentralMeridian(int zone)
		{
			return -183 + (zone * 6);
		}

		public static int GetUTMZone(double longitude_deg)
		{
			return (int)(1 + Math.Floor((longitude_deg + 180d) / 6d)); // longitude to utm zone
		}


		/// <summary>
		/// Create a UTM Projection
		/// </summary>
		/// <param name="zone">The utm zone number. Pass 44 for UTM-44</param>
		/// <returns>The projection</returns>
		public static TransverseMercatorProjection UTM(int zone)
		{
			return new TransverseMercatorProjection(Ellipsoid.WGS84, UTMCentralMeridian(zone), 0.9996);
		}

		public static TransverseMercatorProjection UTMLon(double longitude_deg)
		{
			var zone = GetUTMZone(longitude_deg);
			return new TransverseMercatorProjection(Ellipsoid.WGS84, UTMCentralMeridian(zone), 0.9996);
		}


		/// <summary>
		/// Create a MUTM Projection
		/// </summary>
		/// <param name="CM">Central meridian of the MUTM zone. Pass 81 for MUTM-81</param>
		/// <returns>The projection</returns>
		public static TransverseMercatorProjection MUTM(double CM)
		{
			return new TransverseMercatorProjection(Ellipsoid.Everest1830, CM, 0.9999);
		}
		#endregion

		#region "Math Functions"
		protected static double DegToRad(double deg)
		{
			return (deg / 180.0 * pi);
		}

		protected static double RadToDeg(double rad)
		{
			return (rad / pi * 180.0);
		}

		protected static double Pow(double num, double exp)
		{
			return Math.Pow(num, exp);
		}

		protected static double Sin(double num)
		{
			return Math.Sin(num);
		}

		protected static double Cos(double num)
		{
			return Math.Cos(num);
		}

		protected static double Tan(double num)
		{
			return Math.Tan(num);
		}

		protected static double Sqrt(double num)
		{
			return Math.Sqrt(num);
		}

		#endregion


		#region "Projection Computations"

		/// <summary>
		///  Computes the ellipsoidal distance from the equator to a point at a given latitude.
		/// </summary>
		/// <param name="phi">Latitude of the point, in radians.</param>
		/// <returns>The ellipsoidal distance of the point from the equator, in meters.</returns>
		protected double ArcLengthOfMeridian(double phi)
		{
			var n = (Ellipsoid.a - Ellipsoid.b) / (Ellipsoid.a + Ellipsoid.b);
			var alpha = ((Ellipsoid.a + Ellipsoid.b) / 2.0) * (1.0 + (Pow(n, 2.0) / 4.0) + (Pow(n, 4.0) / 64.0));
			var beta = (-3.0 * n / 2.0) + (9.0 * Pow(n, 3.0) / 16.0) + (-3.0 * Pow(n, 5.0) / 32.0);
			var gamma = (15.0 * Pow(n, 2.0) / 16.0) + (-15.0 * Pow(n, 4.0) / 32.0);
			var delta = (-35.0 * Pow(n, 3.0) / 48.0) + (105.0 * Pow(n, 5.0) / 256.0);
			var epsilon = (315.0 * Pow(n, 4.0) / 512.0);
			var result = alpha * (phi + (beta * Sin(2.0 * phi)) + (gamma * Sin(4.0 * phi)) + (delta * Sin(6.0 * phi)) + (epsilon * Sin(8.0 * phi)));
			return result;
		}

		/// <summary>
		/// Computes the footpoint latitude for use in converting transverse Mercator coordinates to ellipsoidal coordinates.
		/// </summary>
		/// <param name="y">The UTM northing coordinate, in meters.</param>
		/// <returns>The footpoint latitude, in radians.</returns>
		protected double FootpointLatitude(double y)
		{
			
			var n = (Ellipsoid.a - Ellipsoid.b) / (Ellipsoid.a + Ellipsoid.b);
			var alpha_ = ((Ellipsoid.a + Ellipsoid.b) / 2.0) * (1 + (Pow(n, 2.0) / 4) + (Pow(n, 4.0) / 64));
			var y_ = y / alpha_;
			var beta_ = (3.0 * n / 2.0) + (-27.0 * Pow(n, 3.0) / 32.0) + (269.0 * Pow(n, 5.0) / 512.0);
			var gamma_ = (21.0 * Pow(n, 2.0) / 16.0) + (-55.0 * Pow(n, 4.0) / 32.0);
			var delta_ = (151.0 * Pow(n, 3.0) / 96.0) + (-417.0 * Pow(n, 5.0) / 128.0);
			var epsilon_ = (1097.0 * Pow(n, 4.0) / 512.0);
			var result = y_ + (beta_ * Sin(2.0 * y_)) + (gamma_ * Sin(4.0 * y_)) + (delta_ * Sin(6.0 * y_)) + (epsilon_ * Sin(8.0 * y_));
			return result;
		}


		/// <summary>
		/// Converts a latitude/longitude pair to x and y coordinates in the Transverse Mercator projection.
		/// Note that Transverse Mercator is not the same as UTM; a scale factor is required to convert between them.
		/// </summary>
		/// <param name="phi">Latitude of the point, in radians.</param>
		/// <param name="lambda">Longitude of the point, in radians.</param>
		/// <param name="lambda0">Longitude of the central meridian to be used, in radians.</param>
		/// <returns> A 2-element array containing the x and y coordinates of the computed point.</returns>
		protected double[] MapLatLonToXY(double phi, double lambda, double lambda0)
		{
			var ep2 = (Pow(Ellipsoid.a, 2.0) - Pow(Ellipsoid.b, 2.0)) / Pow(Ellipsoid.b, 2.0);
			var nu2 = ep2 * Pow(Cos(phi), 2.0);
			var N = Pow(Ellipsoid.a, 2.0) / (Ellipsoid.b * Sqrt(1 + nu2));
			var t = Tan(phi);
			var t2 = t * t;

			var l = lambda - lambda0;

			var l3coef = 1.0 - t2 + nu2;
			var l4coef = 5.0 - t2 + 9 * nu2 + 4.0 * (nu2 * nu2);
			var l5coef = 5.0 - 18.0 * t2 + (t2 * t2) + 14.0 * nu2 - 58.0 * t2 * nu2;
			var l6coef = 61.0 - 58.0 * t2 + (t2 * t2) + 270.0 * nu2 - 330.0 * t2 * nu2;
			var l7coef = 61.0 - 479.0 * t2 + 179.0 * (t2 * t2) - (t2 * t2 * t2);
			var l8coef = 1385.0 - 3111.0 * t2 + 543.0 * (t2 * t2) - (t2 * t2 * t2);

			double[] xy = new double[2];

			// Calculate easting (x) 
			xy[0] = N * Cos(phi) * l + (N / 6.0 * Pow(Cos(phi), 3.0) * l3coef * Pow(l, 3.0)) + (N / 120.0 * Pow(Cos(phi), 5.0) * l5coef * Pow(l, 5.0)) + (N / 5040.0 * Pow(Cos(phi), 7.0) * l7coef * Pow(l, 7.0));

			// Calculate northing (y) 
			xy[1] = ArcLengthOfMeridian(phi) + (t / 2.0 * N * Pow(Cos(phi), 2.0) * Pow(l, 2.0)) + (t / 24.0 * N * Pow(Cos(phi), 4.0) * l4coef * Pow(l, 4.0)) + (t / 720.0 * N * Pow(Cos(phi), 6.0) * l6coef * Pow(l, 6.0)) + (t / 40320.0 * N * Pow(Cos(phi), 8.0) * l8coef * Pow(l, 8.0));

			return xy;
		}


		/// <summary>
		/// Converts x and y coordinates in the Transverse Mercator projection to a latitude/longitude pair.
		/// Note that Transverse Mercator is not the same as UTM; a scale factor is required to convert between them.
		/// </summary>
		/// <param name="x">The easting of the point, in meters.</param>
		/// <param name="y">The northing of the point, in meters.</param>
		/// <param name="lambda0">Longitude of the central meridian to be used, in radians.</param>
		/// <returns>A 2-element containing the latitude and longitude in radians.</returns>
		protected double[] MapXYToLatLon(double x, double y, double lambda0)
		{
			

			var phif = FootpointLatitude(y);
			var ep2 = (Pow(Ellipsoid.a, 2.0) - Pow(Ellipsoid.b, 2.0)) / Pow(Ellipsoid.b, 2.0);
			var cf = Cos(phif);
			var nuf2 = ep2 * Pow(cf, 2.0);
			var Nf = Pow(Ellipsoid.a, 2.0) / (Ellipsoid.b * Sqrt(1 + nuf2));
			var Nfpow = Nf;
			var tf = Tan(phif);
			var tf2 = tf * tf;
			var tf4 = tf2 * tf2;
			var x1frac = 1.0 / (Nfpow * cf);
			Nfpow *= Nf;
			var x2frac = tf / (2.0 * Nfpow);
			Nfpow *= Nf;
			var x3frac = 1.0 / (6.0 * Nfpow * cf);
			Nfpow *= Nf;
			var x4frac = tf / (24.0 * Nfpow);
			Nfpow *= Nf;
			var x5frac = 1.0 / (120.0 * Nfpow * cf);
			Nfpow *= Nf;
			var x6frac = tf / (720.0 * Nfpow);
			Nfpow *= Nf;
			var x7frac = 1.0 / (5040.0 * Nfpow * cf);
			Nfpow *= Nf;
			var x8frac = tf / (40320.0 * Nfpow);
			var x2poly = -1.0 - nuf2;
			var x3poly = -1.0 - 2 * tf2 - nuf2;
			var x4poly = 5.0 + 3.0 * tf2 + 6.0 * nuf2 - 6.0 * tf2 * nuf2 - 3.0 * (nuf2 * nuf2) - 9.0 * tf2 * (nuf2 * nuf2);
			var x5poly = 5.0 + 28.0 * tf2 + 24.0 * tf4 + 6.0 * nuf2 + 8.0 * tf2 * nuf2;
			var x6poly = -61.0 - 90.0 * tf2 - 45.0 * tf4 - 107.0 * nuf2 + 162.0 * tf2 * nuf2;
			var x7poly = -61.0 - 662.0 * tf2 - 1320.0 * tf4 - 720.0 * (tf4 * tf2);
			var x8poly = 1385.0 + 3633.0 * tf2 + 4095.0 * tf4 + 1575 * (tf4 * tf2);

			double[] philambda = new double[2];
			// Calculate latitude 
			philambda[0] = phif + x2frac * x2poly * (x * x) + x4frac * x4poly * Pow(x, 4.0) + x6frac * x6poly * Pow(x, 6.0) + x8frac * x8poly * Pow(x, 8.0);
			// Calculate longitude 
			philambda[1] = lambda0 + x1frac * x + x3frac * x3poly * Pow(x, 3.0) + x5frac * x5poly * Pow(x, 5.0) + x7frac * x7poly * Pow(x, 7.0);
			return philambda;
		}
		#endregion

	}
}
