using SwCad.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SW_GNSS.Processing
{
	public class RinexConverter
	{
		public event EventHandler<string> ReportProgress;

		public string Format { get; set; }
		public string AntennaModel { get; set; }
		public Vector3D ApproximatePosition { get; set; } //Approximate position, in XYZ ECEF
		public Vector3D AntennaDelta { get; set; }// Antenna Delta, ENU
		public string OutputDir { get; set; }
		public string FilePath { get; set; }

		public async Task ConvertToRinex(CancellationToken token)
		{
			var arguments = new List<string>();
			arguments.Add("-od");
			arguments.Add("-os");
			arguments.Add("-oi");
			arguments.Add("-ot");
			arguments.Add("-f 3");
			arguments.Add("-ts 2010/01/01 00:00:00");
			arguments.Add($"-r {Format}");

			if (ApproximatePosition != null)
				arguments.Add($"-hp {ApproximatePosition.X.ToString("0.0000")}/{ApproximatePosition.Y.ToString("0.0000")}/{ApproximatePosition.Z.ToString("0.0000")}");

			if (AntennaDelta != null)
			{
				arguments.Add($"-hd {AntennaDelta.Z.ToString("0.0000")}/{AntennaDelta.X.ToString("0.0000")}/{AntennaDelta.Y.ToString("0.0000")}");
			}

			arguments.Add("-v 3.03");

			if (AntennaModel != "*" && AntennaModel != "") arguments.Add($"-ha {AntennaModel}/NONE");
			arguments.Add($"-d \"{OutputDir}\"");
			arguments.Add($"-hc SW_GNSS");
			arguments.Add($"\"{FilePath}\"");

			var args = String.Join(" ", arguments);



			//* Create your Process
			Process process = new Process();
			process.StartInfo.FileName = "./RTKLIB/convbin.exe";
			process.StartInfo.Arguments = args; // $"-od -os -oi -ot -f 1 \"{path}\"";
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			//* Set your output and error (asynchronous) handlers
			process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
			process.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);
			//* Start process and handlers
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			token.Register(() =>
			{
				try { process.Kill(); } catch { }
			});
			await process.WaitForExitAsync(token);

			Console.WriteLine("File conversion complete.\r\n");
		}

		void ErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			ReportProgress?.Invoke(sendingProcess, outLine.Data);
			Console.WriteLine(outLine.Data);
		}
		void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			ReportProgress?.Invoke(sendingProcess, outLine.Data);
			Console.WriteLine(outLine.Data);
		}

	}
}
