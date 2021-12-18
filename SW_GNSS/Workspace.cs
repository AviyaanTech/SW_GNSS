using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

namespace SW_GNSS
{
	internal static class Workspace
	{
		public static MainWindow MainForm => (MainWindow)App.Current.MainWindow;
		public const string AppName = "SW GNSS";
		public const string ProductId = "SW_GNSS";

		public static string AppTempDir => Path.Combine(Path.GetTempPath(), ProductId);
		public static string PPPDir => Path.Combine(AppTempDir, "PPP_Corrections");
		public static string DCBDir => Path.Combine(AppTempDir, "DCB");

		public static void CreateDirectories()
		{
			Directory.CreateDirectory(AppTempDir);
			Directory.CreateDirectory(PPPDir);
		}
		public static void ShowErrorBox(string msg)
		{
			if (MainForm != null)
			{
				MainForm.Dispatcher.Invoke(() =>
				{
					MessageBox.Show(msg, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
				});
			}
			else
			{
				MessageBox.Show(msg, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public static void ShowInfoBox(string msg)
		{
			if (MainForm != null)
			{
				MainForm.Dispatcher.Invoke(() =>
				{
					MessageBox.Show(msg, AppName, MessageBoxButton.OK, MessageBoxImage.Information);
				});
			}
			else
			{
				MessageBox.Show(msg, AppName, MessageBoxButton.OK, MessageBoxImage.Information);
			}
		}

	}
}
