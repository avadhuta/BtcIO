using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BtcWalletTools;
using NBitcoin;
using NBitcoin.DataEncoders;
using NBitcoin.OpenAsset;
using Newtonsoft.Json;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System.Net.NetworkInformation;


namespace BtcIO
{
    public static class WalletTools
    {

        static txref[] UnspenttxBc(string addr, string net = "test3")
        {
            var url = $"https://api.blockcypher.com/v1/btc/{net}/addrs/{addr}?unspentOnly=true&includeScript=true";
            var req = Tech.webreq(url);

            if (req == null) return null;

            try
            {
                var res = JsonConvert.DeserializeObject<txxx>(req).txrefs.ToArray();
                return res;
            }
            catch (Exception e)
            {
                throw new Exception("UnspenttxBc JsonConvert.DeserializeObject<txxx> error, maybe there are unconfirmed transactions, wait until they are confirmed");
            }

        }

        public static (bool suc, string hash) PushTx2Bc(string netStr, Transaction tx, bool useTor = false)
        {
            try
            {
                string req = "";


                if (useTor) 
                    req = Tech.webreqTor( $"https://api.blockcypher.com/v1/btc/{netStr}/txs/push", "POST", JsonConvert.SerializeObject(new { tx = tx.ToHex() }));
                else 
                    req = Tech.webreq($"https://api.blockcypher.com/v1/btc/{netStr}/txs/push", "POST", JsonConvert.SerializeObject(new { tx = tx.ToHex() }));

                var parse = JsonConvert.DeserializeObject<BCtcPushResult>(req);

                return (true,parse.tx.hash);
            }
            catch (Exception e)
            {
                return (false, "");
            }
        }

        static (bool suc, string hash) PushTx2Ninja(string netStr, Transaction tx, QBitNinjaClient client)
        {
            var broadcastResponse = client.Broadcast(tx).Result;
            if (broadcastResponse != null && broadcastResponse.Success) return (true, tx.GetHash().ToString());
            else return (false, null);
        }

        public static (Transaction tx, List<KeyValuePair<string, decimal>> toList, string note) Sendbtc2One(string wif, string addrFrom, string addrTo, decimal value, decimal fee = 0.0002m)
        {
            return Sendbtc2Many(wif, addrFrom, new[] {addrTo}, new[] {value}, fee);
        }
        
        
        //public static (Transaction tx, List<KeyValuePair<string, decimal>> toList,  string note) Sendbtc2Many(string wif, string addrFrom, string[] addrTo, decimal[] value, decimal fee = 0.0002m)
        //{
        //    try
        //    {
        //        var b = GetBallance(addrFrom);
        //        if (b < value.Sum() + fee) return (null, null, "not funds");

        //        var bitcoinPrivateKey = new BitcoinSecret(wif);
        //        var network = bitcoinPrivateKey.Network;
        //        var address = BitcoinAddress.Create(addrFrom, network);
        //        var scriptpubkey = address.ScriptPubKey;

        //        var netStr = network == Network.Main ? "main" : "test3";
        //        var uttx = UnspenttxBc(address.ToString(), netStr);
        //        if (uttx == null) return (null, null, "utxx proplem");

        //        var tx = new HashSet<string>(uttx).ToArray();

        //        var client = new QBitNinjaClient(network);
        //        var transaction = Transaction.Create(network);

        //        var coins = new List<ICoin>();
        //        long satoshi = 0;

        //        decimal valueSum = value.Sum(d => d > 0 ? d : 0);
        //        valueSum = value.Count(d => d < 0) == 0 ? valueSum : b;

        //        for (int i = 0; i < tx.Length; i++)
        //        {
        //            var transactionId = uint256.Parse(tx[i]);
        //            var transactionResponse = client.GetTransaction(transactionId).Result;
        //            if (transactionResponse == null) continue;
        //            var receivedCoins = transactionResponse.ReceivedCoins;

