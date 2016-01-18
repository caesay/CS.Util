using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util
{
    public static class RandomEx
    {
        private static Random _random = new Random();
        private static object _lock = new object();

        public static int GetInteger()
        {
            lock (_lock)
            {
                return _random.Next();
            }
        }
        public static int GetInteger(int max)
        {
            lock (_lock)
            {
                return _random.Next(max);
            }
        }
        public static int GetInteger(int min, int max)
        {
            lock (_lock)
            {
                return _random.Next(min, max);
            }
        }
        public static string GetGuid()
        {
            return Guid.NewGuid().ToString();
        }
        public static string GetString(int length)
        {
            lock (_lock)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());
            }
        }
        public static string GetCryptoUniqueString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            data = new byte[length];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(length);
            foreach (byte b in data)
            {
                result.Append(chars[b % ((((((((chars.Length))))))))]); //brackets courtesy of Timwi
            }
            return result.ToString();
        }
    }
}
