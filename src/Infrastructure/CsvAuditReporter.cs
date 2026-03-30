using System;
using System.IO;
using System.Text;
using PathManagerProfessional.Core.Application;
using PathManagerProfessional.Core.Ports;

namespace PathManagerProfessional.Infrastructure
{
    public class CsvAuditReporter : IAuditReporter
    {
        public void GenerateReport(TransactionPlan plan, string outputDirectory)
        {
            if (plan == null || plan.Transactions == null || plan.Transactions.Count == 0) return;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = string.Format("PathManager_Audit_{0}.csv", timestamp);
            string fullPath = Path.Combine(outputDirectory, fileName);

            using (var writer = new StreamWriter(fullPath, false, Encoding.UTF8))
            {
                writer.WriteLine("OriginalPath,ProposedPath,TransactionType,Status,Message");

                foreach (var tx in plan.Transactions)
                {
                    string original = EscapeCsv(tx.OriginalPath);
                    string proposed = EscapeCsv(tx.ProposedPath);
                    string type = EscapeCsv(tx.Type.ToString());
                    string status = EscapeCsv(tx.Status.ToString());
                    string message = EscapeCsv(tx.ExecutionMessage);

                    writer.WriteLine(string.Format("{0},{1},{2},{3},{4}", original, proposed, type, status, message));
                }
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n"))
            {
                string escaped = value.Replace("\"", "\"\"");
                return string.Format("\"{0}\"", escaped);
            }

            return value;
        }
    }
}
