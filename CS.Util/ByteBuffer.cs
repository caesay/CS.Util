using System;
namespace CS.Util
{
    public class ByteBuffer
    {
        public int Position { get; private set; }
        public int Length { get { return _buffer.Length; } }

        private readonly byte[] _buffer;

        public ByteBuffer(byte[] buffer)
        {
            this._buffer = buffer;
        }

        public byte ReadByte()
        {
            CanReadOrThrow(1);
            return _buffer[Position++];
        }
        public byte[] ReadBytes(int length)
        {
            CanReadOrThrow(length);
            var value = new byte[length];
            Buffer.BlockCopy(_buffer, Position, value, 0, length);
            Position += length;
            return value;
        }

        public short ReadInt16()
        {
            CanReadOrThrow(2);
            short value = (short)(_buffer[Position]
                | (_buffer[Position + 1] << 8));
            Position += 2;
            return value;
        }
        public int ReadInt32()
        {
            CanReadOrThrow(4);
            int value = _buffer[Position]
                | (_buffer[Position + 1] << 8)
                | (_buffer[Position + 2] << 16)
                | (_buffer[Position + 3] << 24);
            Position += 4;
            return value;
        }
        public long ReadInt64()
        {
            CanReadOrThrow(8);
            uint low = (uint)(_buffer[Position]
                | (_buffer[Position + 1] << 8)
                | (_buffer[Position + 2] << 16)
                | (_buffer[Position + 3] << 24));

            uint high = (uint)(_buffer[Position + 4]
                | (_buffer[Position + 5] << 8)
                | (_buffer[Position + 6] << 16)
                | (_buffer[Position + 7] << 24));

            long value = (((long)high) << 32) | low;
            Position += 8;
            return value;
        }
        public float ReadSingle()
        {
            if (!BitConverter.IsLittleEndian)
            {
                var bytes = ReadBytes(4);
                Array.Reverse(bytes);
                return BitConverter.ToSingle(bytes, 0);
            }

            CanReadOrThrow(4);
            float value = BitConverter.ToSingle(_buffer, Position);
            Position += 4;
            return value;
        }
        public double ReadDouble()
        {
            if (!BitConverter.IsLittleEndian)
            {
                var bytes = ReadBytes(8);
                Array.Reverse(bytes);
                return BitConverter.ToDouble(bytes, 0);
            }

            CanReadOrThrow(8);
            double value = BitConverter.ToDouble(_buffer, Position);
            Position += 8;
            return value;
        }

        void CanReadOrThrow(int count)
        {
            if (Position + count > _buffer.Length)
                throw new ArgumentOutOfRangeException();
        }
    }
}
