using System;
using System.Collections.Generic;
using System.Text;

namespace Jaecoin.Blockchain
{
    public class Transaction : ITransaction
    {
        public TransactionType TransactionType { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }

        public int Amount { get; set; }
        public string Content { get; set; }

        public Transaction()
        {
        }

        public string TransactionId()
        {
            throw new NotImplementedException();
        }
    }
}
