using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad.Primitives
{
	public class LatLng
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public LatLng(double lat, double lon)
		{
			Latitude = lat;
			Longitude = lon;
		}
	}

	public class LatLngHt
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public double Elevation { get; set; }

		public LatLngHt(double lat, double lon, double elv)
		{
			Latitude = lat;
			Longitude = lon;
			Elevation = elv;
		}
	}
}
