using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using PathManagerProfessional.Core.Application;
using PathManagerProfessional.Core.Domain;
using PathManagerProfessional.Core.Engine;
using PathManagerProfessional.Core.Ports;
using PathManagerProfessional.Infrastructure;

namespace PathManager.UI
{
    public class MainForm : Form
    {
        private TextBox txtRootPath;
        private Button btnBrowse;
        private NumericUpDown numThreshold;
        private Button btnPreview;
        private Button btnCommit;
        private DataGridView gridTransactions;
        private ProgressBar progressCommit;

        private TransactionOrchestrator _orchestrator;
        private TransactionPlan _currentPlan;

        public MainForm()
        {
            InitializeComponent();
            SetupDependencies();
        }

        private IAuditReporter _reporter;

        private void SetupDependencies()
        {
            var engine = new PathResolutionEngine();
            var adapter = new Win32FileSystemAdapter();
            _orchestrator = new TransactionOrchestrator(engine, adapter);
            _reporter = new CsvAuditReporter();
        }

        private void InitializeComponent()
        {
            this.Text = "PathManager Professional";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.BackColor = Color.FromArgb(245, 245, 245);

            txtRootPath = new TextBox { Location = new Point(20, 20), Width = 500 };
            
            btnBrowse = new Button { Text = "Browse...", Location = new Point(530, 18), Width = 80, FlatStyle = FlatStyle.Flat, BackColor = Color.White };
            btnBrowse.Click += BtnBrowse_Click;

            Label lblThreshold = new Label { Text = "Threshold:", Location = new Point(620, 22), AutoSize = true };
            numThreshold = new NumericUpDown { Location = new Point(690, 20), Width = 60, Minimum = 100, Maximum = 32000, Value = 250 };

            btnPreview = new Button { Text = "Preview Scan", Location = new Point(760, 18), Width = 100, FlatStyle = FlatStyle.Flat, BackColor = Color.LightSkyBlue };
            btnPreview.Click += BtnPreview_Click;

            btnCommit = new Button { Text = "Apply Fixes", Location = new Point(870, 18), Width = 100, FlatStyle = FlatStyle.Flat, BackColor = Color.LightGreen, Enabled = false };
            btnCommit.Click += BtnCommit_Click;

            progressCommit = new ProgressBar { Location = new Point(20, 50), Width = 950, Height = 5, Style = ProgressBarStyle.Continuous, Visible = false };

            gridTransactions = new DataGridView 
            { 
                Location = new Point(20, 65), 
                Size = new Size(950, 480),
                ReadOnly = true,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            gridTransactions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 230, 230);
            gridTransactions.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 250);

            gridTransactions.Columns.Add("OriginalPath", "Original Path");
            gridTransactions.Columns["OriginalPath"].Width = 350;
            gridTransactions.Columns.Add("ProposedPath", "Proposed Path");
            gridTransactions.Columns["ProposedPath"].Width = 250;
            gridTransactions.Columns.Add("Excess", "Excess");
            gridTransactions.Columns["Excess"].Width = 60;
            gridTransactions.Columns.Add("Status", "Status");
            gridTransactions.Columns["Status"].Width = 80;
            gridTransactions.Columns.Add("Message", "Message");
            gridTransactions.Columns["Message"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            this.Controls.Add(txtRootPath);
            this.Controls.Add(btnBrowse);
            this.Controls.Add(lblThreshold);
            this.Controls.Add(numThreshold);
            this.Controls.Add(btnPreview);
            this.Controls.Add(btnCommit);
            this.Controls.Add(progressCommit);
            this.Controls.Add(gridTransactions);
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtRootPath.Text = fbd.SelectedPath;
                }
            }
        }

        private async void BtnPreview_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRootPath.Text) || !Directory.Exists(txtRootPath.Text))
            {
                MessageBox.Show("Please select a valid directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ToggleUI(false);
            gridTransactions.Rows.Clear();
            _currentPlan = null;
            btnCommit.Enabled = false;

            try
            {
                var badPaths = await Task.Run(() =>
                {
                    try {
                        var allFiles = Directory.EnumerateFiles(txtRootPath.Text, "*.*", SearchOption.AllDirectories);
                        int threshold = (int)numThreshold.Value;
                        return allFiles.Where(f => f.Length > threshold).ToList();
                    } catch (UnauthorizedAccessException) {
                        return new System.Collections.Generic.List<string>();
                    }
                });

                _currentPlan = _orchestrator.CreatePlan(badPaths, (int)numThreshold.Value);

                foreach (var tx in _currentPlan.Transactions)
                {
                    int rowIndex = gridTransactions.Rows.Add(tx.OriginalPath, tx.ProposedPath, tx.ExcessCharacters, tx.Status.ToString(), tx.ExecutionMessage);
                    gridTransactions.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                    gridTransactions.Rows[rowIndex].Tag = tx;
                }

                if (_currentPlan.TotalPending > 0)
                {
                    btnCommit.Enabled = true;
                }
                else
                {
                    MessageBox.Show("No paths exceeding the threshold were found.", "Scan complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error during scan: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleUI(true);
            }
        }

        private async void BtnCommit_Click(object sender, EventArgs e)
        {
            if (_currentPlan == null || _currentPlan.TotalPending == 0) return;

            ToggleUI(false);
            btnCommit.Enabled = false;
            progressCommit.Visible = true;
            progressCommit.Style = ProgressBarStyle.Marquee;

            try
            {
                var progress = new Progress<PathTransaction>(tx =>
                {
                    foreach (DataGridViewRow row in gridTransactions.Rows)
                    {
                        if (row.Tag == tx)
                        {
                            row.Cells["Status"].Value = tx.Status.ToString();
                            row.Cells["Message"].Value = tx.ExecutionMessage;

                            if (tx.Status == TransactionStatus.Success)
                            {
                                row.DefaultCellStyle.BackColor = Color.LightGreen;
                            }
                            else if (tx.Status == TransactionStatus.Failed)
                            {
                                row.DefaultCellStyle.BackColor = Color.LightSalmon;
                            }
                            break;
                        }
                    }
                });

                await _orchestrator.ExecutePlanAsync(_currentPlan, progress);
                try
                {
                    _reporter.GenerateReport(_currentPlan, txtRootPath.Text);
                    MessageBox.Show("Execution completed. Audit Log saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception reportEx)
                {
                    MessageBox.Show(string.Format("Execution completed, but failed to save Audit Log: {0}", reportEx.Message), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error during execution: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressCommit.Visible = false;
                ToggleUI(true);
                btnCommit.Enabled = false;
            }
        }

        private void ToggleUI(bool enabled)
        {
            btnPreview.Enabled = enabled;
            btnBrowse.Enabled = enabled;
            txtRootPath.Enabled = enabled;
            numThreshold.Enabled = enabled;
        }
    }
}
