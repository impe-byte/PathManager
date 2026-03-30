using System.Linq;
using System.Threading.Tasks;
using PathManagerProfessional.Core.Domain;
using PathManagerProfessional.Core.Engine;
using PathManagerProfessional.Core.Ports;
using PathManagerProfessional.Core.Application;

namespace PathManagerProfessional.Core.Tests
{
    public class TransactionOrchestratorTests
    {
        private class MockFileSystemAdapter : IFileSystemAdapter
        {
            public bool ReturnSuccess { get; set; }

            public MockFileSystemAdapter()
            {
                ReturnSuccess = true;
            }

            public bool TryApplyTransaction(PathTransaction transaction, out string errorMessage)
            {
                if (ReturnSuccess)
                {
                    errorMessage = null;
                    return true;
                }
                else
                {
                    errorMessage = "Mocked FileSystem Error";
                    return false;
                }
            }
        }

        [Fact]
        public async Task Should_Update_Transaction_Status_To_Success_When_Adapter_Succeeds()
        {
            // Arrange
            var engine = new PathResolutionEngine();
            var adapter = new MockFileSystemAdapter { ReturnSuccess = true };
            var orchestrator = new TransactionOrchestrator(engine, adapter);

            string longPath = @"C:\Shares\DepartmentData\" + new string('A', 230) + ".txt"; // > 250
            string[] badPaths = new string[] { longPath };
            
            var plan = orchestrator.CreatePlan(badPaths, 250);
            
            Assert.Equal(1, plan.TotalPending);
            Assert.Equal(0, plan.TotalSuccess);

            // Act
            await orchestrator.ExecutePlanAsync(plan);

            // Assert
            Assert.Equal(0, plan.TotalPending);
            Assert.Equal(1, plan.TotalSuccess);
            
            var transaction = plan.Transactions.First();
            Assert.Equal(TransactionStatus.Success, transaction.Status);
            Assert.Equal("Applied successfully.", transaction.ExecutionMessage);
        }
    }
}
