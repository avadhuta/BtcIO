using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using xNet;
using HttpStatusCode = System.Net.HttpStatusCode;
using Stream = System.IO.Stream;

namespace BtcWalletTools
{
    public static class Tech
    {

        public static string webreqTor(string url, string method = "GET", string content = "")
        {
            try
            {
                using var request = new HttpRequest { Proxy = Socks5ProxyClient.Parse("127.0.0.1:9150") };


                if (method == "POST" && content != "")
                {
                    var data = Encoding.ASCII.GetBytes(content);
                    return request.Post(url, data, "application/json").ToString();
                }


                return request.Get(url).ToString();
            }
            catch (Exception e)
            {
                return null;
            }

            return null;
        }


        public static string webreq(string url, string method = "GET", string content = "")
        {
            try
            {
                var request = WebRequest.Create(url);
                request.Method = method;


                if (method == "POST" && content != "")
                {
                    var data = Encoding.UTF8.GetBytes(content);
                    request.ContentType = "application/json";
                    request.ContentLength = data.Length;
                    request.GetRequestStream().Write(data, 0, data.Length);
                }

                var resp = request.GetResponse();
                var responseStream = resp.GetResponseStream();

                if (responseStream != null) return new StreamReader(responseStream).ReadToEnd();
                return null;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start((ProcessStartInfo) new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        public static string[] words = ReadResource("words.txt").Split('\n');

        public static string RandomSeed(byte[] customEntropy = null)
        {
            var rnd = Rnd3(customEntropy);
            string res = "";
            for (int i = 0; i < 12; i++) res += words[rnd[i]].ToLower() + (i < 11 ? " " : "");

            return res;
        }


        static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public static uint Rnd2()
        {
            var data = new byte[sizeof(uint)];
            rng.GetBytes(data);
            return (uint)(BitConverter.ToUInt32(data, 0) % (words.Length - 1));
        }

        public static uint[] Rnd3(byte[] customEntropy = null)
        {
            var data = new byte[sizeof(uint) * 12];
            rng.GetBytes(data);

            if (customEntropy != null)
                for (int i = 0; i < data.Length; i++)
                {
                    if (i < customEntropy.Length)
                        data[i] = (byte)(data[i] ^ customEntropy[i]);
                }

            uint[] nums = new uint[12];
            for (int i = 0; i < 12; i++)
            {
                nums[i] = (uint)(BitConverter.ToUInt32(data, i * sizeof(uint)) % (words.Length - 1));
            }

            return nums;
        }


        public static byte[] Sha256(string seed, int itter = 1)
        {
            return Sha256(Encoding.UTF8.GetBytes(seed), itter);
        }

        public static byte[] Sha256(byte[] src, int itter = 1)
        {
            byte[] res = src;
            var sha256 = SHA256.Create();
            for (int i = 0; i < itter; i++) res = sha256.ComputeHash(res);

            return res;
        }

        public static string HexFromBytes(this byte[] src)
        {
            string res = "";
            for (int i = 0; i < src.Length; i++)
                res += $"{src[i]:x2}";
            return res;
        }




        public static string ReadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourcePath = name;

            resourcePath = assembly.GetManifestResourceNames().Single(str => str.EndsWith(name));

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
