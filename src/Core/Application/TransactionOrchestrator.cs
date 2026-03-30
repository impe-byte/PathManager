using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PathManagerProfessional.Core.Domain;
using PathManagerProfessional.Core.Engine;
using PathManagerProfessional.Core.Ports;

namespace PathManagerProfessional.Core.Application
{
    public class TransactionOrchestrator
    {
        private readonly PathResolutionEngine _engine;
        private readonly IFileSystemAdapter _fileSystemAdapter;

        public TransactionOrchestrator(PathResolutionEngine engine, IFileSystemAdapter fileSystemAdapter)
        {
            _engine = engine;
            _fileSystemAdapter = fileSystemAdapter;
        }

        public TransactionPlan CreatePlan(IEnumerable<string> badPaths, int threshold)
        {
            var transactions = _engine.GenerateResolutionPlan(badPaths, threshold);
            return new TransactionPlan(transactions);
        }

        public async Task ExecutePlanAsync(TransactionPlan plan, IProgress<PathTransaction> progress = null)
        {
            await Task.Run(() =>
            {
                foreach (var transaction in plan.Transactions)
                {
                    if (transaction.Status == TransactionStatus.Pending)
                    {
                        string errorMessage;
                        bool success = _fileSystemAdapter.TryApplyTransaction(transaction, out errorMessage);

                        if (success)
                        {
                            transaction.Status = TransactionStatus.Success;
                            transaction.ExecutionMessage = "Applied successfully.";
                        }
                        else
                        {
                            transaction.Status = TransactionStatus.Failed;
                            transaction.ExecutionMessage = errorMessage;
                        }

                        if (progress != null)
                        {
                            progress.Report(transaction);
                        }
                    }
                }
            });
        }
    }
}