        //            foreach (var coin in receivedCoins)
        //            {
        //                if (coin.TxOut.ScriptPubKey == scriptpubkey)
        //                {
        //                    transaction.Inputs.Add(new TxIn() { PrevOut = coin.Outpoint });
        //                    satoshi += ((Coin)coin).Amount.Satoshi;
        //                    coins.Add(coin);
        //                }
        //            }

        //            if (((decimal)satoshi / 100000000) - fee > valueSum) break;
        //        }


        //        decimal txInAmount = ((decimal)satoshi / 100000000) - fee;

        //        decimal all = 0;

        //        var toList = new List<KeyValuePair<string, decimal>>();

        //        for (int i = 0; i < addrTo.Length; i++)
        //        {
        //            var reciveAddr = BitcoinAddress.Create(addrTo[i], network);
        //            var v = value[i] > 0 ? value[i] : txInAmount - all;

        //            if (all + v <= txInAmount)
        //            {
        //                all += v;

        //                TxOut baseOut = new TxOut()
        //                {
        //                    Value = new Money(v, MoneyUnit.BTC),
        //                    ScriptPubKey = reciveAddr.ScriptPubKey
        //                };

        //                transaction.Outputs.Add(baseOut);

        //                toList.Add(new KeyValuePair<string, decimal>(addrTo[i],v));
        //            }


        //        }

        //        var changeBackAmount = txInAmount - all;

        //        if (changeBackAmount > 0)
        //        {
        //            TxOut changeBackTxOut = new TxOut()
        //            {
        //                Value = new Money(changeBackAmount, MoneyUnit.BTC),
        //                ScriptPubKey = scriptpubkey
        //            };
        //            transaction.Outputs.Add(changeBackTxOut);

        //            toList.Add(new KeyValuePair<string, decimal>("changeBack "+ addrFrom, changeBackAmount));
        //        }


        //        if(!address.ScriptPubKey.IsScriptType(ScriptType.Witness))
        //            for (int i = 0; i < transaction.Inputs.Count; i++) transaction.Inputs[i].ScriptSig = scriptpubkey;


        //        transaction.Sign(bitcoinPrivateKey, coins.ToArray());

        //        return (transaction, toList, netStr);
        //    }
        //    catch (Exception e)
        //    {
        //        return (null, null, e.ToString());
        //    }

        //}




        public static (Transaction tx, List<KeyValuePair<string, decimal>> toList, string note) Sendbtc2Many(string wif, string addrFrom, string[] addrTo, decimal[] value, decimal fee = 0.0002m)
        {
            try
            {
                var b = GetBallance(addrFrom);
                if (b < value.Sum() + fee) return (null, null, "not funds");

                var bitcoinPrivateKey = new BitcoinSecret(wif);
                var network = bitcoinPrivateKey.Network;
                var address = BitcoinAddress.Create(addrFrom, network);
                var scriptpubkey = address.ScriptPubKey;

                var netStr = network == Network.Main ? "main" : "test3";
                var tx2 = UnspenttxBc(address.ToString(), netStr);
                if (tx2 == null) return (null, null, "utxx proplem");

                var transaction = Transaction.Create(network);

                var coins = new List<ICoin>();
                long satoshi = 0;

                decimal valueSum = value.Sum(d => d > 0 ? d : 0);
                valueSum = value.Count(d => d < 0) == 0 ? valueSum : b;

                for (int i = 0; i < tx2.Length; i++)
                {
                    var coin = new Coin(uint256.Parse(tx2[i].tx_hash), (uint)tx2[i].tx_output_n, tx2[i].value, scriptpubkey);

                    transaction.Inputs.Add(new TxIn() { PrevOut = coin.Outpoint });
                    satoshi += ((Coin)coin).Amount.Satoshi;
                    coins.Add(coin);

                    if (((decimal)satoshi / 100000000) - fee > valueSum) break;
                }


                decimal txInAmount = ((decimal)satoshi / 100000000) - fee;

                decimal all = 0;

                var toList = new List<KeyValuePair<string, decimal>>();

                for (int i = 0; i < addrTo.Length; i++)
                {
                    var reciveAddr = BitcoinAddress.Create(addrTo[i], network);
                    var v = value[i] > 0 ? value[i] : txInAmount - all;

                    if (all + v <= txInAmount)
                    {
                        all += v;

                        TxOut baseOut = new TxOut()
                        {
                            Value = new Money(v, MoneyUnit.BTC),
                            ScriptPubKey = reciveAddr.ScriptPubKey
                        };

                        transaction.Outputs.Add(baseOut);

                        toList.Add(new KeyValuePair<string, decimal>(addrTo[i], v));
                    }


                }

                var changeBackAmount = txInAmount - all;

                if (changeBackAmount > 0)
                {
                    TxOut changeBackTxOut = new TxOut()
                    {
                        Value = new Money(changeBackAmount, MoneyUnit.BTC),
                        ScriptPubKey = scriptpubkey
                    };
                    transaction.Outputs.Add(changeBackTxOut);

                    toList.Add(new KeyValuePair<string, decimal>("changeBack " + addrFrom, changeBackAmount));
                }


                if (!address.ScriptPubKey.IsScriptType(ScriptType.Witness))
                    for (int i = 0; i < transaction.Inputs.Count; i++) transaction.Inputs[i].ScriptSig = scriptpubkey;


                transaction.Sign(bitcoinPrivateKey, coins.ToArray());

                return (transaction, toList, netStr);
            }
            catch (Exception e)
            {
                return (null, null, e.Message);
            }

        }


