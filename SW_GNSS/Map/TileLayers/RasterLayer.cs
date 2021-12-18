using SkiaSharp;
using SW_GNSS.Map;
using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad.TileLayers
{
	public abstract class RasterLayer
	{
		public MapView View;
		public string LayerName { get;  set; }
		public bool Visible { get; set; } = true;
		public RasterLayer(MapView view,string layer)
		{
			View = view;
			LayerName = layer;
		}

		public int ZIndex = 0;

		public abstract BoundingBox ImageExtents { get; }
		public abstract void Draw( SKCanvas canvas);
	}
}
