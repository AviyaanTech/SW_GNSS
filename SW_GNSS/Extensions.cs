using SW_GNSS.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace SW_GNSS
{
    public static class RichTextBoxExtensions
	{
        static FontFamily consolas = new FontFamily("Consolas");
        public static void AppendLine(this FlowDocument doc, string text="", SolidColorBrush brush=null, bool bold = false)
        {
            if (brush == null) brush = Brushes.Black;
            TextRange tr = new TextRange(doc.ContentEnd, doc.ContentEnd);
            tr.Text = text+ "\n";
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                tr.ApplyPropertyValue(TextElement.FontFamilyProperty, consolas);


            }
            catch (FormatException) { }
        }

        public static int NmeaFixQuality(this FixQuality fq)
		{
            if (fq == FixQuality.Single) return 1;
            if (fq == FixQuality.DGPS) return 2;
            if (fq == FixQuality.PPP) return 3;
            if (fq == FixQuality.Fix) return 4;
            if (fq == FixQuality.Float) return 5;
            if (fq == FixQuality.SBAS) return 9;
            return 1;
        }

        public static string StringLabel(this FixQuality fq)
        {
            if (fq == FixQuality.Single) return "SINGLE";
            if (fq == FixQuality.DGPS) return "DGPS";
            if (fq == FixQuality.PPP) return "PPP";
            if (fq == FixQuality.Fix) return "FIX";
            if (fq == FixQuality.Float) return "FLOAT";
            if (fq == FixQuality.SBAS) return "SBAS";
            return "INVALID";
        }
    }
    public static class WebClientExtensions
    {
        public static async Task DownloadFileTaskAsync(
            this WebClient webClient,
            Uri address,
            string fileName,
            long expectedLength,
            IProgress<Tuple<long, int, long>> progress)
        {
            // Create the task to be returned
            var tcs = new TaskCompletionSource<object>(address);

            // Setup the callback event handler handlers
            AsyncCompletedEventHandler completedHandler = (cs, ce) =>
            {
                if (ce.UserState == tcs)
                {
                    if (ce.Error != null) tcs.TrySetException(ce.Error);
                    else if (ce.Cancelled) tcs.TrySetCanceled();
                    else tcs.TrySetResult(null);
                }
            };

            DownloadProgressChangedEventHandler progressChangedHandler = (ps, pe) =>
            {
                if (pe.UserState == tcs)
                {
                    var percent = pe.ProgressPercentage;
                    var totalBytes = pe.TotalBytesToReceive;
                    
                    if(pe.TotalBytesToReceive == -1)
					{
                        totalBytes = expectedLength;
                        percent = (int)(pe.BytesReceived * 100 / totalBytes);
					}
                    progress.Report(
                        Tuple.Create(
                            pe.BytesReceived,
                            percent,
                            totalBytes));
                }
            };

            try
            {
                webClient.DownloadFileCompleted += completedHandler;
                webClient.DownloadProgressChanged += progressChangedHandler;

                webClient.DownloadFileAsync(address, fileName, tcs);

                await tcs.Task;
            }
            finally
            {
                webClient.DownloadFileCompleted -= completedHandler;
                webClient.DownloadProgressChanged -= progressChangedHandler;
            }
        }
    }
}
