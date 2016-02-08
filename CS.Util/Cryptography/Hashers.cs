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
}
