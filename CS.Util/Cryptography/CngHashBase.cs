using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CS.Util.Extensions;

namespace CS.Util.Cryptography
{
    public enum SaltPlacement
    {
        Before,
        After
    }
    public class CngHashBase<T> where T : HashAlgorithm
    {
        /// <summary>
        /// This changes the salt placement statically.
        /// </summary>
        public static SaltPlacement SaltPlacement { get; set; } = SaltPlacement.After;

        /// <summary>
        /// This changes the encoding statically.
        /// </summary>
        public static Encoding Encoding { get; set; } = Encoding.UTF8;

        internal CngHashBase()
        {
        }

        public static string Compute(string input)
        {
            return Compute(input, "");
        }
        public static string Compute(string input, string salt)
        {
            string combined = SaltPlacement == SaltPlacement.After
                ? input + salt
                : salt + input;
            byte[] inputBytes = Encoding.GetBytes(combined);
            var alg = CreateNew();
            byte[] hash = alg.ComputeHash(inputBytes);
            alg.Initialize();
            return HashHelper.GetHashHex(hash);
        }
        public static string Compute(string input, byte[] salt)
        {
            return Compute(Encoding.GetBytes(input), salt);
        }

        public static string Compute(byte[] input)
        {
            return Compute(input, "");
        }
        public static string Compute(byte[] input, string salt)
        {
            return Compute(input, Encoding.GetBytes(salt));
        }
        public static string Compute(byte[] input, byte[] salt)
        {
            byte[] combine = new byte[input.Length + salt.Length];
            var bytesPin = GCHandle.Alloc(combine, GCHandleType.Pinned);
            try
            {
                if (SaltPlacement == SaltPlacement.After)
                {
                    Buffer.BlockCopy(input, 0, combine, 0, input.Length);
                    Buffer.BlockCopy(salt, 0, combine, input.Length, salt.Length);
                }
                else
                {
                    Buffer.BlockCopy(salt, 0, combine, 0, salt.Length);
                    Buffer.BlockCopy(input, 0, combine, salt.Length, input.Length);
                }
                var alg = CreateNew();
                byte[] hash = alg.ComputeHash(combine);
                alg.Initialize();
                return HashHelper.GetHashHex(hash);
            }
            finally
            {
                for (var i = 0; i < combine.Length; i++)
                    combine[i] = 0;
                bytesPin.Free();
            }
        }

        public static string Compute(Stream input)
        {
            var alg = CreateNew();
            var hex = HashHelper.GetHashHex(alg.ComputeHash(input));
            alg.Initialize();
            return hex;
        }
        public static string Compute(Stream input, string salt)
        {
            return Compute(input, Encoding.GetBytes(salt));
        }
        public static string Compute(Stream input, byte[] salt)
        {
            var tmp = CreateNew();
            int bufferSize = 65535;
            byte[] buffer = new byte[bufferSize];
            int readBytes;
            var placement = SaltPlacement;
            var saltBytes = salt;

            if (saltBytes.Length > 0 && placement == SaltPlacement.Before)
            {
                Buffer.BlockCopy(saltBytes, 0, buffer, 0, saltBytes.Length);
                readBytes = input.Read(buffer, saltBytes.Length, bufferSize - saltBytes.Length);
                tmp.TransformBlock(buffer, 0, readBytes, buffer, 0);
            }

            while ((readBytes = input.Read(buffer, 0, bufferSize)) > 0)
            {
                tmp.TransformBlock(buffer, 0, readBytes, buffer, 0);
            }

            if (saltBytes.Length > 0 && placement == SaltPlacement.After)
            {
                tmp.TransformFinalBlock(saltBytes, 0, saltBytes.Length);
            }
            else
            {
                tmp.TransformFinalBlock(new byte[0], 0, 0);
            }
            var hash = tmp.Hash;
            tmp.Initialize();
            return HashHelper.GetHashHex(hash);
        }

