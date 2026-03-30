using System.Collections.Generic;

namespace PathManager.Core
{
    public class OverThresholdPath
    {
        public string RelativePath;
        public int CharCount;
        public int ExcessChars;
    }

    public class ScanReport
    {
        public string RootPath;
        public ulong TotalFiles = 0;
        public ulong TotalFolders = 0;
        public ulong TotalSizeBytes = 0;
        public int ThresholdLimit = 0;
        public List<OverThresholdPath> BadPaths = new List<OverThresholdPath>();
    }
}
