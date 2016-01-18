using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        /// This changes the salt placement statically for all of the algorithms that implement HashBase, be warned.
        /// </summary>
        public static SaltPlacement SaltPlacement { get; set; } = SaltPlacement.After;

        /// <summary>
        /// This changes the encoding statically for all of the algorithms that implement HashBase, be warned.
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
            return ComputeSecureHash(CreateNew(), input, salt);
        }
        public static string Compute(SecureString input, byte[] salt)
        {
            return ComputeSecureHash(CreateNew(), input, Encoding.GetString(salt));
        }

        private static HashAlgorithm CreateNew()
        {
            return Activator.CreateInstance<T>();
        }
        private static string ComputeSecureHash(HashAlgorithm alg, SecureString input, string salt = "")
        {
            try
            {
                byte[] hash = UnmanagedSecureHash(alg, input, Encoding.GetBytes(salt));
                return HashHelper.GetHashHex(hash);
            }
            catch (Exception e)
            {
                // fallback to unsafe/managed method.
                alg.Initialize();
                System.Diagnostics.Debug.WriteLine("Error in UnmanagedSecureHash: " + e.Message);
                byte[] hash;
                if (SaltPlacement == SaltPlacement.After)
                {
                    hash = alg.ComputeHash(Encoding.GetBytes(GetUnSafeString(input) + salt));
                }
                else
                {
                    hash = alg.ComputeHash(Encoding.GetBytes(salt + GetUnSafeString(input)));
                }
                alg.Initialize();
                return HashHelper.GetHashHex(hash);
            }
        }
        private static unsafe string GetUnSafeString(SecureString secure)
        {
            IntPtr ptr = Marshal.SecureStringToBSTR(secure);
            try
            {
                return new string((char*)ptr);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
        }
        private static unsafe byte[] UnmanagedSecureHash(HashAlgorithm impl, SecureString secure, byte[] salt)
        {
            if (impl == null) throw new ArgumentNullException("implementation");
            if (secure == null) throw new ArgumentNullException("secureString");

            var alg = typeof(T).GetField("m_hashAlgorithm", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(impl);
            var hwnd = (SafeHandle)alg.GetType().GetField("m_hashHandle", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(alg);

            IntPtr clearText = Marshal.SecureStringToBSTR(secure);
            try
            {
                var clearTextBytes = Encoding.GetByteCount((char*)clearText, secure.Length);
                var clearTextWithSalt = Marshal.AllocHGlobal(clearTextBytes + salt.Length);
                try
                {
                    if (SaltPlacement == SaltPlacement.After)
                    {
                        Marshal.Copy(salt, 0, clearTextWithSalt + clearTextBytes, salt.Length);
                        Encoding.GetBytes((char*)clearText, secure.Length, (byte*)clearTextWithSalt, clearTextBytes);
                    }
                    else
                    {
                        Marshal.Copy(salt, 0, clearTextWithSalt, salt.Length);
                        Encoding.GetBytes((char*)clearText, secure.Length, (byte*)(clearTextWithSalt + salt.Length), clearTextBytes);
                    }

                    Marshal.ZeroFreeBSTR(clearText);
                    clearText = IntPtr.Zero;

                    var error = HashHelper.BCryptHashData(hwnd, clearTextWithSalt, clearTextBytes + salt.Length, 0);
                    if (error != 0)
                        throw new CryptographicException(error);
                }
                finally
                {
                    HashHelper.ZeroMemory(clearTextWithSalt, (IntPtr)clearTextBytes);
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