        public static string Compute(SecureString input)
        {
            return Compute(input, "");
        }
        public static string Compute(SecureString input, string salt)
        {
            return Compute(input, Encoding.GetBytes(salt));
        }
        public static string Compute(SecureString input, byte[] salt)
        {
            var alg = CreateNew();
            try
            {
                byte[] hash = UnmanagedSecureHash(alg, input, salt);
                return HashHelper.GetHashHex(hash);
            }
            catch (Exception e)
            {
                // fallback to managed method (less secure).
                alg.Initialize();
                System.Diagnostics.Debug.WriteLine("Error in UnmanagedSecureHash: " + e.Message);

                byte[] hash = ManagedSecureHash(alg, input, salt);
                return HashHelper.GetHashHex(hash);
            }
        }

        private static HashAlgorithm CreateNew()
        {
            return Activator.CreateInstance<T>();
        }
        private static byte[] ManagedSecureHash(HashAlgorithm alg, SecureString input, byte[] salt)
        {
            var bstr = Marshal.SecureStringToBSTR(input);
            var length = Marshal.ReadInt32(bstr, -4);
            var bytes = new byte[length + salt.Length];

            var bytesPin = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                if (SaltPlacement == SaltPlacement.After)
                {
                    Marshal.Copy(bstr, bytes, 0, length);
                    Buffer.BlockCopy(salt, 0, bytes, length, salt.Length);
                }
                else
                {
                    Buffer.BlockCopy(salt, 0, bytes, 0, salt.Length);
                    Marshal.Copy(bstr, bytes, salt.Length, length);
                }

                byte[] hash = alg.ComputeHash(bytes);
                alg.Initialize();
                return hash;
            }
            finally
            {
                Marshal.ZeroFreeBSTR(bstr);
                for (var i = 0; i < bytes.Length; i++)
                    bytes[i] = 0;
                bytesPin.Free();
            }
        }
        private static unsafe byte[] UnmanagedSecureHash(HashAlgorithm impl, SecureString secure, byte[] salt)
        {
            if (impl == null) throw new ArgumentNullException(nameof(impl));
            if (secure == null) throw new ArgumentNullException(nameof(secure));

            var algField = typeof(T).GetField("m_hashAlgorithm", BindingFlags.Instance | BindingFlags.NonPublic);
            if (algField == null) throw new NotSupportedException("Failed to retrieve unmanaged hash handle.");
            var alg = algField.GetValue(impl);

            var hwndField = alg.GetType().GetField("m_hashHandle", BindingFlags.Instance | BindingFlags.NonPublic);
            if (hwndField == null) throw new NotSupportedException("Failed to retrieve unmanaged hash handle.");
            var hwnd = (SafeHandle)hwndField.GetValue(alg);

            IntPtr clearText = Marshal.SecureStringToBSTR(secure);
            try
            {
                var clearTextByteLength = Encoding.GetByteCount((char*)clearText, secure.Length);
                var clearTextWithSalt = Marshal.AllocHGlobal(clearTextByteLength + salt.Length);
                try
                {
                    if (SaltPlacement == SaltPlacement.After)
                    {
                        Marshal.Copy(salt, 0, clearTextWithSalt + clearTextByteLength, salt.Length);
                        Encoding.GetBytes((char*)clearText, secure.Length, (byte*)clearTextWithSalt, clearTextByteLength);
                    }
                    else
                    {
                        Marshal.Copy(salt, 0, clearTextWithSalt, salt.Length);
                        Encoding.GetBytes((char*)clearText, secure.Length, (byte*)(clearTextWithSalt + salt.Length), clearTextByteLength);
                    }

                    Marshal.ZeroFreeBSTR(clearText);
                    clearText = IntPtr.Zero;

                    var error = HashHelper.BCryptHashData(hwnd, clearTextWithSalt, clearTextByteLength + salt.Length, 0);
                    if (error != 0)
                        throw new CryptographicException(error);
                }
                finally
                {
                    HashHelper.ZeroMemory(clearTextWithSalt, (IntPtr)(clearTextByteLength + salt.Length));
                    Marshal.FreeHGlobal(clearTextWithSalt);
                }
            }
            finally
            {
                if (clearText != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(clearText);
            }

            var hash = (byte[])((byte[])typeof(T).GetMethod("HashFinal", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(impl, null)).Clone();
            impl.Initialize();
            return hash;
        }
    }
}
