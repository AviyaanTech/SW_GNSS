using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SwCad;
using SwCad.Primitives;
using SwCad.Projections;
using SwCad.Algorithms;
using System.ComponentModel;
using SW_GNSS.Processing;
using SwCad.Entities;
using SwCad.TileLayers;

namespace SW_GNSS.Map
{
	/// <summary>
	/// Interaction logic for MapView.xaml
	/// </summary>
	public partial class MapView : UserControl
	{
		public SKColor BackgroundColor { get; set; } = new SKColor(220, 220, 220);
		public bool IsLoadingRaster = false;

		public OnlineTileLayer BaseMap;
		public MapView()
		{
			InitializeComponent();
			BaseMap = new OnlineTileLayer(this, "GoogleMap", UrlTileProvider.GoogleSatellite, true);
		}

		public List<PointEntity> Points = new List<PointEntity>();


		internal BoundingBox Extents;

		internal TransformationMatrix Transform = TransformationMatrix.CreateScale(1, -1);
		internal bool designMode => (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
		internal bool Panning = false;
		internal Vector2D PanPoint;

		private bool VerificationError = false;
		double TransformScale
		{
			get
			{
				try
				{
					return System.Windows.PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
				}
				catch
				{
					return 1;
				}
			}
		}

		public void UpdateBaseMap()
		{

			var tl = Transform.InvertTransform(new Vector2D(0, 0));
			var br = Transform.InvertTransform(new Vector2D(RenderWidth, RenderHeight));

			Extents = new BoundingBox(tl.X, br.Y, br.X, tl.Y);
			BaseMap?.Update(true);


		}

		public double RenderWidth => ActualWidth * TransformScale;
		public double RenderHeight => ActualHeight * TransformScale;

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			view.MouseDown += View_MouseDown;
			view.MouseUp += View_MouseUp;
			view.MouseMove += View_MouseMove;
			view.MouseWheel += View_MouseWheel;
			view.MouseLeave += View_MouseLeave;
		}

		private void View_MouseLeave(object sender, MouseEventArgs e)
		{
			Panning = false;
		}


		public void Clear()
		{
			Points.Clear();
			Invalidate();
		}


		internal TransformationMatrix GetExtentsTransform(BoundingBox bbox)
		{
			var width = (int)RenderWidth;
			var height = (int)RenderHeight;


			var t = TransformationMatrix.CreateScale(1, -1);
			if (Points.Count == 0) return t;
			var screenCenter = new Vector2D(width / 2, height / 2);
			var bounds = bbox;
			bounds.Scale(1.1);
			var ctr = bounds.Center;


			if (bounds.Width == 0 && bounds.Height == 0) return TransformationMatrix.CreateTranslation(screenCenter.X - ctr.X, screenCenter.Y - ctr.Y) * t;

			var sx = width / bounds.Width;
			var sy = height / bounds.Height;
			var scale = Math.Min(sx, sy);

			return TransformationMatrix.CreateTranslation(screenCenter.X, screenCenter.Y)
				* TransformationMatrix.CreateScale(scale)
				* TransformationMatrix.CreateTranslation(-ctr.X, ctr.Y) * t;
		}

		public void ZoomExtents()
		{
			BoundingBox bbox;
			if (Points.Count == 0)
			{
				bbox = new Tile(0, 0, 0).Rectangle;
			}
			else
			{
				bbox = BoundingBox.FromPoints(Points.Select(p => new Vector2D(p.X, p.Y)));
			}
			Transform = GetExtentsTransform(bbox);
			UpdateBaseMap();
			Invalidate();
		}

		internal void ZoomToBounds(BoundingBox activeSectionBounds)
		{
			Transform = GetExtentsTransform(activeSectionBounds);
			UpdateBaseMap();
			Invalidate();
		}

		public void Invalidate()
		{
			view.InvalidateVisual();
		}

		private void View_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			var position = e.GetPosition(this);
			if (e.Delta > 0)
			{
				Transform = TransformationMatrix.CreateScale(position.X, position.Y, 1.5) * Transform;
				UpdateBaseMap();
				Invalidate();
			}
			else if (e.Delta < 0)
			{
				Transform = TransformationMatrix.CreateScale(position.X, position.Y, 1 / 1.5) * Transform;
				UpdateBaseMap();
				Invalidate();
			}
		}

		private void View_MouseMove(object sender, MouseEventArgs e)
		{
			var mPt = e.GetPosition(this).ToVector2D();
			if (Panning)
			{
				var translation = (mPt - PanPoint) * TransformScale;
				Transform = TransformationMatrix.CreateTranslation(translation.X, translation.Y) * Transform;
				Invalidate();
				PanPoint = mPt;
			}
		}


		private void View_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Middle)
			{
				UpdateBaseMap();
				Panning = false;
			}
		}

		private void View_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2 && e.ChangedButton == MouseButton.Middle)
			{
				ZoomExtents();
			}

			if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Middle)
			{
				Panning = true;
				PanPoint = e.GetPosition(this).ToVector2D();
			}
		}

		private void view_PaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
		{
			if (designMode) return;
			var canvas = e.Surface.Canvas;

			if (VerificationError)
			{
				canvas.DrawColor(SKColors.Red);
				return;
			}
			canvas.DrawColor(BackgroundColor);

			BaseMap?.Draw(canvas);
			var tl = Transform.InvertTransform(new Vector2D(0, 0));
			var br = Transform.InvertTransform(new Vector2D(RenderWidth, RenderHeight));

			Extents = new BoundingBox(tl.X, br.Y, br.X, tl.Y);


			foreach (var pt in Points)
			{
				pt.Draw(this, canvas);
			}


		}

		public void Redraw()
		{
			this.Invalidate();
		}

		public int MapZoomLevel
		{
			get
			{
				var z = Math.Round(Math.Log(ActualWidth * 2 * WebMercatorProjection.WorldSize / (256 * Extents.Width)) / Math.Log(2));
				if (Double.IsNaN(z) || z < 1) return 1;
				return (int)z;
			}
		}

	}
}
