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
using System.Windows.Shapes;

namespace SW_GNSS
{
	/// <summary>
	/// Interaction logic for FrmEditTileLayer.xaml
	/// </summary>
	public partial class FrmEditTileLayer : Window
	{
		public FrmEditTileLayer()
		{
			InitializeComponent();
		}

		public XyzTileSource TileSource = null;
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if(TileSource == null)
			{
				this.Title = "Add New Tile Source";
			}
			else
			{
				this.Title = "Edit Tile Source";
				txtLayerName.Text = TileSource.Name;
				txtTileUrl.Text = TileSource.URL;
				numMaxZoom.Value = TileSource.MaxZoomLevel;
				numMinZoom.Value = TileSource.MinZoomLevel;
			}
		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(txtLayerName.Text))
			{
				Workspace.ShowErrorBox("Enter a name.");
				return;
			}
			if (string.IsNullOrEmpty(txtTileUrl.Text))
			{
				Workspace.ShowErrorBox("Enter the tile URL.");
				return;
			}

			var ts = new XyzTileSource();
			ts.Name = txtLayerName.Text;
			ts.URL = txtTileUrl.Text;
			ts.MinZoomLevel = numMinZoom.Value ?? 0;
			ts.MaxZoomLevel = numMaxZoom.Value ?? 19;

			if (TileSource != null) XyzTileSource.RemoveSource(ts.Name);
			XyzTileSource.AddSource(ts);

			this.DialogResult = true; this.Close();

		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
  