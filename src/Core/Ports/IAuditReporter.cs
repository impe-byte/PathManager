using PathManagerProfessional.Core.Application;

namespace PathManagerProfessional.Core.Ports
{
    public interface IAuditReporter
    {
        void GenerateReport(TransactionPlan plan, string outputDirectory);
    }
}
