using PathManagerProfessional.Core.Domain;

namespace PathManagerProfessional.Core.Ports
{
    public interface IFileSystemAdapter
    {
        bool TryApplyTransaction(PathTransaction transaction, out string errorMessage);
    }
}
