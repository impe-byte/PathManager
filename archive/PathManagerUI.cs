using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        public MainForm()
        {
            this.Text = "Path Manager UI";
            this.Size = new Size(500, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Imposta un font moderno e gradevole
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.BackColor = Color.FromArgb(245, 245, 245);

            Label lblTitle = new Label() { Text = "Path Manager", Location = new Point(20, 10), AutoSize = true, Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold) };
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

            btnScan = new Button() { Text = "Avvia Scansione C++", Location = new Point(20, 170), Width = 445, Height = 40, Font = new Font("Segoe UI Semibold", 10F), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White };
            btnScan.Click += BtnScan_Click;
            btnScan.FlatStyle = FlatStyle.Flat;
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Cursor = Cursors.Hand;
            this.Controls.Add(btnScan);

            progressBar = new ProgressBar() { Location = new Point(20, 220), Width = 445, Height = 6, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 0, Visible = false };
            this.Controls.Add(progressBar);

            lblStatus = new Label() { Text = "Pronto. Assicurati di aver compilato prima il progetto C++.", Location = new Point(20, 235), Width = 445, ForeColor = Color.Gray };
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
            string exePath = FindExecutable();
            if (string.IsNullOrEmpty(exePath))
            {
                MessageBox.Show("Eseguibile PathManager.exe (C++) non trovato!\nCompila il progetto C++ prima di avviare la scansione (es. tramite CMake o Visual Studio).", "Eseguibile C++ Mancante", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPath.Text) || !Directory.Exists(txtPath.Text))
            {
                MessageBox.Show("Seleziona una cartella radice valida e accessibile.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnScan.Enabled = false;
            btnOpenTxt.Enabled = false;
            btnOpenCsv.Enabled = false;
            lblStatus.Text = "Scansione in corso tramite motore C++... Attendere.";
            lblStatus.ForeColor = Color.FromArgb(0, 120, 215);
            progressBar.Visible = true;
            progressBar.MarqueeAnimationSpeed = 30;

            string rootPath = txtPath.Text;
            int threshold = (int)numThreshold.Value;

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = exePath;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardInput = true;
                    psi.RedirectStandardOutput = true;
                    psi.CreateNoWindow = true;
                    psi.WorkingDirectory = Application.StartupPath;

                    using (Process process = Process.Start(psi))
                    {
                        using (StreamWriter sw = process.StandardInput)
                        {
                            if (sw.BaseStream.CanWrite)
                            {
                                // Invia i parametri attesi dal programma C++ (simulate std::cin)
                                sw.WriteLine(rootPath);
                                sw.WriteLine(threshold);
                            }
                        }
                        process.WaitForExit();
                    }
                });

                lblStatus.Text = "Scansione C++ completata con successo!";
                lblStatus.ForeColor = Color.Green;
                
                if (File.Exists("Report_PathManager.txt")) btnOpenTxt.Enabled = true;
                if (File.Exists("Report_PathManager.csv")) btnOpenCsv.Enabled = true;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Errore durante la scansione.";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show("Errore di avvio eseguibile C++: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnScan.Enabled = true;
                progressBar.Visible = false;
                progressBar.MarqueeAnimationSpeed = 0;
            }
        }

        private string FindExecutable()
        {
            string[] possiblePaths = {
                "PathManager.exe",
                @"build\PathManager.exe",
                @"build\Debug\PathManager.exe",
                @"build\Release\PathManager.exe",
                @"out\build\x64-Debug\PathManager.exe",
                @"out\build\x64-Release\PathManager.exe"
            };

            foreach (var p in possiblePaths)
            {
                if (File.Exists(p)) return Path.GetFullPath(p);
            }
            return null;
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
