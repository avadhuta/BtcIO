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

namespace BtcWalletTools
{
    public static class Tech
    {

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

        public static string RandomSeed()
        {
            string res = "";
            for (int i = 0; i < 12; i++) res += words[Rnd2()].ToLower() + (i < 11 ? " " : "");

            return res;
        }


        static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        public static uint Rnd2()
        {
            var data = new byte[sizeof(uint)];
            rng.GetBytes(data);
            return (uint) (BitConverter.ToUInt32(data, 0) % (words.Length - 1));
        }


        public static byte[] Sha256(string seed, int itter = 1)
        {
            byte[] b = Encoding.UTF8.GetBytes(seed);
            for (int i = 0; i < itter; i++) b = SHA256.Create().ComputeHash(b);
            b = SHA256.Create().ComputeHash(b);

            return b;
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
