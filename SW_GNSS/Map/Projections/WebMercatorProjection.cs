using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SwCad.Primitives;

namespace SwCad.Projections
{
	public static class WebMercatorProjection
	{
		private const double RADIUS = 6378137.0; //in meters on the equator 
		public const double WorldSize = 20037508.3427892439067364;


		private static double y2lat(double y)
		{
			return MathHelper.ToDegrees(2 * Math.Atan(Math.Exp(y / RADIUS)) - Math.PI / 2);
			//return Math.Atan(Math.Exp(y / 180 * Math.PI)) / System.Math.PI * 360 - 90;
		}

		private static double lat2y(double latitude)
		{
			return Math.Log(Math.Tan(MathHelper.ToRadians(latitude) / 2 +  Math.PI / 4)) * RADIUS;
		}

		private static double x2lon(double aX) => MathHelper.ToDegrees(aX / RADIUS);

		private static double lon2x(double aLong) => MathHelper.ToRadians(aLong) * RADIUS;


		public static Vector2D ToWebMercator(double lat, double lon)
		{
			return new Vector2D(lon2x(lon), lat2y(lat));
		}

		public static Vector2D ToWebMercator(LatLng ll)
		{
			return new Vector2D(lon2x(ll.Longitude), lat2y(ll.Latitude));
		}

		public static LatLng ToLatLng(Vector2D pt)
		{
			return new LatLng(y2lat(pt.Y), x2lon(pt.X));
		}
	}
}
