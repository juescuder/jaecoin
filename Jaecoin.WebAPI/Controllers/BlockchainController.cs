using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jaecoin.Blockchain;
using Jaecoin.WebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Jaecoin.WebAPI.Controllers
{
    public class BlockchainController : Controller
    {
        private Blockchain.Blockchain _blockchain;

        public BlockchainController(IBlockchain block)
        {
            this._blockchain = block as Blockchain.Blockchain;
        }

        #region Mine
        [Route("api/mine")]
        [HttpPost]
        public string Mine([FromBody]ProofOfWorkModel proofOfWork)
        {
            return this._blockchain.Mine(proofOfWork.proof);
        }

        [Route("api/mine/difficulty")]
        public string Difficulty()
        {
            var data = new
            {
                difficulty = "0000",
                hashrate = "000.0",
            };

            return JsonConvert.SerializeObject(data);
        }

        [Route("api/mine/last")]
        public string LastHash()
        {
            var last = new
            {
                lastProof = _blockchain.LastProof,
                previousHash = _blockchain.LastHash,
            };

            return JsonConvert.SerializeObject(last);
        }
        #endregion

        #region Transaction
        [Route("api/transaction/new")]
        [HttpPost]
        public string NewTransaction([FromBody] TransactionModel trx)
        {
            Transaction transaction = new Transaction();
            transaction.TransactionType = (TransactionType)trx.TransactionType;
            transaction.Sender = trx.Sender;
            transaction.Recipient = trx.Recipient;
            transaction.Amount = trx.Amount;
            transaction.Content = trx.Content;

            int blockId = _blockchain.CreateTransaction(transaction);
            return $"Your transaction will be included in block {blockId}";
        }
        #endregion

        #region Chain
        [Route("api/chain")]
        public string Chain()
        {
            return this._blockchain.GetFullChain();
        }
        #endregion

        #region Nodes
        [Route("api/nodes/register")]
        [HttpPost]
        public string RegisterNode([FromBody] NodeModel node)
        {
            return _blockchain.RegisterNode(node.Name, node.Address);
        }

        [Route("api/nodes/resolve")]
        public string ResolveNode()
        {
            return _blockchain.Consensus();
        }

        [Route("api/nodes")]
        public string Nodes()
        {
            return JsonConvert.SerializeObject(_blockchain.Nodes);
        }
        #endregion
    }
}