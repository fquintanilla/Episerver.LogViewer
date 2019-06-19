using System;
using System.Configuration;
using System.Web.Mvc;
using EPiServer.PlugIn;
using Kamsar.WebConsole;

namespace LogViewer.Controllers
{
    [Authorize(Roles = "Administrators,WebAdmins")]
    [GuiPlugIn(Area = PlugInArea.AdminMenu, UrlFromModuleFolder = "LogViewer", DisplayName = "Live Log Viewer")]
    public class VerndaleLogViewerController : Controller
    {
        public ActionResult Index()
        {
            var startTime = DateTime.Now;
            var portNumber = ConfigurationManager.AppSettings["LogViewerPort"];
            var duration = ConfigurationManager.AppSettings["LogViewerDuration"];

            #region Get parameters

            if (string.IsNullOrEmpty(portNumber))
            {
                portNumber = "878";
            }

            if (!int.TryParse(portNumber, out var port))
            {
                port = 878;
            }

            if (string.IsNullOrEmpty(duration))
            {
                duration = "120";
            }

            if (!int.TryParse(duration, out var durationValue))
            {
                durationValue = 120;
            }

            var durationTimeSpan = TimeSpan.FromSeconds(durationValue);

            #endregion
            
            var processor = new Html5WebConsole(Response)
            {
                Title = $"Live Log Viewer (Port {port}) during " + ToReadableString(durationTimeSpan)
            };

            processor.Render(progress =>
            {
                var s = new UDPSocket();
                s.Server("127.0.0.1", 878, progress);

                while (startTime.Add(durationTimeSpan) > DateTime.Now)
                {

                }

                progress.ReportStatus("Paused.");
                progress.ReportStatus("Refresh to resume live tail.");

                // Reset socket to stop sending messages.
                s.Stop();
            });

            return Content("");
        }

        private string ToReadableString(TimeSpan span)
        {
            var formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? $"{span.Days:0} day{(span.Days == 1 ? string.Empty : "s")}, " : string.Empty,
                span.Duration().Hours > 0 ? $"{span.Hours:0} hour{(span.Hours == 1 ? string.Empty : "s")}, " : string.Empty,
                span.Duration().Minutes > 0 ? $"{span.Minutes:0} minute{(span.Minutes == 1 ? string.Empty : "s")}, " : string.Empty,
                span.Duration().Seconds > 0 ? $"{span.Seconds:0} second{(span.Seconds == 1 ? string.Empty : "s")}" : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }
    }
}