        //public static string IssuanceCoin()
        //{
        //    var net = Network.TestNet;

        //    var coin = new Coin(
        //        fromTxHash: new uint256("35d08a7ffa218b78dec3038eb51fe6108dedca5bf011a7e152dd9e124924470c"),
        //        fromOutputIndex: 0,
        //        amount: 9990000, 
        //        scriptPubKey: new Script(Encoders.Hex.DecodeData("a914b6fe120e0a50a9cb3ad5a7ccf8be8cac9b9bbad887")));

        //    var issuance = new IssuanceCoin(coin);


        //    var nico = BitcoinAddress.Create("2MvF83UXQRq783WFBd6kSjFBJznji6urGMS", net);
        //    var bookKey = new BitcoinSecret("cQR1X2ooR8MhjTxPYEzid6h3dUjXgbh38DR916JtfTn3ZaxCJoqJ");
        //    var builder = net.CreateTransactionBuilder();

        //    var tx = builder
        //        .AddKeys(bookKey)
        //        .AddCoins(issuance)
        //        .IssueAsset(nico, new AssetMoney(issuance.AssetId, 21000000m, 8))
        //        .SendFees(Money.Coins(0.0005m))
        //        .SetChange(bookKey.GetAddress(ScriptPubKeyType.Legacy))
        //        .BuildTransaction(true);

        //    Console.WriteLine(tx);
        //    Console.WriteLine(nico.ToColoredAddress());
        //    Console.WriteLine(builder.Verify(tx));


        //    var assetId = new AssetId(bookKey).GetWif(net);
        //    Console.WriteLine(assetId); // AVAVfLSb1KZf9tJzrUVpktjxKUXGxUTD4e

        //    var ttt = PushTx2Bc("test3", tx);


        //    return ttt.hash;
        //}

        public static decimal GetBallance(string addr, string net = "test3")
        {
            var url = $"https://api.blockcypher.com/v1/btc/{net}/addrs/{addr}/balance";
            var b = Tech.webreq(url);
            if (b == null) return -1;
            try
            {
                var req = JsonConvert.DeserializeObject<req2>(b);
                return req.final_balance / 100000000;
            }
            catch (Exception e)
            {
                return -2;
            }
        }




        public static (string address, string wif) NewWifAddr(string seed, string net = "test3", int addrtype = 2)
        {
            var b = Tech.Sha256(seed);
            var network = net == "test3" ? Network.TestNet : Network.Main;
            var privateKey = new Key(b);

            return (privateKey.PubKey.GetAddress((ScriptPubKeyType) addrtype, network).ToString(), privateKey.GetWif(network).ToString());

        }


    }
}
