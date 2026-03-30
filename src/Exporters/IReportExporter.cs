using PathManager.Core;

namespace PathManager.Exporters
{
    public interface IReportExporter
    {
        void Export(ScanReport report, string outputPath);
    }
}
