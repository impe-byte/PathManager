using System.Collections.Generic;
using System.Linq;
using PathManagerProfessional.Core.Domain;

namespace PathManagerProfessional.Core.Application
{
    public class TransactionPlan
    {
        public List<PathTransaction> Transactions { get; private set; }

        public int TotalPending 
        { 
            get { return Transactions.Count(t => t.Status == TransactionStatus.Pending); } 
        }

        public int TotalSuccess 
        { 
            get { return Transactions.Count(t => t.Status == TransactionStatus.Success); } 
        }

        public int TotalFailed 
        { 
            get { return Transactions.Count(t => t.Status == TransactionStatus.Failed); } 
        }

        public TransactionPlan(IEnumerable<PathTransaction> transactions)
        {
            Transactions = transactions.ToList();
        }
    }
}
