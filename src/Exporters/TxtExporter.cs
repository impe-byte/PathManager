using System.IO;
using System.Text;
using PathManager.Core;
using PathManager.Localization;

namespace PathManager.Exporters
{
    public class TxtExporter : IReportExporter
    {
        public void Export(ScanReport report, string outputPath)
        {
            using (var sw = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                sw.WriteLine(string.Format("[{0}]", Strings.Get("StatsGlobal")));
                sw.WriteLine(string.Format("{0} {1}", Strings.Get("StatsRoot"), report.RootPath));
                sw.WriteLine(string.Format("{0} {1}", Strings.Get("StatsFolders"), report.TotalFolders));
                sw.WriteLine(string.Format("{0} {1}", Strings.Get("StatsFiles"), report.TotalFiles));
                sw.WriteLine(string.Format("{0} {1}", Strings.Get("StatsSize"), Strings.FormatSize(report.TotalSizeBytes)));
                sw.WriteLine(string.Format("{0} {1}", Strings.Get("StatsThreshold"), report.ThresholdLimit));
                sw.WriteLine(string.Format("{0} {1}\n", Strings.Get("StatsViolations"), report.BadPaths.Count));

                if (report.BadPaths.Count > 0)
                {
                    sw.WriteLine(string.Format("[{0}]", Strings.Get("DetailViolations")));
                    foreach (var bp in report.BadPaths)
                    {
                        sw.WriteLine(string.Format("[+{0}] (Tot: {1}) - {2}", bp.ExcessChars, bp.CharCount, bp.RelativePath));
                    }
                }
            }
        }
    }
}
