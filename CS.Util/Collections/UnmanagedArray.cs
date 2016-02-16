using System;
using System.Runtime.InteropServices;

namespace CS.Util.Collections
{
    public unsafe class UnmanagedArray<T> : IDisposable
        where T : struct
    {
        public IntPtr Handle { get; private set; }

        private bool _disposed;
        private short _size;

        public UnmanagedArray(int count)
        {
            _size = getSize<T>();
            var newSizeInBytes = _size*count;
            Handle = Marshal.AllocHGlobal(newSizeInBytes);

            byte* newArrayPointer = (byte*)Handle.ToPointer();
            for (int i = 0; i < newSizeInBytes; i++)
                *(newArrayPointer + i) = 0;
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Marshal.FreeHGlobal(Handle);
        }
        public void Resize(int newCount)
        {
            Handle = Marshal.ReAllocHGlobal(Handle, new IntPtr(_size * newCount));
        }

        private static short getSize<T>()
        {
            var t = typeof(T);
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Boolean:
                    return sizeof(bool);
                case TypeCode.Char:
                    return sizeof(char);
                case TypeCode.SByte:
                    return sizeof(sbyte);
                case TypeCode.Byte:
                    return sizeof(byte);
                case TypeCode.Int16:
                    return sizeof(short);
                case TypeCode.UInt16:
                    return sizeof(ushort);
                case TypeCode.Int32:
                    return sizeof(int);
                case TypeCode.UInt32:
                    return sizeof(uint);
                case TypeCode.Int64:
                    return sizeof(long);
                case TypeCode.UInt64:
                    return sizeof(ulong);
                case TypeCode.Single:
                    return sizeof(float);
                case TypeCode.Double:
                    return sizeof(double);
                case TypeCode.Decimal:
                    return sizeof(decimal);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
