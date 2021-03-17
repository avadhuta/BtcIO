using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BtcWalletTools
{
    static class WindowsClipboard
    {
        public static void SetText(string text)
        {
            OpenClipboard();

            EmptyClipboard();
            IntPtr hGlobal = default;
            try
            {
                var bytes = (text.Length + 1) * 2;
                hGlobal = Marshal.AllocHGlobal(bytes);

                if (hGlobal == default)
                {
                    ThrowWin32();
                }

                var target = GlobalLock(hGlobal);

                if (target == default)
                {
                    ThrowWin32();
                }

                try
                {
                    Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
                }
                finally
                {
                    GlobalUnlock(target);
                }

                if (SetClipboardData(cfUnicodeText, hGlobal) == default)
                {
                    ThrowWin32();
                }

                hGlobal = default;
            }
            finally
            {
                if (hGlobal != default)
                {
                    Marshal.FreeHGlobal(hGlobal);
                }

                CloseClipboard();
            }
        }

        public static void OpenClipboard()
        {
            var num = 10;
            while (true)
            {
                if (OpenClipboard(default))
                {
                    break;
                }

                if (--num == 0)
                {
                    ThrowWin32();
                }

                Thread.Sleep(100);
            }
        }

        const uint cfUnicodeText = 13;

        static void ThrowWin32()
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        static extern bool EmptyClipboard();
    }

    static class OsxClipboard
    {
        public static void SetText(string text)
        {
            var nsString = objc_getClass("NSString");
            IntPtr str = default;
            IntPtr dataType = default;
            try
            {
                str = objc_msgSend(objc_msgSend(nsString, sel_registerName("alloc")), sel_registerName("initWithUTF8String:"), text);
                dataType = objc_msgSend(objc_msgSend(nsString, sel_registerName("alloc")), sel_registerName("initWithUTF8String:"), NSPasteboardTypeString);

                var nsPasteboard = objc_getClass("NSPasteboard");
                var generalPasteboard = objc_msgSend(nsPasteboard, sel_registerName("generalPasteboard"));

                objc_msgSend(generalPasteboard, sel_registerName("clearContents"));
                objc_msgSend(generalPasteboard, sel_registerName("setString:forType:"), str, dataType);
            }
            finally
            {
                if (str != default)
                {
                    objc_msgSend(str, sel_registerName("release"));
                }

                if (dataType != default)
                {
                    objc_msgSend(dataType, sel_registerName("release"));
                }
            }
        }

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_getClass(string className);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr sel_registerName(string selectorName);

        const string NSPasteboardTypeString = "public.utf8-plain-text";
    }

    static class LinuxClipboard
    {
        public static void SetText(string text)
        {
            var tempFileName = Path.GetTempFileName();
            File.WriteAllText(tempFileName, text);
            try
            {
                BashRunner.Run($"cat {tempFileName} | xclip");
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        public static string GetText()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                BashRunner.Run($"xclip -o > {tempFileName}");
                return File.ReadAllText(tempFileName);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }

    static class BashRunner
    {
        public static string Run(string commandLine)
        {
            var errorBuilder = new StringBuilder();
            var outputBuilder = new StringBuilder();
            var arguments = $"-c \"{commandLine}\"";
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            })
            {
                process.Start();
                process.OutputDataReceived += (sender, args) => { outputBuilder.AppendLine(args.Data); };
                process.BeginOutputReadLine();
                process.ErrorDataReceived += (sender, args) => { errorBuilder.AppendLine(args.Data); };
                process.BeginErrorReadLine();
                if (!process.WaitForExit(500))
                {
                    var timeoutError = $@"Process timed out. Command line: bash {arguments}.
Output: {outputBuilder}
Error: {errorBuilder}";
                    throw new Exception(timeoutError);
                }
                if (process.ExitCode == 0)
                {
                    return outputBuilder.ToString();
                }

                var error = $@"Could not execute process. Command line: bash {arguments}.
Output: {outputBuilder}
Error: {errorBuilder}";
                throw new Exception(error);
            }
        }
    }

    public static class Shell
    {
        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            string result = Run("/bin/bash", $"-c \"{escapedArgs}\"");
            return result;
        }

        public static string Bat(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            string result = Run("cmd.exe", $"/c \"{escapedArgs}\"");
            return result;
        }

        private static string Run(string filename, string arguments)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }

    public static class OperatingSystem
    {
        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    public static class Clipboard
    {
        public static void Copy(string val)
        {
            if (OperatingSystem.IsWindows())
            {
                $"echo {val} | clip".Bat();
            }

            if (OperatingSystem.IsMacOS())
            {
                $"echo \"{val}\" | pbcopy".Bash();
            }
        }

        public static void Copy2(string val)
        {
            if (OperatingSystem.IsWindows())
            {
                WindowsClipboard.SetText(val);
            }

            if (OperatingSystem.IsLinux())
            {
                LinuxClipboard.SetText(val);
            }
        }
    }
}
