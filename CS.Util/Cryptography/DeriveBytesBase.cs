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
    public abstract class DeriveBytesBase<T> : IDisposable
        where T : DeriveBytesBase<T>
    {
        public static Encoding Encoding => Encoding.UTF8;
        public static int Iterations => 64000;

        internal DeriveBytesBase()
        {
        }
        ~DeriveBytesBase()
        {
            Dispose(false);
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
            using (var inst = Activator.CreateInstance<T>())
                return Convert.ToBase64String(inst.GetBytes(password, salt, Iterations));
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
                return Compute(bytes, salt);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstr);
                for (var i = 0; i < bytes.Length; i++)
                    bytes[i] = 0;
                bytesPin.Free();
            }
        }

        public static byte[] GetDerivedKey(int dklen, byte[] password, byte[] salt, int iterations)
        {
            using (var inst = Activator.CreateInstance<T>())
                return inst.GetBytes(password, salt, iterations, dklen);
        }

        protected abstract byte[] GetBytes(byte[] password, byte[] salt, int iterationCount, int? derivedKeyLength = null);

        protected virtual void Dispose(bool disposing) { }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
