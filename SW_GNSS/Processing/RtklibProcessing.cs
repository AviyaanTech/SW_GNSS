using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SW_GNSS.Processing
{
	class RtklibProcessing
	{
		public event EventHandler<string> ReportProgress;

		

		public async Task<string> ProcessAsync(string roverObsPath, IEnumerable<string> baseObsPaths, IEnumerable<string> navPaths, CancellationToken token)
		{
			string BaseFileDir = Path.GetTempFileName();
			File.Delete(BaseFileDir);
			Directory.CreateDirectory(BaseFileDir);
			foreach (var f in baseObsPaths)
				File.Copy(f, Path.Combine(BaseFileDir, Path.GetFileName(f)));

			foreach (var f in navPaths)
				File.Copy(f, Path.Combine(BaseFileDir, Path.GetFileName(f)));


			var roverDir = Path.GetDirectoryName(roverObsPath);
			var FileName = Path.GetFileNameWithoutExtension(roverObsPath) + ".pos";
			var outputFileName = Path.Combine(roverDir, FileName);

			var arguments = new List<string>();
			arguments.Add($"-k \"{RtklibConfig.ConfigOutFile}\"");
			arguments.Add($"-o \"{outputFileName}\"");
			arguments.Add($"\"{roverObsPath}\"");
			arguments.Add($"\"{BaseFileDir}\\*.*\"");


			var args = String.Join(" ", arguments);

			ReportProgress?.Invoke(this, "Reading RINEX Files");

			Process process = new Process();
			process.StartInfo.FileName = "./RTKLIB/rnx2rtkp.exe";
			process.StartInfo.Arguments = args;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			//* Set your output and error (asynchronous) handlers
			process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
			process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
			//* Start process and handlers
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			token.Register(() =>
			{
				try { process.Kill(); } catch { }
			});
			await process.WaitForExitAsync(token);

			Directory.Delete(BaseFileDir, true);
			return outputFileName;
		}


		void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
		{
			if (outLine.Data != "")
				ReportProgress?.Invoke(sendingProcess, outLine.Data);
			Console.WriteLine(outLine.Data);
		}
	}
}
