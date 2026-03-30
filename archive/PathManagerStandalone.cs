using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PathManagerGUI
{
    public class MainForm : Form
    {
        private TextBox txtPath;
        private Button btnBrowse;
        private NumericUpDown numThreshold;
        private Button btnScan;
        private Button btnOpenTxt;
        private Button btnOpenCsv;
        private Label lblStatus;
        private ProgressBar progressBar;

        class OverThresholdPath
        {
            public string relativePath;
            public int charCount;
            public int excessChars;
        }

        class ScanReport
        {
            public ulong totalFiles = 0;
            public ulong totalFolders = 0;
            public ulong totalSizeBytes = 0;
            public int thresholdLimit = 0;
            public List<OverThresholdPath> badPaths = new List<OverThresholdPath>();
        }

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

        public MainForm()
        {
            this.Text = "Path Manager (Standalone Edition)";
            this.Size = new Size(500, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.BackColor = Color.FromArgb(245, 245, 245);

            Label lblTitle = new Label() { Text = "Path Manager Scanner", Location = new Point(20, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold) };
            this.Controls.Add(lblTitle);

            Label lbl1 = new Label() { Text = "Percorso Radice (Root):", Location = new Point(20, 45), AutoSize = true };
            this.Controls.Add(lbl1);

            txtPath = new TextBox() { Location = new Point(20, 65), Width = 360, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(txtPath);

            btnBrowse = new Button() { Text = "Sfoglia...", Location = new Point(390, 64), Width = 75, Height = 28, BackColor = Color.White };
            btnBrowse.Click += BtnBrowse_Click;
            btnBrowse.Cursor = Cursors.Hand;
            this.Controls.Add(btnBrowse);

            Label lbl2 = new Label() { Text = "Soglia massima caratteri:", Location = new Point(20, 105), AutoSize = true };
            this.Controls.Add(lbl2);

            numThreshold = new NumericUpDown() { Location = new Point(20, 125), Width = 100, Minimum = 1, Maximum = 32767, Value = 150, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(numThreshold);

            btnScan = new Button() { Text = "Avvia Scansione Completa", Location = new Point(20, 170), Width = 445, Height = 40, Font = new Font("Segoe UI Semibold", 10F), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White };
            btnScan.Click += BtnScan_Click;
            btnScan.FlatStyle = FlatStyle.Flat;
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Cursor = Cursors.Hand;
            this.Controls.Add(btnScan);

            progressBar = new ProgressBar() { Location = new Point(20, 220), Width = 445, Height = 6, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 0, Visible = false };
            this.Controls.Add(progressBar);

            lblStatus = new Label() { Text = "Pronto all'uso. Nessun motore esterno richiesto.", Location = new Point(20, 235), Width = 445, ForeColor = Color.Gray };
            this.Controls.Add(lblStatus);

            btnOpenTxt = new Button() { Text = "Apri Report TXT", Location = new Point(20, 260), Width = 215, Height = 30, Enabled = false, BackColor = Color.White };
            btnOpenTxt.Click += (s, e) => OpenFile("Report_PathManager.txt");
            this.Controls.Add(btnOpenTxt);

            btnOpenCsv = new Button() { Text = "Apri Report CSV", Location = new Point(250, 260), Width = 215, Height = 30, Enabled = false, BackColor = Color.White };
            btnOpenCsv.Click += (s, e) => OpenFile("Report_PathManager.csv");
            this.Controls.Add(btnOpenCsv);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Seleziona la cartella radice da scansionare";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtPath.Text = fbd.SelectedPath;
                }
            }
        }

        private async void BtnScan_Click(object sender, EventArgs e)
        {
            string rootInput = txtPath.Text.Trim();
            if (rootInput.StartsWith("\"") && rootInput.EndsWith("\""))
                rootInput = rootInput.Substring(1, rootInput.Length - 2);

            if (string.IsNullOrWhiteSpace(rootInput) || !Directory.Exists(rootInput))
            {
                MessageBox.Show("Seleziona una cartella radice valida e accessibile.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnScan.Enabled = false;
            btnOpenTxt.Enabled = false;
            btnOpenCsv.Enabled = false;
            lblStatus.Text = "Scansione nativa in corso... Attendere.";
            lblStatus.ForeColor = Color.FromArgb(0, 120, 215);
            progressBar.Visible = true;
            progressBar.MarqueeAnimationSpeed = 30;

            int threshold = (int)numThreshold.Value;

            try
            {
                await Task.Run(new Action(() => PerformScan(rootInput, threshold)));

                lblStatus.Text = "Scansione completata con successo!";
                lblStatus.ForeColor = Color.Green;

                if (File.Exists("Report_PathManager.txt")) btnOpenTxt.Enabled = true;
                if (File.Exists("Report_PathManager.csv")) btnOpenCsv.Enabled = true;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Errore durante la scansione.";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Errore di avvio scansione: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnScan.Enabled = true;
                progressBar.Visible = false;
                progressBar.MarqueeAnimationSpeed = 0;
            }
        }

        private void PerformScan(string rootInput, int threshold)
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
            report.thresholdLimit = threshold;

            Action<string> scanDirectory = null;
            scanDirectory = new Action<string>((currentDir) =>
            {
                report.totalFolders++;
                
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
                                    report.badPaths.Add(new OverThresholdPath { relativePath = relStr, charCount = relLen, excessChars = relLen - threshold });
                                }

                                scanDirectory(fullPath);
                            }
                        }
                        else
                        {
                            report.totalFiles++;
                            ulong fileSize = ((ulong)findData.nFileSizeHigh << 32) | findData.nFileSizeLow;
                            report.totalSizeBytes += fileSize;

                            string fPrintPath = fullPath;
                            if (fPrintPath.StartsWith(@"\\?\UNC\")) fPrintPath = @"\\" + fPrintPath.Substring(8);
                            else if (fPrintPath.StartsWith(@"\\?\")) fPrintPath = fPrintPath.Substring(4);

                            int fLen = fPrintPath.Length - rootLength - 1;
                            if (fLen > threshold)
                            {
                                string relStr = (rootLength + 1 < fPrintPath.Length) ? fPrintPath.Substring(rootLength + 1) : fPrintPath;
                                report.badPaths.Add(new OverThresholdPath { relativePath = relStr, charCount = fLen, excessChars = fLen - threshold });
                            }
                        }

                    } while (FindNextFileW(hFind, out findData));

                    FindClose(hFind);
                }
            });

            scanDirectory(longAbsRoot);
            if (report.totalFolders > 0) report.totalFolders--; // Rimuoviamo il root level folder per coerenza con il report

            GenerateReports(report, rootInput);
        }

        private void GenerateReports(ScanReport report, string rootInput)
        {
            using (var sw = new StreamWriter("Report_PathManager.txt", false, Encoding.UTF8))
            {
                sw.WriteLine("[Sezione 1: Statistiche Globali]");
                sw.WriteLine("Root analizzata: " + rootInput);
                sw.WriteLine("Totale Cartelle: " + report.totalFolders);
                sw.WriteLine("Totale File: " + report.totalFiles);
                sw.WriteLine("Peso Totale: " + FormatSize(report.totalSizeBytes));
                sw.WriteLine("Soglia impostata: " + report.thresholdLimit + " caratteri");
                sw.WriteLine("File/Cartelle oltre la soglia: " + report.badPaths.Count + "\n");

                if (report.badPaths.Count > 0)
                {
                    sw.WriteLine("[Sezione 2: Dettaglio Soglia Violata]");
                    foreach (var bp in report.badPaths)
                    {
                        sw.WriteLine(string.Format("[+{0} caratteri] (Tot: {1}) - {2}", bp.excessChars, bp.charCount, bp.relativePath));
                    }
                }
            }

            using (var sw = new StreamWriter("Report_PathManager.csv", false, new UTF8Encoding(true)))
            {
                sw.WriteLine("Eccesso Caratteri,Caratteri Totali,Percorso Relativo");
                foreach (var bp in report.badPaths)
                {
                    sw.WriteLine(string.Format("{0},{1},\"{2}\"", bp.excessChars, bp.charCount, bp.relativePath));
                }
            }
        }

        private string FormatSize(ulong bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int s = 0;
            double count = bytes;
            while (count >= 1024 && s < 5)
            {
                s++;
                count /= 1024;
            }
            return string.Format("{0:0.00} {1}", count, suffixes[s]);
        }

        private void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
