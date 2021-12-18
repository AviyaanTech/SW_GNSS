using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad.Primitives
{
	public class Vertex : Vector3D
	{
		public int MinimumZoomLevel = 0;

		public Vertex(Vector3D point, int minZoom = 0)
		{
			this.X = point.X;
			this.Y = point.Y;
			MinimumZoomLevel = minZoom;
		}
	}
}
