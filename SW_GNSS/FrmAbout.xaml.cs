using System;
using System.Collections.Generic;
using System.Diagnostics;
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
	/// Interaction logic for FrmAbout.xaml
	/// </summary>
	public partial class FrmAbout : Window
	{
		public FrmAbout()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
			lblVersion.Text = String.Format("Version {0}", version);
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			var proc = new Process();
			proc.StartInfo.UseShellExecute = true;
			proc.StartInfo.FileName = e.Uri.AbsoluteUri;
			proc.Start();

			e.Handled = true;
		}
	}
}
