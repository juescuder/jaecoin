﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Jaecoin.Blockchain
{
    public class Block : IBlock
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public List<ITransaction> Transactions { get; set; }
        public int Proof { get; set; }
        public string PreviousHash { get; set; }

        public override string ToString()
        {
            return $"{Index} [{Timestamp.ToString("yyyy-MM-dd HH:mm:ss")}] Proof: {Proof} | PrevHash: {PreviousHash} | Trx: {Transactions.Count}";
        }
    }
}
