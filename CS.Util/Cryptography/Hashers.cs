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
    /// <summary>
    /// Provides a static CNG (Cryptography Next Generation) implementation of the MD5 (Message Digest 5) 128-bit hashing algorithm
    /// </summary>
    public sealed class MD5 : CngHashBase<MD5Cng>
    {
        private MD5() { }
    }
    /// <summary>
    /// Provides a static CNG (Cryptography Next Generation) implementation of SHA (Secure Hash Algorithm)
    /// </summary>
    public sealed class SHA1 : CngHashBase<SHA1Cng>
    {
        private SHA1() { }
    }
    /// <summary>
    /// Provides a static CNG (Cryptography Next Generation) implementation of SHA (Secure Hash Algorithm) for 256-bit hash values
    /// </summary>
    public sealed class SHA256 : CngHashBase<SHA256Cng>
    {
        private SHA256() { }
    }
    /// <summary>
    /// Provides a static CNG (Cryptography Next Generation) implementation of SHA (Secure Hash Algorithm) for 384-bit hash values
    /// </summary>
    public sealed class SHA384 : CngHashBase<SHA384Cng>
    {
        private SHA384() { }
    }
    /// <summary>
    /// Provides a static CNG (Cryptography Next Generation) implementation of SHA (Secure Hash Algorithm) for 512-bit hash values
    /// </summary>
    public sealed class SHA512 : CngHashBase<SHA512Cng>
    {
        private SHA512() { }
    }
    /// <summary>
    /// Provides a static implementation of RIPEMD160 (RACE Integrity Primitives Evaluation Message Digest 160-bit).
    /// </summary>
    public sealed class RIPEMD : CngHashBase<RIPEMD160Managed>
    {
        private RIPEMD() { }
    }

    /// <summary>
    /// Provides a static PBKDF2 (SHA1) implementation using <see cref="Rfc2898DeriveBytes"/>.
    /// </summary>
    public sealed class PBKDF2 : DeriveBytesBase<Rfc2898DeriveBytes, PBKDF2.Rfc2898_SHA1_Generator>
    {
        private PBKDF2() { }
        public sealed class Rfc2898_SHA1_Generator : DeriveBytesGenerator<Rfc2898DeriveBytes>
        {
            public override Rfc2898DeriveBytes CreateNew(byte[] password, byte[] salt, int iterations)
            {
                return new Rfc2898DeriveBytes(PBKDF2.Encoding.GetString(password), salt, iterations);
            }
        }
    }

    /// <summary>
    /// Provides a static PBKDF2 (SHA512) implementation using <see cref="Rfc2898DeriveBytes_SHA512"/>.
    /// </summary>
    public sealed class PBKDF2_SHA512 : DeriveBytesBase<Rfc2898DeriveBytes_SHA512, PBKDF2_SHA512.Rfc2898_SHA512_Generator>
    {
        private PBKDF2_SHA512() { }
        public sealed class Rfc2898_SHA512_Generator : DeriveBytesGenerator<Rfc2898DeriveBytes_SHA512>
        {
            public override Rfc2898DeriveBytes_SHA512 CreateNew(byte[] password, byte[] salt, int iterations)
            {
                return new Rfc2898DeriveBytes_SHA512(password, salt, iterations);
            }
        }
    }
}
