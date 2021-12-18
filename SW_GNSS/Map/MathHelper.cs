using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad
{
	internal static class MathHelper
	{
		public static double ToRadians(double deg) => deg * Math.PI / 180;
		public static double ToDegrees(double rad) => rad * 180 / Math.PI;
	}
}
