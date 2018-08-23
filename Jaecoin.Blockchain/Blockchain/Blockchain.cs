using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Jaecoin.Blockchain
{
    public class Blockchain : IBlockchain
    {
        private List<ITransaction> _currentTransactions = new List<ITransaction>();
        private List<Block> _chain = new List<Block>();
        private List<Node> _nodes = new List<Node>();
        private Block _lastBlock => _chain.Last();

        public string NodeId { get; private set; }
        public int LastIndex
        {
            get { return this._lastBlock.Index; }
        }
        public int LastProof
        {
            get { return this._lastBlock.Proof;  }
        }
        public string LastHash
        {
            get { return this._lastBlock.PreviousHash; }
        }
        public string Nodes
        {
            get
            {
                return JsonConvert.SerializeObject(this._nodes);
            }
        }

        public Blockchain()
        {
            NodeId = Guid.NewGuid().ToString().Replace("-", "");
            CreateNewBlock(proof: 100, previousHash: "1"); //genesis block
        }

        #region Private methods
        private void AddNode(string name, string address)
        {
            _nodes.Add(new Node { Address = new Uri(address) });
        }

        private bool IsValidChain(List<Block> chain)
        {
            Block block = null;
            Block lastBlock = chain.First();
            int currentIndex = 1;
            while (currentIndex < chain.Count)
            {
                block = chain.ElementAt(currentIndex);

                //Check that the hash of the block is correct
                if (block.PreviousHash != GetHash(lastBlock))
                    return false;

                //Check that the Proof of Work is correct
                if (!IsValidProof(lastBlock.Proof, block.Proof, lastBlock.PreviousHash))
                    return false;

                lastBlock = block;
                currentIndex++;
            }

            return true;
        }

        private bool ResolveConflicts()
        {
            List<Block> newChain = null;
            int maxLength = _chain.Count;

            foreach (Node node in _nodes)
            {
                var url = new Uri(node.Address, "/chain");
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var model = new
                    {
                        chain = new List<Block>(),
                        length = 0
                    };
                    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    var data = JsonConvert.DeserializeAnonymousType(json, model);

                    if (data.chain.Count > _chain.Count && IsValidChain(data.chain))
                    {
                        maxLength = data.chain.Count;
                        newChain = data.chain;
                    }
                }
            }

            if (newChain != null)
            {
                _chain = newChain;
                return true;
            }

            return false;
        }

        private Block CreateNewBlock(int proof, string previousHash = null)
        {
            var block = new Block
            {
                Index = _chain.Count,
                Timestamp = DateTime.UtcNow,
                Transactions = _currentTransactions.ToList(),
                Proof = proof,
                PreviousHash = previousHash ?? GetHash(_chain.Last())
            };

            _currentTransactions.Clear();
            _chain.Add(block);
            return block;
        }

        private bool IsValidProof(int lastProof, int proof, string previousHash)
        {
            string guess = $"{lastProof}{proof}{previousHash}";
            string result = GetSha256(guess);
            return result.StartsWith("00000");
        }

        private string GetHash(Block block)
        {
            string blockText = JsonConvert.SerializeObject(block);
            return GetSha256(blockText);
        }

        private string GetSha256(string data)
        {
            var sha256 = new SHA256Managed();
            var hashBuilder = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = sha256.ComputeHash(bytes);

            foreach (byte x in hash)
                hashBuilder.Append($"{x:x2}");

            return hashBuilder.ToString();
        }
        #endregion

        #region Public methods
        public string Mine(int proofOfWork)
        {
            if (this.IsValidProof(_lastBlock.Proof, proofOfWork, _lastBlock.PreviousHash))
            {
                //CreateTransaction(sender: "0", recipient: NodeId, amount: 1);
                Block block = CreateNewBlock(proofOfWork);

                var response = new
                {
                    Accepted = true,
                    Message = "New Block Forged",
                    Index = block.Index,
                    Transactions = block.Transactions.ToArray(),
                    Proof = block.Proof,
                    PreviousHash = block.PreviousHash
                };

                return JsonConvert.SerializeObject(response);
            }
            else
            {
                var response = new
                {
                    Accepted = false,
                    Message = "Invalid Proof of Work",
                    Proof = proofOfWork,
                };

                return JsonConvert.SerializeObject(response);
            }
        }

        public string GetFullChain()
        {
            var response = new
            {
                chain = _chain.ToArray(),
                length = _chain.Count
            };

            return JsonConvert.SerializeObject(response);
        }

        public string RegisterNode(string name, string address)
        {
            AddNode(name, address);

            return $"Node:{name}with address{address}was added at{DateTime.UtcNow.ToShortTimeString()}.";
        }

        public string Consensus()
        {
            bool replaced = ResolveConflicts();
            string message = replaced ? "was replaced" : "is authoritive";

            var response = new
            {
                Message = $"Our chain {message}",
                Chain = _chain
            };

            return JsonConvert.SerializeObject(response);
        }

        public int CreateTransaction(Transaction transaction)
        {
            _currentTransactions.Add(transaction);

            return _lastBlock != null ? _lastBlock.Index + 1 : 0;
        }
        #endregion
    }
}
