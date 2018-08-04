using Jaecoin.Miner.Model;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Jaecoin.Miner
{
    class Program
    {
        static void Main(string[] args)
        {
            string address = "http://localhost:29005";
            string pathLast = "/api/mine/last";
            string pathMine = "/api/mine";
            int proof = 0;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("-- JAECOIN MINER --");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + " - Starting to mine...");

            //Always running
            while (true)
            {
                //I get the last PoW available
                dynamic last = GetLastPoW(address + pathLast);
                proof = last.lastProof + 1;

                Console.WriteLine(DateTime.Now.ToShortTimeString() + " - Getting last PoW...");
                Stopwatch elapsed = new Stopwatch();
                elapsed.Start();

                //Until I get a valid proof
                while (!IsValidProof(last.lastProof, proof, last.previousHash))
                    proof++;

                elapsed.Stop();

                //I send the proof to the node (address).
                SendPoW(address + pathMine, last.lastProof, last.previousHash, proof);

                Console.WriteLine(DateTime.Now.ToShortTimeString() + " - PoW found and sent! ({0}ms)", elapsed.ElapsedMilliseconds);

                //I restart the proof?? and I sleep for 5 seconds.
                //proof = 0;
                System.Threading.Thread.Sleep(5000);
            }
        }

        private static dynamic GetLastPoW(string fullAddress)
        {
            HttpWebRequest request = WebRequest.CreateHttp(fullAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string json = "";

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                json = reader.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<LastMined>(json);
        }

        private static void SendPoW(string fullAddress, int lstProof, string prevHash, int newProof)
        {
            HttpWebRequest request = WebRequest.CreateHttp(fullAddress);
            request.Method = "POST";
            request.ContentType = "application/json";
            using (Stream stream = request.GetRequestStream())
            {
                dynamic json = new
                {
                    lastProof = lstProof,
                    previousHash = prevHash,
                    proof = newProof
                };

                byte[] requestContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(json));

                stream.Write(requestContent, 0, requestContent.Length);
                stream.Flush();
                stream.Close();
            }

            WebResponse response = request.GetResponse();
        }

        private static bool IsValidProof(int lastProof, int proof, string previousHash)
        {
            string guess = $"{lastProof}{proof}{previousHash}";
            string result = GetSha256(guess);
            return result.StartsWith("00000");
        }

        private static string GetSha256(string data)
        {
            var sha256 = new SHA256Managed();
            var hashBuilder = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = sha256.ComputeHash(bytes);

            foreach (byte x in hash)
                hashBuilder.Append($"{x:x2}");

            return hashBuilder.ToString();
        }
    }
}
