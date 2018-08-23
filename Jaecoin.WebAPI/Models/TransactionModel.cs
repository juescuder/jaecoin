﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaecoin.WebAPI.Models
{
    public class TransactionModel
    {
        public int TransactionType { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }

        public int Amount { get; set; }
        public string Content { get; set; }
    }
}
