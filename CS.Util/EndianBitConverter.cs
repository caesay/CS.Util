using System;
using System.Linq;

namespace CS.Util
{
    internal sealed class BigEndianBitConverter : EndianBitConverter
    {
        public override bool IsLittleEndian => false;
        protected override long FromBytesImpl(byte[] buffer, int index, int bytes)
        {
            long ret = 0;
            for (int i = 0; i < bytes; i++)
            {
                ret = unchecked((ret << 8) | buffer[index + i]);
            }
            return ret;
        }
        protected override byte[] ToBytesImpl(long value, int bytes)
        {
            byte[] buffer = new byte[bytes];
            int endOffset = bytes - 1;
            for (int i = 0; i < bytes; i++)
            {
                buffer[endOffset - i] = unchecked((byte)(value & 0xff));
                value = value >> 8;
            }
            return buffer;
        }
    }

    internal sealed class LittleEndianBitConverter : EndianBitConverter
    {
        public override bool IsLittleEndian => true;
        protected override long FromBytesImpl(byte[] buffer, int index, int bytes)
        {
            long ret = 0;
            for (int i = 0; i < bytes; i++)
            {
                ret = unchecked((ret << 8) | buffer[index + bytes - 1 - i]);
            }
            return ret;
        }
        protected override byte[] ToBytesImpl(long value, int bytes)
        {
            byte[] buffer = new byte[bytes];
            for (int i = 0; i < bytes; i++)
            {
                buffer[i] = unchecked((byte)(value & 0xff));
                value = value >> 8;
            }
            return buffer;
        }
    }

    public abstract class EndianBitConverter
    {
        public abstract bool IsLittleEndian { get; }

        public static EndianBitConverter Big => _big ?? (_big = new BigEndianBitConverter());
        public static EndianBitConverter Little => _little ?? (_little = new LittleEndianBitConverter());

        private static BigEndianBitConverter _big;
        private static LittleEndianBitConverter _little;

        protected EndianBitConverter() { }

        protected long FromBytes(byte[] buffer, int index, int bytes)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (index + bytes > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(bytes));

            return FromBytesImpl(buffer, index, bytes);
        }
        protected abstract long FromBytesImpl(byte[] buffer, int index, int bytes);
        protected byte[] ToBytes(long value, int bytes)
        {
            return ToBytesImpl(value, bytes);
        }
        protected abstract byte[] ToBytesImpl(long value, int bytes);

        public byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }
        public byte[] GetBytes(char value)
        {
            return ToBytes(value, 2);
        }
        public byte[] GetBytes(short value)
        {
            return ToBytes(value, 2);
        }
        public byte[] GetBytes(int value)
        {
            return ToBytes(value, 4);
        }
        public byte[] GetBytes(long value)
        {
            return ToBytes(value, 8);
        }
        public byte[] GetBytes(ushort value)
        {
            return ToBytes(value, 2);
        }
        public byte[] GetBytes(uint value)
        {
            return ToBytes(value, 4);
        }
        public byte[] GetBytes(ulong value)
        {
            return ToBytes(unchecked((long)value), 8);
        }
        public unsafe byte[] GetBytes(float value)
        {
            return GetBytes(*(int*)&value);
        }
        public unsafe byte[] GetBytes(double value)
        {
            return GetBytes(*(long*)&value);
        }
        [Obsolete("This approach is not standardized. The recommended approach is to down-cast to a double instead.")]
        public byte[] GetBytes(decimal value)
        {
            // there is no real specified format for transmitting C# decimals (128bits).
            // this will write four sequential 
            return decimal.GetBits(value).SelectMany(GetBytes).ToArray();
        }

        public char ToChar(byte[] value, int index = 0)
        {
            return unchecked((char)(FromBytes(value, index, 2)));
        }
        public short ToInt16(byte[] value, int index = 0)
        {
            return unchecked((short)(FromBytes(value, index, 2)));
        }
        public int ToInt32(byte[] value, int index = 0)
        {
            return unchecked((int)(FromBytes(value, index, 4)));
        }
        public long ToInt64(byte[] value, int index = 0)
        {
            return FromBytes(value, index, 8);
        }
        public ushort ToUInt16(byte[] value, int index = 0)
        {
            return unchecked((ushort)(FromBytes(value, index, 2)));
        }
        public uint ToUInt32(byte[] value, int index = 0)
        {
            return unchecked((uint)(FromBytes(value, index, 4)));
        }
        public ulong ToUInt64(byte[] value, int index = 0)
        {
            return unchecked((ulong)(FromBytes(value, index, 8)));
        }
        public unsafe float ToSingle(byte[] value, int index = 0)
        {
            int val = ToInt32(value, index);
            return *(float*)&val;
        }
        public unsafe double ToDouble(byte[] value, int index = 0)
        {
            long val = ToInt64(value, index);
            return *(double*)&val;
        }
        public bool ToBoolean(byte[] value, int index = 0)
        {
            return BitConverter.ToBoolean(value, index);
        }
        public decimal ToDecimal(byte[] value, int index = 0)
        {
            // there is no real format for transmitting decimals (128bits).
            // this basically assumes there will be four sequential 32bit integers
            // all in their own endianness.
            int[] parts = new int[4];
            for (int i = 0; i < 4; i++)
            {
                parts[i] = ToInt32(value, index + i * 4);
            }
            return new decimal(parts);
        }
    }
}