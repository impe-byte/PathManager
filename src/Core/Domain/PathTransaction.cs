namespace PathManagerProfessional.Core.Domain
{
    public enum TransactionType
    {
        Rename,
        Move,
        Truncate
    }

    public enum TransactionStatus
    {
        Pending,
        Success,
        Failed
    }

    public class PathTransaction
    {
        public string OriginalPath { get; private set; }
        public string ProposedPath { get; private set; }
        public TransactionType Type { get; private set; }
        public int ExcessCharacters { get; private set; }
        public string Reason { get; private set; }

        public TransactionStatus Status { get; set; }
        public string ExecutionMessage { get; set; }

        public PathTransaction(string originalPath, string proposedPath, TransactionType type, int excessCharacters, string reason)
        {
            OriginalPath = originalPath;
            ProposedPath = proposedPath;
            Type = type;
            ExcessCharacters = excessCharacters;
            Reason = reason;
            Status = TransactionStatus.Pending;
            ExecutionMessage = string.Empty;
        }
    }
}
