using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Cryptography
{
    public class Rfc2898DeriveBytes_SHA512 : DeriveBytes
    {
        private byte[] _buffer;
        private byte[] _salt;
        private HMACSHA512 _HMACSHA512;  // The pseudo-random generator function used in PBKDF2

        private uint _iterations;
        private uint _block;
        private int _startIndex;
        private int _endIndex;

        private static RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        private const int BlockSize = 20;

        public Rfc2898DeriveBytes_SHA512(string password, int saltSize) : this(password, saltSize, 1000) { }
        public Rfc2898DeriveBytes_SHA512(string password, int saltSize, int iterations)
        {
            if (saltSize < 0)
                throw new ArgumentOutOfRangeException("saltSize", "Salt size must be positive.");

            byte[] salt = new byte[saltSize];
            _rng.GetBytes(salt);

            Salt = salt;
            IterationCount = iterations;
            _HMACSHA512 = new HMACSHA512(new UTF8Encoding(false).GetBytes(password));
            Initialize();
        }
        public Rfc2898DeriveBytes_SHA512(string password, byte[] salt) : this(password, salt, 1000) { }
        public Rfc2898DeriveBytes_SHA512(string password, byte[] salt, int iterations) : this(new UTF8Encoding(false).GetBytes(password), salt, iterations) { }
        public Rfc2898DeriveBytes_SHA512(byte[] password, byte[] salt, int iterations)
        {
            Salt = salt;
            IterationCount = iterations;
            _HMACSHA512 = new HMACSHA512(password);
            Initialize();
        }

        public int IterationCount
        {
            get { return (int)_iterations; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", "value must be positive");
                _iterations = (uint)value;
                Initialize();
            }
        }
        public byte[] Salt
        {
            get { return (byte[])_salt.Clone(); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (value.Length < 8)
                    throw new ArgumentException("Salt must be at least 8 chatacters");
                _salt = (byte[])value.Clone();
                Initialize();
            }
        }

        public override byte[] GetBytes(int cb)
        {
            if (cb <= 0)
                throw new ArgumentOutOfRangeException("cb", "must be positive");
            byte[] password = new byte[cb];

            int offset = 0;
            int size = _endIndex - _startIndex;
            if (size > 0)
            {
                if (cb >= size)
                {
                    Buffer.BlockCopy(_buffer, _startIndex, password, 0, size);
                    _startIndex = _endIndex = 0;
                    offset += size;
                }
                else
                {
                    Buffer.BlockCopy(_buffer, _startIndex, password, 0, cb);
                    _startIndex += cb;
                    return password;
                }
            }

            //BCLDebug.Assert(m_startIndex == 0 && m_endIndex == 0, "Invalid start or end index in the internal buffer.");

            while (offset < cb)
            {
                byte[] T_block = Func();
                int remainder = cb - offset;
                if (remainder > BlockSize)
                {
                    Buffer.BlockCopy(T_block, 0, password, offset, BlockSize);
                    offset += BlockSize;
                }
                else
                {
                    Buffer.BlockCopy(T_block, 0, password, offset, remainder);
                    offset += remainder;
                    Buffer.BlockCopy(T_block, remainder, _buffer, _startIndex, BlockSize - remainder);
                    _endIndex += (BlockSize - remainder);
                    return password;
                }
            }
            return password;
        }
        public override void Reset()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_buffer != null)
                Array.Clear(_buffer, 0, _buffer.Length);
            _buffer = new byte[BlockSize];
            _block = 1;
            _startIndex = _endIndex = 0;
        }
        internal static byte[] Int(uint i)
        {
            byte[] b = BitConverter.GetBytes(i);
            byte[] littleEndianBytes = { b[3], b[2], b[1], b[0] };
            return BitConverter.IsLittleEndian ? littleEndianBytes : b;
        }
        // This function is defined as follow : 
        // Func (S, i) = HMAC(S || i) | HMAC2(S || i) | ... | HMAC(iterations) (S || i)
        // where i is the block number. 
        private byte[] Func()
        {
            byte[] INT_block = Int(_block);

            _HMACSHA512.TransformBlock(_salt, 0, _salt.Length, _salt, 0);
            _HMACSHA512.TransformFinalBlock(INT_block, 0, INT_block.Length);
            byte[] temp = _HMACSHA512.Hash;
            _HMACSHA512.Initialize();

            byte[] ret = temp;
            for (int i = 2; i <= _iterations; i++)
            {
                temp = _HMACSHA512.ComputeHash(temp);
                for (int j = 0; j < BlockSize; j++)
                {
                    ret[j] ^= temp[j];
                }
            }

            // increment the block count.
            _block++;
            return ret;
        }

    }
}

