using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CS.Util.Extensions;

namespace CS.Util.Cryptography
{
    public abstract class DeriveBytesGenerator<T>
    {
        internal DeriveBytesGenerator()
        {
        }
        public abstract T CreateNew(byte[] password, byte[] salt, int iterations);
    }
    public class DeriveBytesBase<TAlg, TGen>
       where TAlg : DeriveBytes
       where TGen : DeriveBytesGenerator<TAlg>
    {
        public static Encoding Encoding { get; set; } = Encoding.UTF8;
        public static int Iterations { get; set; } = 5000;
        public static int DerivedKeyLength { get; set; } = 20;

        internal DeriveBytesBase()
        {
        }

        public static string Compute(string password, string salt)
        {
            return Compute(Encoding.GetBytes(password), Encoding.GetBytes(salt));
        }
        public static string Compute(string password, byte[] salt)
        {
            return Compute(Encoding.GetBytes(password), salt);
        }
        public static string Compute(byte[] password, string salt)
        {
            return Compute(password, Encoding.GetBytes(salt));
        }
        public static string Compute(byte[] password, byte[] salt)
        {
            return Convert.ToBase64String(GetBytes(DerivedKeyLength, password, salt, Iterations));
        }
        public static string Compute(SecureString password, string salt)
        {
            return Compute(password, Encoding.GetBytes(salt));
        }
        public static string Compute(SecureString password, byte[] salt)
        {
            var bstr = Marshal.SecureStringToBSTR(password);
            var length = Marshal.ReadInt32(bstr, -4);
            var bytes = new byte[length];

            var bytesPin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.Copy(bstr, bytes, 0, length);
                return Convert.ToBase64String(GetBytes(DerivedKeyLength, bytes, salt, Iterations));
            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstr);
                for (var i = 0; i < bytes.Length; i++)
                    bytes[i] = 0;
                bytesPin.Free();
            }
        }

        private static byte[] GetBytes(int dklen, byte[] password, byte[] salt, int iterationCount)
        {
            TGen generator = null;
            try
            {
                generator = Activator.CreateInstance<TGen>();
                // create instance of DeriveBytes using generator
                using (var alg = generator.CreateNew(password, salt, iterationCount))
                {
                    return alg.GetBytes(dklen);
                }
            }
            finally
            {
                if(generator is IDisposable)
                    ((IDisposable)generator).Dispose();
            }
        }
    }
}
