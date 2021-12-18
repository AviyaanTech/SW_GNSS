using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW_GNSS.Map
{
	public static class PointExtensions
	{
		public static Vector2D ToVector2D(this System.Windows.Point pt)
		{
			return new Vector2D(pt.X, pt.Y);
		}

	}
}
