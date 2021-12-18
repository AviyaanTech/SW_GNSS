using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using SW_GNSS.Processing;
using System.Diagnostics;
using SW_GNSS.SwMaps;
using SwMapsLib.IO;
using SwMapsLib.Data;
using ClosedXML.Excel;
using SwCad.Projections;
using SwCad.Entities;
using SwCad.TileLayers;
using System.IO.Enumeration;

namespace SW_GNSS
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Workspace.CreateDirectories();
		}

		public ObservableCollection<GnssInputFile> RoverInputFiles { get; set; } = new ObservableCollection<GnssInputFile>();
		public ObservableCollection<GnssInputFile> BaseInputFiles { get; set; } = new ObservableCollection<GnssInputFile>();

		public List<SwMapsCorrectionPoint> CorrectionPoints { get; set; } = new List<SwMapsCorrectionPoint>();
		public List<SolutionFile> SolutionFiles { get; set; } = new List<SolutionFile>();

		SwMapsProject swMapsProject = null;
		string swMapsFilePath = "";

		private void mnuExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}


		private void cmbProcessingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var baseEnabled = new int[] { 0, 1 };
			var swMapsEnabled = new int[] { 0, 2, 4 };

			if (grpBase == null || grpSwMaps == null) return;

			grpBase.IsEnabled = baseEnabled.Contains(cmbProcessingMode.SelectedIndex);
			grpSwMaps.IsEnabled = swMapsEnabled.Contains(cmbProcessingMode.SelectedIndex);
			chkSwMapsRaw.IsEnabled = swMapsEnabled.Contains(cmbProcessingMode.SelectedIndex);
		}



		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "SWGNSS"));

			dgvRoverFiles.ItemsSource = RoverInputFiles;
			dgvBaseFiles.ItemsSource = BaseInputFiles;

			AppConfig.Initialize();

			LoadBasemapLayers();
		}

		private void btnRemoveBaseData_Click(object sender, RoutedEventArgs e)
		{
			if (dgvBaseFiles.SelectedItem == null) return;
			BaseInputFiles.Remove(dgvBaseFiles.SelectedItem as GnssInputFile);
		}

		private void btnAddBaseData_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog();
			dlg.Multiselect = true;
			if (dlg.ShowDialog() == false) return;
			foreach (var fileName in dlg.FileNames)
			{
				var fileType = "";
				var ext = Path.GetExtension(fileName).ToUpper();

				if (ext == ".UBX") fileType = "UBX";
				if (ext == ".SBF") fileType = "SBF";
				if (ext == ".RTCM3") fileType = "RTCM3";
				if (ext == ".OBS") fileType = "RINEX OBS";
				if (ext == ".NAV") fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*O", ext)) fileType = "RINEX OBS";
				if (FileSystemName.MatchesSimpleExpression(".*N", ext)) fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*G", ext)) fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*L", ext)) fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*C", ext)) fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*P", ext)) fileType = "RINEX NAV";
				if (fileType == "") continue;
				var f = new GnssInputFile();
				f.Name = Path.GetFileName(fileName);
				f.FileType = fileType;
				f.FullPath = fileName;

				var existingFile = RoverInputFiles.FirstOrDefault(f => f.FullPath == fileName);
				if (existingFile != null) continue;

				BaseInputFiles.Add(f);
			}
		}

		private void btnAddRoverData_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new OpenFileDialog();
			dlg.Multiselect = true;
			if (dlg.ShowDialog() == false) return;
			foreach (var fileName in dlg.FileNames)
			{
				var fileType = "";
				var ext = Path.GetExtension(fileName).ToUpper();

				if (ext == ".UBX") fileType = "UBX";
				if (ext == ".SBF") fileType = "SBF";
				if (ext == ".RTCM3") fileType = "RTCM3";
				if (ext == ".OBS") fileType = "RINEX OBS";
				if (ext == ".NAV") fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*O", ext)) fileType = "RINEX OBS";
				if (FileSystemName.MatchesSimpleExpression(".*N", ext)) fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*G", ext)) fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*L", ext)) fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*C", ext)) fileType = "RINEX NAV";
				if (FileSystemName.MatchesSimpleExpression(".*P", ext)) fileType = "RINEX NAV";
				if (fileType == "") continue;
				var f = new GnssInputFile();
				f.Name = Path.GetFileName(fileName);
				f.FileType = fileType;
				f.FullPath = fileName;
	
				var existingFile = RoverInputFiles.FirstOrDefault(f=>f.FullPath == fileName);
				if (existingFile != null) continue;

				RoverInputFiles.Add(f);
			}
		}

		private void btnRemoveRoverData_Click(object sender, RoutedEventArgs e)
		{
			if (dgvRoverFiles.SelectedItem == null) return;
			RoverInputFiles.Remove(dgvRoverFiles.SelectedItem as GnssInputFile);
		}

		private void btnSettings_Click(object sender, RoutedEventArgs e)
		{
			var f = new FrmRtklibConfig();
			f.ShowDialog();
		}

		private async void btnProcess_Click(object sender, RoutedEventArgs e)
		{
			mapView.Points.Clear();

			grdInput.IsEnabled = false;
			var st = new Stopwatch();
			st.Start();
			var pr = new GnssProcessing();
			pr.RoverFiles = this.RoverInputFiles.ToList();
			pr.BaseFiles = this.BaseInputFiles.ToList();
			pr.ProcessingMode = (cmbProcessingMode.SelectedItem as ComboBoxItem).Content.ToString().ToLower();

			if (string.IsNullOrWhiteSpace(swMapsFilePath) == false)
			{
				swMapsProject = new SwmzReader(swMapsFilePath).Read(true);
			}
			else
			{
				swMapsProject = null;
			}

			if (chkSwMapsRaw.IsChecked == true)
			{
				if (swMapsProject == null)
				{
					Workspace.ShowErrorBox("Please select a SW Maps Project!");
					return;
				}
				pr.UseSwMapsRawFiles = true;
				pr.SwMapsProject = swMapsProject;
			}


			pr.ReportProgress += OnProgressChange;

			bool processed = await pr.ProcessAsync();

			st.Stop();

			if (processed)
			{
				Workspace.ShowInfoBox("Processing Complete in " + (st.ElapsedMilliseconds / 1000).ToString() + " seconds!");
				this.SolutionFiles = pr.SolutionFiles;
				FillSolutionOutput(pr.SolutionFiles);
				FillSwMapsOutput(pr.SolutionFiles);

				foreach (var sf in pr.SolutionFiles)
				{
					foreach (var pt in sf.Points)
					{
						var wmPt = WebMercatorProjection.ToWebMercator(pt.Latitude, pt.Longitude);
						var mapPt = new PointEntity();
						mapPt.X = wmPt.X;
						mapPt.Y = wmPt.Y;
						mapPt.Fix = pt.FixQuality;
						mapView.Points.Add(mapPt);
					}
				}
				mapView.ZoomExtents();
			}
			else
			{
				Workspace.ShowErrorBox("Processing Failed!");
			}

			grdInput.IsEnabled = true;
		}

		void FillSwMapsOutput(List<SolutionFile> solutionFiles)
		{
			if (swMapsProject == null) return;
			var pts = new List<SolutionPoint>();
			foreach (var file in solutionFiles)
			{
				pts.AddRange(file.Points);
			}

			var solSet = new GnssSolutionSet(pts);
			var correctionTool = new SwMapsCorrection(swMapsProject, solSet);

			CorrectionPoints.Clear();
			CorrectionPoints.AddRange(correctionTool.GetCorrections());
			dgvSwMapsCorrections.ItemsSource = CorrectionPoints;
			dgvSwMapsCorrections.Items.Refresh();
		}

		void FillSolutionOutput(List<SolutionFile> solutionFiles)
		{
			txtSolution.Text = "";

			var doc = new StringBuilder();
			foreach (var f in solutionFiles)
			{
				doc.AppendLine(Path.GetFileName(f.Path));
				doc.AppendLine();
				doc.AppendLine(f.Path);
				doc.AppendLine();
				doc.AppendLine(SolutionFile.SolutionHeader);

				foreach (var pt in f.Points)
				{
					doc.AppendLine(pt.SolutionLine);
				}
				doc.AppendLine();
				doc.AppendLine();
				doc.AppendLine();
			}

			txtSolution.Text = doc.ToString();
		}

		void OnProgressChange(object sender, string e)
		{
			lblStatus.Dispatcher.BeginInvoke(new Action(() =>
			{
				lblStatus.Text = e;
			}));
		}

		private void btnBrowseSwmz_Click(object sender, RoutedEventArgs e)
		{
			if (cmbProcessingMode.SelectedIndex == 1 || cmbProcessingMode.SelectedIndex == 3) return;
			var dlg = new OpenFileDialog();
			dlg.Filter = "SW Maps SWMZ File|*.swmz";
			if (dlg.ShowDialog() == false)
			{
				swmapsFilePathLabel.Text = "";
				swMapsFilePath = "";
			}
			else
			{
				swmapsFilePathLabel.Text = dlg.FileName;
				swMapsFilePath = dlg.FileName;
			}
		}

		bool ApplySwmapsCorrections()
		{
			if (CorrectionPoints.Count == 0)
			{
				Workspace.ShowErrorBox("No Corrections to Export!");
				return false;
			}

			if (swMapsProject == null)
			{
				Workspace.ShowErrorBox("No SW Maps File Selected!");
				return false;
			}

			foreach (var corrPt in CorrectionPoints)
			{
				var feature = swMapsProject.GetFeature(corrPt.FeatureID);
				if (feature == null) continue;
				var pt = feature.Points.FirstOrDefault(p => p.ID == corrPt.PointUUID);
				if (pt == null) continue;
				pt.Latitude = corrPt.ProcessedLatitude;
				pt.Longitude = corrPt.ProcessedLongitude;
				pt.Elevation = corrPt.ProcessedElevation;
				pt.OrthoHeight = corrPt.ProcessedOrthoHeight;
				pt.FixID = corrPt.FixID;
			}

			return true;
		}

		private void exportSwmz_Click(object sender, RoutedEventArgs e)
		{
			if (ApplySwmapsCorrections() == false) return;

			var dlg = new SaveFileDialog();
			dlg.Filter = "SW Maps SWMZ Project|*.swmz";
			if (dlg.ShowDialog() == false) return;

			var writer = new SwmzWriter(swMapsProject, 2);
			writer.Write(dlg.FileName);
		}

		private void exportXls_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new SaveFileDialog();
			dlg.Filter = "Excel File|*.xlsx";
			if (dlg.ShowDialog() == false) return;

			var book = new XLWorkbook();

			foreach (var solFile in SolutionFiles)
			{
				if (solFile.Points.Count == 0) continue;

				var utmZone = TransverseMercatorProjection.GetUTMZone(solFile.Points.First().Longitude);
				var utm = TransverseMercatorProjection.UTM(utmZone);


				var sheetName = Path.GetFileNameWithoutExtension(solFile.Path);
				int i = 1;
				while (book.TryGetWorksheet(sheetName, out _))
				{
					sheetName = Path.GetFileNameWithoutExtension(solFile.Path) + "_" + i;
					i++;
				}
				var sheet = book.AddWorksheet(sheetName);

				sheet.Cell(1, 1).Value = "Date";
				sheet.Cell(1, 2).Value = "Time";
				sheet.Cell(1, 3).Value = "Latitude";
				sheet.Cell(1, 4).Value = "Longitude";
				sheet.Cell(1, 5).Value = "Ell. Height";
				sheet.Cell(1, 6).Value = "Fix";
				sheet.Cell(1, 7).Value = "Satellites";
				sheet.Cell(1, 8).Value = "SDN";
				sheet.Cell(1, 9).Value = "SDE";
				sheet.Cell(1, 10).Value = "SDU";
				sheet.Cell(1, 11).Value = $"UTM {utmZone} X";
				sheet.Cell(1, 12).Value = $"UTM {utmZone} Y";

				int row = 2;

				foreach (var pt in solFile.Points)
				{
					sheet.Cell(row, 1).Value = pt.Time.ToString("yyyy/MM/dd");
					sheet.Cell(row, 2).Value = pt.Time.ToString("HH:mm:ss.fff");
					sheet.Cell(row, 3).Value = Math.Round(pt.Latitude, 9);
					sheet.Cell(row, 4).Value = Math.Round(pt.Longitude, 9);
					sheet.Cell(row, 5).Value = Math.Round(pt.Elevation, 3);
					sheet.Cell(row, 6).Value = pt.FixQuality.StringLabel();
					sheet.Cell(row, 7).Value = pt.NumSat;
					sheet.Cell(row, 8).Value = Math.Round(pt.SDN, 4);
					sheet.Cell(row, 9).Value = Math.Round(pt.SDE, 4);
					sheet.Cell(row, 10).Value = Math.Round(pt.SDU, 4);
					var utmPt = utm.LatLngToXY(pt.Latitude, pt.Longitude);
					sheet.Cell(row, 11).Value = Math.Round(utmPt.X, 3);
					sheet.Cell(row, 12).Value = Math.Round(utmPt.Y, 3);
					row++;
				}

				sheet.Column(3).Style.NumberFormat.SetFormat("0.000000000");
				sheet.Column(4).Style.NumberFormat.SetFormat("0.000000000");
				sheet.Column(5).Style.NumberFormat.SetFormat("0.000");
				sheet.Column(11).Style.NumberFormat.SetFormat("0.000");
				sheet.Column(12).Style.NumberFormat.SetFormat("0.000");

				for (int c = 1; c <= 12; c++)
				{
					sheet.Column(c).AdjustToContents(1, 8.5, 50000);
				}
			}
			book.SaveAs(dlg.FileName);
			Workspace.ShowInfoBox("Excel File Exported!");
		}

		private void exportShp_Click(object sender, RoutedEventArgs e)
		{
			if (ApplySwmapsCorrections() == false) return;

			var dlg = new SaveFileDialog();
			dlg.Filter = "Shapefiles Zip|*.zip";
			if (dlg.ShowDialog() == false) return;

			var writer = new SwMapsLib.Conversions.Shapefile.SwMapsShapefileWriter(swMapsProject);
			writer.Export(dlg.FileName);
			Workspace.ShowInfoBox("Shapefiles Exported!");
		}

		private void exportKmz_Click(object sender, RoutedEventArgs e)
		{
			if (ApplySwmapsCorrections() == false) return;

			var dlg = new SaveFileDialog();
			dlg.Filter = "KMZ|*.kmz";
			if (dlg.ShowDialog() == false) return;

			var writer = new SwMapsLib.Conversions.KMZ.SwMapsKmzWriter(swMapsProject);
			writer.WriteKmz(dlg.FileName);
			Workspace.ShowInfoBox("KMZ Exported!");
		}

		private void exportGpkg_Click(object sender, RoutedEventArgs e)
		{
			if (ApplySwmapsCorrections() == false) return;

			var dlg = new SaveFileDialog();
			dlg.Filter = "Geopackage|*.gpkg";
			if (dlg.ShowDialog() == false) return;

			var writer = new SwMapsLib.Conversions.GPKG.SwMapsGpkgWriter(swMapsProject);
			writer.Export(dlg.FileName);
			Workspace.ShowInfoBox("Geopackage Exported!");
		}

		private void mnuTileSources_Click(object sender, RoutedEventArgs e)
		{
			var dlg = new FrmBackgroundMapList();
			dlg.ShowDialog();
			LoadBasemapLayers();
		}

		void LoadBasemapLayers()
		{
			var currentItem = cmbBgMap.SelectedItem?.ToString();

			cmbBgMap.Items.Clear();

			cmbBgMap.Items.Add("None");
			var layers = XyzTileSource.GetSourceList();
			foreach (var l in layers)
			{
				cmbBgMap.Items.Add(l.Name);
			}

			if (currentItem != null)
			{
				if (cmbBgMap.Items.Contains(currentItem))
				{
					cmbBgMap.SelectedItem = currentItem;
				}
			}

			if (cmbBgMap.SelectedIndex == -1)
				cmbBgMap.SelectedIndex = 0;
		}

		private void cmbBgMap_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selLayer = cmbBgMap.SelectedItem;
			if (cmbBgMap.SelectedIndex == -1) cmbBgMap.SelectedIndex = 0;
			if (cmbBgMap.SelectedIndex <= 0)
			{
				mapView.BaseMap = null;
			}
			else
			{
				var layerName = cmbBgMap.SelectedItem?.ToString();
				var layer = XyzTileSource.GetSourceList().FirstOrDefault(s => s.Name == layerName);
				var tileProv = new UrlTileProvider(layer.URL, layer.Name, layer.MaxZoomLevel);
				mapView.BaseMap = new OnlineTileLayer(mapView, layerName, tileProv, true);

			}
			mapView.UpdateBaseMap();
			mapView.Invalidate();
		}

		private void exportRawKmz_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(swMapsFilePath))
			{
				Workspace.ShowErrorBox("SW Maps file not selected!");
				return;
			}

			if (File.Exists(swMapsFilePath) == false)
			{
				Workspace.ShowErrorBox("SW Maps file does not exist!");
				return;
			}

			var reader = new SwmzReader(swMapsFilePath);
			var project = reader.Read(true);
			var dlg = new SaveFileDialog();
			dlg.Filter = "KMZ|*.kmz";
			if (dlg.ShowDialog() == false) return;

			var writer = new SwMapsLib.Conversions.KMZ.SwMapsKmzWriter(project);
			writer.WriteKmz(dlg.FileName);
			Workspace.ShowInfoBox("KMZ Exported!");
		}

		private void exportRawShp_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(swMapsFilePath))
			{
				Workspace.ShowErrorBox("SW Maps file not selected!");
				return;
			}

			if (File.Exists(swMapsFilePath) == false)
			{
				Workspace.ShowErrorBox("SW Maps file does not exist!");
				return;
			}
			var reader = new SwmzReader(swMapsFilePath);
			var project = reader.Read(true);
			var dlg = new SaveFileDialog();
			dlg.Filter = "Shapefile Zip|*.zip";
			if (dlg.ShowDialog() == false) return;

			var writer = new SwMapsLib.Conversions.Shapefile.SwMapsShapefileWriter(project);
			writer.Export(dlg.FileName);
			Workspace.ShowInfoBox("Shapefiles Exported!");
		}

		private void exportRawGpkg_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(swMapsFilePath))
			{
				Workspace.ShowErrorBox("SW Maps file not selected!");
				return;
			}

			if (File.Exists(swMapsFilePath) == false)
			{
				Workspace.ShowErrorBox("SW Maps file does not exist!");
				return;
			}

			var reader = new SwmzReader(swMapsFilePath);
			var project = reader.Read(true);
			var dlg = new SaveFileDialog();
			dlg.Filter = "Geopackage (GPKG)|*.gpkg";
			if (dlg.ShowDialog() == false) return;

			var writer = new SwMapsLib.Conversions.GPKG.SwMapsGpkgWriter(project);
			writer.Export(dlg.FileName);
			Workspace.ShowInfoBox("Geopackage Exported!");
		}

		private void mnuAbout_Click(object sender, RoutedEventArgs e)
		{
			var f = new FrmAbout();
			f.ShowDialog();
		}

		private void mnuRtkPlot_Click(object sender, RoutedEventArgs e)
		{
			foreach (var solFile in SolutionFiles)
			{
				if (File.Exists(solFile.Path))
				{
					Process.Start("RTKLIB\\rtkplot.exe","\"" + solFile.Path + "\"");
				}
			}

		}
	}
}
