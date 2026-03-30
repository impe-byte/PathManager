using System;
using System.IO;
using System.Runtime.InteropServices;
using PathManager.Localization;

namespace PathManager.Core
{
    public class PathScanner
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr FindFirstFileW(string lpFileName, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool FindNextFileW(IntPtr hFindFile, out WIN32_FIND_DATAW lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FindClose(IntPtr hFindFile);

        const long INVALID_HANDLE_VALUE = -1;
        const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        const uint FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;

        public ScanReport RunScan(string rootInput, int threshold, IProgress<string> progress = null)
        {
            string absRoot = Path.GetFullPath(rootInput);
            int rootLength = absRoot.Length;
            if (absRoot.EndsWith("\\") || absRoot.EndsWith("/")) rootLength--;

            string longAbsRoot = absRoot;
            if (!longAbsRoot.StartsWith(@"\\?\"))
            {
                if (longAbsRoot.StartsWith(@"\\"))
                    longAbsRoot = @"\\?\UNC\" + longAbsRoot.Substring(2);
                else
                    longAbsRoot = @"\\?\" + longAbsRoot;
            }

            ScanReport report = new ScanReport();
            report.RootPath = rootInput;
            report.ThresholdLimit = threshold;

            int reportProgressCounter = 0;

            Action<string> scanDirectory = null;
            scanDirectory = new Action<string>((currentDir) =>
            {
                report.TotalFolders++;
                reportProgressCounter++;

                if (progress != null && reportProgressCounter % 500 == 0)
                {
                    progress.Report(currentDir.Replace(@"\\?\", ""));
                }

                string searchFilter = currentDir;
                if (!searchFilter.EndsWith("\\")) searchFilter += "\\";
                searchFilter += "*";

                WIN32_FIND_DATAW findData;
                IntPtr hFind = FindFirstFileW(searchFilter, out findData);

                if (hFind.ToInt64() != INVALID_HANDLE_VALUE)
                {
                    do
                    {
                        string fileName = findData.cFileName;
                        if (fileName == "." || fileName == "..") continue;

                        string fullPath = currentDir;
                        if (!fullPath.EndsWith("\\")) fullPath += "\\";
                        fullPath += fileName;

                        bool isDir = (findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == FILE_ATTRIBUTE_DIRECTORY;
                        bool isReparse = (findData.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) == FILE_ATTRIBUTE_REPARSE_POINT;

                        if (isDir)
                        {
                            if (!isReparse)
                            {
                                string printPath = fullPath;
                                if (printPath.StartsWith(@"\\?\UNC\")) printPath = @"\\" + printPath.Substring(8);
                                else if (printPath.StartsWith(@"\\?\")) printPath = printPath.Substring(4);

                                int relLen = printPath.Length - rootLength - 1;
                                if (relLen > threshold)
                                {
                                    string relStr = (rootLength + 1 < printPath.Length) ? printPath.Substring(rootLength + 1) : printPath;
                                    report.BadPaths.Add(new OverThresholdPath { RelativePath = relStr, CharCount = relLen, ExcessChars = relLen - threshold });
                                }

                                scanDirectory(fullPath);
                            }
                        }
                        else
                        {
                            report.TotalFiles++;
                            ulong fileSize = ((ulong)findData.nFileSizeHigh << 32) | findData.nFileSizeLow;
                            report.TotalSizeBytes += fileSize;

                            string fPrintPath = fullPath;
                            if (fPrintPath.StartsWith(@"\\?\UNC\")) fPrintPath = @"\\" + fPrintPath.Substring(8);
                            else if (fPrintPath.StartsWith(@"\\?\")) fPrintPath = fPrintPath.Substring(4);

                            int fLen = fPrintPath.Length - rootLength - 1;
                            if (fLen > threshold)
                            {
                                string relStr = (rootLength + 1 < fPrintPath.Length) ? fPrintPath.Substring(rootLength + 1) : fPrintPath;
                                report.BadPaths.Add(new OverThresholdPath { RelativePath = relStr, CharCount = fLen, ExcessChars = fLen - threshold });
                            }
                        }

                    } while (FindNextFileW(hFind, out findData));

                    FindClose(hFind);
                }
            });

            scanDirectory(longAbsRoot);
            if (report.TotalFolders > 0) report.TotalFolders--;

            return report;
        }
    }
}
