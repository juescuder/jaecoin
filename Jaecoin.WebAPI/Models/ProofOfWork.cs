using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jaecoin.WebAPI.Models
{
    public class ProofOfWork
    {
        public int lastProof;
        public string previousHash;
        public int proof;
    }
}
