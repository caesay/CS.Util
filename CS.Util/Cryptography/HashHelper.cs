using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Cryptography
{
    internal class HashHelper
    {
        [DllImport("bcrypt.dll")]
        public static extern int BCryptHashData(SafeHandle hHash, IntPtr pbInput, int cbInput, int dwFlags);
        [DllImport("kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, IntPtr size);
        public static string GetHashHex(byte[] hash)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
