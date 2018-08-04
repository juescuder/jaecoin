using System;
using System.Collections.Generic;
using System.Text;

namespace Jaecoin.Blockchain
{
    public class Transaction
    {
        public int Amount { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
    }
}
