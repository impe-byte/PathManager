using System.IO;
using System.Text;
using PathManager.Core;
using PathManager.Localization;

namespace PathManager.Exporters
{
    public class CsvExporter : IReportExporter
    {
        public void Export(ScanReport report, string outputPath)
        {
            using (var sw = new StreamWriter(outputPath, false, new UTF8Encoding(true)))
            {
                sw.WriteLine(string.Format("{0},{1},\"{2}\"", Strings.Get("ColExcess"), Strings.Get("ColTotal"), Strings.Get("ColRelative")));
                foreach (var bp in report.BadPaths)
                {
                    sw.WriteLine(string.Format("{0},{1},\"{2}\"", bp.ExcessChars, bp.CharCount, bp.RelativePath.Replace("\"", "\"\"")));
                }
            }
        }
    }
}
