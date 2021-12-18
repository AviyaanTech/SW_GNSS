using System.Windows;
using System.IO;
using System;
using System.Collections.Generic;

namespace SW_GNSS
{
	/// <summary>
	/// Interaction logic for FrmBackgroundMapList.xaml
	/// </summary>
	public partial class FrmBackgroundMapList : Window
	{
		public List<XyzTileSource> Sources;
		
		public FrmBackgroundMapList()
		{
			InitializeComponent();
		}

		void RefreshSources()
		{
			Sources = XyzTileSource.GetSourceList();
			dgvSourceList.ItemsSource = Sources;
		}
		
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			RefreshSources();
		}

		private void btnAdd_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new FrmEditTileLayer();
			if (dlg.ShowDialog() == true)
			{
				RefreshSources();
			}
		}

		private void btnEdit_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new FrmEditTileLayer();
			dlg.TileSource = dgvSourceList.SelectedItem as XyzTileSource;

			if (dlg.ShowDialog() == true)
			{
				RefreshSources();
			}
		}

		private void btnDelete_Click(object sender, RoutedEventArgs e)
		{
			var row = dgvSourceList.SelectedItem as XyzTileSource;
			if (row != null)
			{
				XyzTileSource.RemoveSource(row);
				RefreshSources();
			}
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}


	public class XyzTileSource
	{
		public string Name { get; set; }
		public string URL { get; set; }
		public int MinZoomLevel { get; set; }
		public int MaxZoomLevel { get; set; }

		public override string ToString()
		{
			return $"{Name}|{URL}|{MinZoomLevel}|{MaxZoomLevel}";
		}

		static string SoftwelLocalAppFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Softwel\";
		static string TileFileName => Path.Combine(SoftwelLocalAppFolder, "XYZTile.txt");


		public static List<XyzTileSource> GetSourceList()
		{
			var ret = new List<XyzTileSource>();
			try
			{
				Directory.CreateDirectory(SoftwelLocalAppFolder);
				if (File.Exists(TileFileName) == false) return ret;
				using var reader = new StreamReader(TileFileName);
				while (!reader.EndOfStream)
				{
					var items = reader.ReadLine().Trim().Split("|");
					if (items.Length != 4) continue;
					var xyzTile = new XyzTileSource() { 
						Name = items[0], 
						URL = items[1], 
						MinZoomLevel = Convert.ToInt32(items[2]),
						MaxZoomLevel = Convert.ToInt32(items[3])
					};
					ret.Add(xyzTile);
				}
				reader.Close();
			}
			catch
			{

			}
			return ret;
		}


		public static void SaveSourceList(List<XyzTileSource> tiles)
		{
			Directory.CreateDirectory(SoftwelLocalAppFolder);
			var writer = new StreamWriter(TileFileName);
			foreach(var s in tiles)
			{
				writer.WriteLine(s.ToString());
			}
			writer.Close();
		}

		public static void AddSource(XyzTileSource source)
		{
			var sources = GetSourceList();
			sources.Add(source);
			SaveSourceList(sources);
		}

		public static void RemoveSource(string name)
		{
			var sources = GetSourceList();
			sources.RemoveAll(s => s.Name == name);
			SaveSourceList(sources);
		}

		public static void RemoveSource(XyzTileSource source)
		{
			RemoveSource(source.Name);
		}
	}
}
