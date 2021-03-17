using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BtcWalletTools;
using NBitcoin;


namespace BtcIO
{


    class Program
    {

        private static decimal fee = 0.0001m;


        private static void HelpMenu()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("New WIF and address:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("n net['m|t'] type[0|1|2](optional)\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Get ballance of btc address:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("b 'your_btc_address'\n");
            Console.WriteLine("b 'your_btc_wallet+index[w0|w1...|wn]'\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Send btc to one address:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("s 'wif' 'addr_from' 'addr_to' value fee(optional)\n");

            var s = "sm 'wif' 'addr_from' fee(optional)\\n\n";
            s += "'addr_to1' value1\\n\n";
            s += "'addr_to2' value2\\n\n";
            s += ".................\\n\n";
            s += "'addr_toN' valueN\\n\n";
            s += "\\n\n";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Send btc to many addreses:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(s);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Open wallet from seed phrase:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("w net['m|t'] type[0|1|2](optional)\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Send btc from wallet to one address:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("sw 'w+index' 'addr_to' value fee(optional)\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Get help:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("h\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Clear screen:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("clr\n");

        }


        private static void NewAddress(string[] command)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (command.Length >= 2 && (command[1] == "m" || command[1] == "t"))
            {
                var net = command[1] == "m" ? "main" : "test3";
                int t = 2;
                if (command.Length == 3) Int32.TryParse(command[2], out t);

                byte[] customEntropy = null;
                Console.WriteLine("do yo want enter custom entropy? 'y/n'");
                if (Console.ReadKey().KeyChar == 'y') customEntropy = KeyboardEntropy();

                var seed = Tech.RandomSeed(customEntropy);
                Console.WriteLine($"seed phrase:\n{seed}");

                var aw = WalletTools.NewWifAddr(seed, net, t);
                Console.WriteLine($"{aw.wif} {aw.address}");
            }
            else Console.WriteLine("n net['m|t'] type[0|1|2](optional)");
        }



        static (string seed, string net, int t) w;
        private static void OpenWallet(string[] command)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (command.Length >= 1)
            {
                Console.WriteLine("enter seed phrase");
                Console.ForegroundColor = ConsoleColor.White;
                var seed = Console.ReadLine();

                string net = "main";
                if(command.Length >= 2) net = command[1] == "m" ? "main" : "test3";

                int t = 2;
                if (command.Length == 3) Int32.TryParse(command[2], out t);

                w = (seed, net, t);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("");
                Console.WriteLine($"open wallet");
                Console.WriteLine($"b w0");
                GetBallance(new []{"b","w0"});
            }

            else Console.WriteLine("w net['m|t'] type[0|1|2](optional)");
        }


        private static void Send2Many(string[] command)
        {
            if (command.Length >= 3)
            {
                if (command.Length >= 4) fee = Decimal.Parse(command[3], CultureInfo.InvariantCulture);
                var w = command[1];
                var a = command[2];

                var av = new List<KeyValuePair<string, decimal>>();
                int i = 0;
                while (true)
                {
                    try
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"addres{i} value{i}");
                        Console.ForegroundColor = ConsoleColor.White;
                        var avt = Console.ReadLine();
                        if (avt == "") break;
                        var ttt = avt.Split('\n');
                        for (int j = 0; j < ttt.Length; j++)
                        {
                            var avts = ttt[j].Split(' ');
                            if (avts.Length < 2) break;
                            av.Add(new KeyValuePair<string, decimal>(avts[0],
                                Decimal.Parse(avts[1], CultureInfo.InvariantCulture)));
                            i++;
                        }
                    }

                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                var addrs = av.Select(p => p.Key).ToArray();
                var values = av.Select(p => p.Value).ToArray();
                var t = WalletTools.Sendbtc2Many(w, a, addrs, values, fee);

                PrintTXDetails(t);

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("s 'wif' 'addr_from' 'addr_to' value fee(optional)");
            }
        }
        private static void Send2OneW(string[] command)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (command.Length >= 3)
            {
                if (command.Length == 5) fee = Decimal.Parse(command[4], CultureInfo.InvariantCulture);

                int num = Int32.Parse(command[1].Replace("w", ""));
                var aw = WalletTools.NewWifAddr(w.seed + num, w.net, w.t);

                var t = WalletTools.Sendbtc2One(aw.wif, aw.address, command[2], Decimal.Parse(command[3], CultureInfo.InvariantCulture), fee);

                PrintTXDetails(t);

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("sw 'wallet' 'addr_to' value fee(optional)\n");
            }
        }
        private static void Send2One(string[] command)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (command.Length >= 5)
            {
                if (command.Length == 6) fee = Decimal.Parse(command[4], CultureInfo.InvariantCulture);

                var t = WalletTools.Sendbtc2One(command[1], command[2], command[3], Decimal.Parse(command[4], CultureInfo.InvariantCulture), fee);

                PrintTXDetails(t);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("s 'wif' 'addr_from' 'addr_to' value fee(optional)");
            }
        }

        private static void PrintTXDetails((Transaction tx, List<KeyValuePair<string, decimal>> toList, string note) t)
        {
            //Console.WriteLine("JSON:");
            //Console.WriteLine(t);

            //Console.WriteLine();
            //Console.WriteLine("HEX:");
            //Console.WriteLine(t.tx.ToHex());
            //Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("you want send to:");

            if(t.toList.Count>0)
                foreach (var o in t.toList)  Console.WriteLine($"{o.Key} {o.Value}");


            Console.WriteLine("Aprove TX??? y\\n");
            Console.ForegroundColor = ConsoleColor.White;
            var s = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Red;
            if (s == "y")
            {
                var res = WalletTools.PushTx2Bc(t.note, t.tx);
                //  var res = PushTx2Ninja(netStr, transaction,client);

                if (res.hash != null)
                {
                    Console.WriteLine($"TX {res.hash} sent");
                    Task.Run(() =>
                    {
                        Thread.Sleep(1000);
                        var h = t.tx.GetHash();
                        Tech.OpenBrowser($"https://live.blockcypher.com/btc-testnet/tx/{h}");
                    });
                }
            }
            else Console.WriteLine("cancel");
        }

        private static void GetBallance(string[] command)
        {
            if (command.Length >= 2)
            {
                string net = "test3";
                var c = command[1];
                Console.ForegroundColor = ConsoleColor.Green;
                if (command[1][0] == 'w')
                {
                    var n = Int32.Parse(c.Replace("w", ""));
                    c = WalletTools.NewWifAddr(w.seed + n, w.net, w.t).address;
                    Console.WriteLine(c);
                }

                var c0 = c[0];
                if (c0 == '3' || c0 == '1' || c0 == 'b') net = "main";

                var b = WalletTools.GetBallance(c, net);
                Console.WriteLine(b);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("b 'your_btc_address'");
                Console.WriteLine("b 'w+index'");
            }
        }

        public static byte[] KeyboardEntropy()
        {
            Console.WriteLine("enter random symbols from keyboard");

            List<byte> symbols = new List<byte>();
            var pos = Console.GetCursorPosition();

            int N = 128;
            char curent = '\0';

            while (symbols.Count < N)
            {
                var c = Console.ReadKey().KeyChar;
                if (c != curent)
                {
                    curent = c;
                    symbols.Add((byte)curent);
                }


                Console.SetCursorPosition(pos.Left, pos.Top);
                Console.WriteLine($"{(int)(((double)symbols.Count / N)*100)}%");
            }

            while (true)
            {
                Console.SetCursorPosition(pos.Left, pos.Top);
                Console.WriteLine("entropy created, enter 'Y'");
                if (Console.ReadKey().KeyChar == 'Y') break;

            }

            var sha1 = Tech.Sha256(symbols.ToArray());
            var sha2 = Tech.Sha256(symbols.ToArray(), 2);
            var merge = sha1.ToList();
            merge.AddRange(sha2);


            return merge.ToArray();
        }


        static void ProcessLoop()
        {
            Console.WriteLine("BtcIO 0.1");
            while (true)
            {
                try
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    var command = Console.ReadLine().Split(' ');
                    var c0 = command[0];
                    switch (c0)
                    {
                        case "b": GetBallance(command); break;
                        case "s": Send2One(command); break;
                        case "sw": Send2OneW(command); break;
                        case "sm": Send2Many(command); break;
                        case "n": NewAddress(command); break;
                        case "w": OpenWallet(command); break;
                        case "h": HelpMenu(); break;
                        case "clr": Console.Clear(); break;
                       // default: HelpMenu(); break;
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }


        }



        static void Main(string[] args)
        {

             ProcessLoop();

            Console.ReadKey();
        }
    }
}
