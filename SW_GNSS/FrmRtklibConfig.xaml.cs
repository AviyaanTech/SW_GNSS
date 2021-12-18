using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
	/// Interaction logic for FrmRtklibConfig.xaml
	/// </summary>
	public partial class FrmRtklibConfig : Window
	{
		public Dictionary<string, TextBox> InputControls = new Dictionary<string, TextBox>();
		public List<string> RoverAntennas { get; set; }
		public List<string>BaseAntennas { get; set; }
		public FrmRtklibConfig()
		{
			InitializeComponent();
		}

		ProcessingConfig config;
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			config = ProcessingConfig.LoadConfig();

			RoverAntennas = new List<string>(AppConfig.Antennas);
			BaseAntennas = new List<string>(AppConfig.Antennas);
			cmbRoverAnt.ItemsSource = RoverAntennas;
			cmbBaseAnt.ItemsSource = BaseAntennas;


			numElevMask.Value = config.ElevationMask;
			numSnr.Value = config.SnrMask;

			numBaseLat.Value = config.BaseLatitude;
			numBaseLon.Value = config.BaseLongitude;
			numBaseAlt.Value = config.BaseElevation;

			numBaseE.Value = config.BaseDeltaE;
			numBaseN.Value = config.BaseDeltaN;
			numBaseU.Value = config.BaseDeltaU;

			numRoverE.Value = config.RoverDeltaE;
			numRoverN.Value = config.RoverDeltaN;
			numRoverU.Value = config.RoverDeltaU;

			cmbBaseAnt.SelectedItem = config.BaseAntennaModel;
			cmbRoverAnt.SelectedItem = config.RoverAntennaModel;

			chkPosFromHeader.IsChecked = config.UseRinexHeaderPosition;

			chkGPS.IsChecked = config.EnableGPS;
			chkGLO.IsChecked = config.EnableGLO;
			chkGAL.IsChecked = config.EnableGAL;
			chkBDS.IsChecked = config.EnableBDS;
			chkQZSS.IsChecked = config.EnableQZS;
			chkSBAS.IsChecked = config.EnableSBAS;
			chkIRNSS.IsChecked = config.EnableIRNSS;

			chkStaticSingle.IsChecked = config.StaticSingle;

		}

		private void cmbRoverAnt_KeyUp(object sender, KeyEventArgs e)
		{
			CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(cmbRoverAnt.ItemsSource);

			itemsViewOriginal.Filter = ((o) =>
			{
				if (String.IsNullOrEmpty(cmbRoverAnt.Text)) return true;
				else
				{
					if (((string)o).Contains(cmbRoverAnt.Text.ToUpper())) return true;
					else return false;
				}
			});

			itemsViewOriginal.Refresh();

		}

		private void cmbBaseAnt_KeyUp(object sender, KeyEventArgs e)
		{
			CollectionView itemsViewOriginal = (CollectionView)CollectionViewSource.GetDefaultView(cmbBaseAnt.ItemsSource);

			itemsViewOriginal.Filter = ((o) =>
			{
				if (String.IsNullOrEmpty(cmbBaseAnt.Text)) return true;
				else
				{
					if (((string)o).Contains(cmbBaseAnt.Text.ToUpper())) return true;
					else return false;
				}
			});

			itemsViewOriginal.Refresh();

		}

		private void btnSave_Click(object sender, RoutedEventArgs e)
		{
			config.ElevationMask = numElevMask.Value ?? 10;
			config.SnrMask = numSnr.Value ?? 0;
			
			config.BaseLatitude = numBaseLat.Value ?? 0;
			config.BaseLongitude = numBaseLon.Value ?? 0;
			config.BaseElevation = numBaseAlt.Value ?? 0;
			
			config.BaseDeltaE = numBaseE.Value ?? 0;
			config.BaseDeltaN = numBaseN.Value ?? 0;
			config.BaseDeltaU = numBaseU.Value ?? 0;
			
			config.RoverDeltaE = numRoverE.Value ?? 0;
			config.RoverDeltaN = numRoverN.Value ?? 0;
			config.RoverDeltaU = numRoverU.Value ?? 0;
			
			config.BaseAntennaModel = cmbBaseAnt.SelectedItem?.ToString() ?? "";
			config.RoverAntennaModel = cmbRoverAnt.SelectedItem?.ToString() ?? "";
			
			config.UseRinexHeaderPosition = chkPosFromHeader.IsChecked == true;
			
			config.EnableGPS = chkGPS.IsChecked == true;
			config.EnableGLO = chkGLO.IsChecked == true;
			config.EnableGAL = chkGAL.IsChecked == true;
			config.EnableBDS = chkBDS.IsChecked == true;
			config.EnableQZS = chkQZSS.IsChecked == true;
			config.EnableSBAS = chkSBAS.IsChecked == true;
			config.EnableIRNSS = chkIRNSS.IsChecked == true;

			config.StaticSingle = chkStaticSingle.IsChecked == true;

			config.SaveConfig();
			this.DialogResult = true;
			this.Close();

		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

	}
}
