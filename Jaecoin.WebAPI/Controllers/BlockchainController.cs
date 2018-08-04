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

        [Route("api/mine")]
        [HttpPost]
        public string Mine([FromBody]ProofOfWork proofOfWork)
        {
            return this._blockchain.Mine(proofOfWork.proof);
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

        [Route("api/transaction/new")]
        public string NewTransaction()
        {
            string json = new StreamReader(this.Request.Body).ReadToEnd();
            Transaction trx = JsonConvert.DeserializeObject<Transaction>(json);
            int blockId = _blockchain.CreateTransaction(trx.Sender, trx.Recipient, trx.Amount);
            return $"Your transaction will be included in block {blockId}";
        }

        [Route("api/chain")]
        public string Chain()
        {
            return this._blockchain.GetFullChain();
        }

        [Route("api/nodes/register")]
        public string RegisterNode()
        {
            string json = new StreamReader(this.Request.Body).ReadToEnd();
            var urlList = new { Urls = new string[0] };
            var obj = JsonConvert.DeserializeAnonymousType(json, urlList);
            return _blockchain.RegisterNodes(obj.Urls);

        }

        [Route("api/nodes/resolve")]
        public string ResolveNode()
        {
            return _blockchain.Consensus();
        }

    }
}