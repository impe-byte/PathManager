using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using PathManager.Core;
using PathManager.Exporters;
using PathManager.Localization;

namespace PathManager.UI
{
    public class MainForm : Form
    {
        private TextBox txtPath;
        private Button btnBrowse;
        private NumericUpDown numThreshold;
        private Button btnScan;
        private Button btnOpenTxt;
        private Button btnOpenCsv;
        private Button btnOpenHtml;
        private Label lblStatus;
        private Label lblTitle;
        private Label lbl1;
        private Label lbl2;
        private ProgressBar progressBar;
        private Button btnLang;

        public MainForm()
        {
            this.Size = new Size(580, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.BackColor = Color.FromArgb(245, 245, 245);

            btnLang = new Button() { Location = new Point(480, 20), Width = 60, Height = 28, BackColor = Color.White, Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat };
            btnLang.FlatAppearance.BorderColor = Color.LightGray;
            btnLang.FlatAppearance.BorderSize = 1;
            btnLang.Click += BtnLang_Click;
            this.Controls.Add(btnLang);

            lblTitle = new Label() { Location = new Point(25, 20), AutoSize = true, Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 215) };
            this.Controls.Add(lblTitle);

            lbl1 = new Label() { Location = new Point(25, 80), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            this.Controls.Add(lbl1);

            txtPath = new TextBox() { Location = new Point(25, 105), Width = 430, Font = new Font("Segoe UI", 11F) };
            this.Controls.Add(txtPath);

            btnBrowse = new Button() { Location = new Point(460, 104), Width = 80, Height = 30, BackColor = Color.White, Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat };
            btnBrowse.FlatAppearance.BorderColor = Color.LightGray;
            btnBrowse.Click += BtnBrowse_Click;
            this.Controls.Add(btnBrowse);

            lbl2 = new Label() { Location = new Point(25, 150), AutoSize = true, Font = new Font("Segoe UI", 9.5F) };
            this.Controls.Add(lbl2);

            numThreshold = new NumericUpDown() { Location = new Point(25, 175), Width = 120, Minimum = 1, Maximum = 32767, Value = 150, Font = new Font("Segoe UI", 11F) };
            this.Controls.Add(numThreshold);

            btnScan = new Button() { Location = new Point(25, 230), Width = 515, Height = 45, Font = new Font("Segoe UI Semibold", 11F), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Click += BtnScan_Click;
            this.Controls.Add(btnScan);

            progressBar = new ProgressBar() { Location = new Point(25, 290), Width = 515, Height = 6, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 0, Visible = false };
            this.Controls.Add(progressBar);

            lblStatus = new Label() { Location = new Point(25, 305), AutoSize = false, Width = 515, Height = 20, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9F) };
            this.Controls.Add(lblStatus);

            btnOpenTxt = new Button() { Location = new Point(25, 335), Width = 160, Height = 35, Enabled = false, BackColor = Color.White, Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat };
            btnOpenTxt.FlatAppearance.BorderColor = Color.LightGray;
            btnOpenTxt.Click += (s, e) => OpenFile("Report_PathManager.txt");
            this.Controls.Add(btnOpenTxt);

            btnOpenCsv = new Button() { Location = new Point(200, 335), Width = 160, Height = 35, Enabled = false, BackColor = Color.White, Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat };
            btnOpenCsv.FlatAppearance.BorderColor = Color.LightGray;
            btnOpenCsv.Click += (s, e) => OpenFile("Report_PathManager.csv");
            this.Controls.Add(btnOpenCsv);

            btnOpenHtml = new Button() { Location = new Point(375, 335), Width = 165, Height = 35, Enabled = false, BackColor = Color.FromArgb(235, 245, 255), ForeColor = Color.FromArgb(0, 80, 160), Cursor = Cursors.Hand, FlatStyle = FlatStyle.Flat };
            btnOpenHtml.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
            btnOpenHtml.Click += (s, e) => OpenFile("Report_PathManager.html");
            this.Controls.Add(btnOpenHtml);

            UpdateUIStrings();
        }

        private void BtnLang_Click(object sender, EventArgs e)
        {
            Strings.CurrentLang = Strings.CurrentLang == "IT" ? "EN" : "IT";
            UpdateUIStrings();
        }

        private void UpdateUIStrings()
        {
            this.Text = Strings.Get("Title");
            lblTitle.Text = this.Text;
            lbl1.Text = Strings.Get("RootLabel");
            btnBrowse.Text = Strings.Get("Browse");
            lbl2.Text = Strings.Get("ThresholdLabel");
            btnScan.Text = Strings.Get("ScanBtn");
            btnOpenTxt.Text = Strings.Get("OpenTxt");
            btnOpenCsv.Text = Strings.Get("OpenCsv");
            btnOpenHtml.Text = Strings.Get("OpenHtml");
            btnLang.Text = Strings.Get("LangToggle");
            
            if (!progressBar.Visible) 
                lblStatus.Text = Strings.Get("StatusReady");
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
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
                MessageBox.Show(Strings.Get("SelectValidRoot"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnScan.Enabled = false;
            btnOpenTxt.Enabled = false;
            btnOpenCsv.Enabled = false;
            btnOpenHtml.Enabled = false;
            lblStatus.Text = Strings.Get("StatusScanning");
            lblStatus.ForeColor = Color.FromArgb(0, 120, 215);
            progressBar.Visible = true;
            progressBar.MarqueeAnimationSpeed = 30;

            int threshold = (int)numThreshold.Value;
            var progress = new Progress<string>(currPath => 
            {
                string trunc = currPath.Length > 60 ? "..." + currPath.Substring(currPath.Length - 57) : currPath;
                lblStatus.Text = Strings.Get("StatusScanning") + " " + trunc;
            });

            try
            {
                PathScanner scanner = new PathScanner();
                ScanReport report = await Task.Run(() => scanner.RunScan(rootInput, threshold, progress));

                new TxtExporter().Export(report, "Report_PathManager.txt");
                new CsvExporter().Export(report, "Report_PathManager.csv");
                new HtmlExporter().Export(report, "Report_PathManager.html");

                lblStatus.Text = Strings.Get("StatusDone");
                lblStatus.ForeColor = Color.Green;

                btnOpenTxt.Enabled = File.Exists("Report_PathManager.txt");
                btnOpenCsv.Enabled = File.Exists("Report_PathManager.csv");
                btnOpenHtml.Enabled = File.Exists("Report_PathManager.html");
            }
            catch (Exception ex)
            {
                lblStatus.Text = Strings.Get("StatusError");
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show(Strings.Get("StatusError") + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnScan.Enabled = true;
                progressBar.Visible = false;
                progressBar.MarqueeAnimationSpeed = 0;
            }
        }

        private void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
        }
    }
}
