using SkiaSharp;
using SW_GNSS.Map;
using SW_GNSS.Processing;
using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwCad.Entities
{
	public class PointEntity : Vector2D
	{
		public FixQuality Fix { get; set; } = FixQuality.Single;


		private static SKPaint SinglePaint = new SKPaint() { Color = SKColors.Red, Style = SKPaintStyle.Fill, IsAntialias = true, FilterQuality = SKFilterQuality.High };
		private static SKPaint FloatPaint = new SKPaint() { Color = SKColors.Orange, Style = SKPaintStyle.Fill, IsAntialias = true, FilterQuality = SKFilterQuality.High };
		private static SKPaint FixPaint = new SKPaint() { Color = SKColors.Green, Style = SKPaintStyle.Fill, IsAntialias = true, FilterQuality = SKFilterQuality.High };
		private static SKPaint PppPaint = new SKPaint() { Color = SKColors.CornflowerBlue, Style = SKPaintStyle.Fill, IsAntialias = true, FilterQuality = SKFilterQuality.High };
		private static SKPaint DgpsPaint = new SKPaint() { Color = SKColors.Blue, Style = SKPaintStyle.Fill, IsAntialias = true, FilterQuality = SKFilterQuality.High };
		private static SKPaint SbasPaint = new SKPaint() { Color = SKColors.Magenta, Style = SKPaintStyle.Fill, IsAntialias = true, FilterQuality = SKFilterQuality.High };

		public void Draw(MapView map, SKCanvas canvas)
		{
			if (map.Extents.ContainsPoint(this) == false) return;

			var paint = SinglePaint;
			if (Fix == FixQuality.Fix) paint = FixPaint;
			else if (Fix == FixQuality.Float) paint = FloatPaint;
			else if (Fix == FixQuality.PPP) paint = PppPaint;
			else if (Fix == FixQuality.DGPS) paint = DgpsPaint;
			else if (Fix == FixQuality.SBAS) paint = SbasPaint;

			var pt = map.Transform.Transform(this);
			canvas.DrawCircle(pt.ToSKPoint(), 2, paint);
		}
	}
}
