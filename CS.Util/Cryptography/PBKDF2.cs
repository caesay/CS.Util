using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Cryptography
{
    /// <summary>
    /// Provides a static PBKDF2 implementation with a varying keyed hash algorithm specified by the generic paramater.
    /// </summary>
    public class PBKDF2<T> : DeriveBytesBase<PBKDF2<T>> where T: HMAC
    {
        public static int DerivedKeyLength { get; }
        static PBKDF2()
        {
            using (var hmac = Activator.CreateInstance<T>())
            {
                DerivedKeyLength = hmac.HashSize / 8;
            }
        }

        internal PBKDF2()
        {
        }

        protected override byte[] GetBytes(byte[] password, byte[] salt, int iterationCount, int? derivedKeyLength = null)
        {
            var dklen = derivedKeyLength ?? DerivedKeyLength;
            using (var hmac = (T)Activator.CreateInstance(typeof(T), password))
            {
                int hashLength = hmac.HashSize / 8;
                if ((hmac.HashSize & 7) != 0)
                    hashLength++;
                int keyLength = dklen / hashLength;
                if ((long)dklen > (0xFFFFFFFFL * hashLength) || dklen < 0)
                    throw new ArgumentOutOfRangeException("dklen");
                if (dklen % hashLength != 0)
                    keyLength++;
                byte[] extendedkey = new byte[salt.Length + 4];
                Buffer.BlockCopy(salt, 0, extendedkey, 0, salt.Length);
                using (var ms = new System.IO.MemoryStream())
                {
                    for (int i = 0; i < keyLength; i++)
                    {
                        extendedkey[salt.Length] = (byte)(((i + 1) >> 24) & 0xFF);
                        extendedkey[salt.Length + 1] = (byte)(((i + 1) >> 16) & 0xFF);
                        extendedkey[salt.Length + 2] = (byte)(((i + 1) >> 8) & 0xFF);
                        extendedkey[salt.Length + 3] = (byte)(((i + 1)) & 0xFF);
                        byte[] u = hmac.ComputeHash(extendedkey);
                        Array.Clear(extendedkey, salt.Length, 4);
                        byte[] f = u;
                        for (int j = 1; j < iterationCount; j++)
                        {
                            u = hmac.ComputeHash(u);
                            for (int k = 0; k < f.Length; k++)
                            {
                                f[k] ^= u[k];
                            }
                        }
                        ms.Write(f, 0, f.Length);
                        Array.Clear(u, 0, u.Length);
                        Array.Clear(f, 0, f.Length);
                    }
                    byte[] dk = new byte[dklen];
                    ms.Position = 0;
                    ms.Read(dk, 0, dklen);
                    ms.Position = 0;
                    for (long i = 0; i < ms.Length; i++)
                    {
                        ms.WriteByte(0);
                    }
                    Array.Clear(extendedkey, 0, extendedkey.Length);
                    return dk;
                }
            }
        }
    }

    public class PBKDF2SHA1 : PBKDF2<HMACSHA1>
    {
        internal PBKDF2SHA1()
        {
        }
    }
    public class PBKDF2SHA256 : PBKDF2<HMACSHA256>
    {
        internal PBKDF2SHA256()
        {
        }
    }
    public class PBKDF2SHA512 : PBKDF2<HMACSHA512>
    {
        internal PBKDF2SHA512()
        {
        }
    }
}
