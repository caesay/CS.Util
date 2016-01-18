using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ReadUntil(this Stream stream, byte delimiter, bool includeDelimiter = true)
        {
            byte[] buffer = new byte[10240];
            int index = 0;

            while (true)
            {
                int cast;
                try
                {
                    cast = stream.ReadByte();
                }
                catch (Exception ex) when (ex is SocketException || ex is IOException)
                {
                    if (index == 0) return null;
                    Array.Resize(ref buffer, index);
                    return buffer;
                }
                if (cast == -1)
                {
                    //end of stream
                    Array.Resize(ref buffer, index);
                    return buffer;
                }
                byte b = (byte)cast;
                if (includeDelimiter || b != delimiter)
                {
                    if (index + 1 > buffer.Length)
                    {
                        Array.Resize(ref buffer, buffer.Length * 2);
                    }
                    buffer[index] = b;
                    index++;
                }
                if (b == delimiter)
                {
                    Array.Resize(ref buffer, index);
                    return buffer;
                }
            }
        }

        public static byte[] Read(this Stream stream, int length)
        {
            byte[] buf = new byte[length];
            int read = stream.FillBuffer(buf, 0, length);
            if (read < length)
                Array.Resize(ref buf, read);
            return buf;
        }
        public static async Task<byte[]> ReadAsync(this Stream stream, int length)
        {
            byte[] buf = new byte[length];
            int read = await stream.FillBufferAsync(buf, 0, length);
            if (read < length)
                Array.Resize(ref buf, read);
            return buf;
        }
        public static async Task<byte[]> ReadAsync(this Stream stream, int length, CancellationToken token)
        {
            byte[] buf = new byte[length];
            int read = await stream.FillBufferAsync(buf, 0, length, token);
            if (read < length)
                Array.Resize(ref buf, read);
            return buf;
        }

        public static int FillBuffer(this Stream stream, byte[] buffer, int offset, int length)
        {
            int totalRead = 0;
            while (length > 0)
            {
                var read = stream.Read(buffer, offset, length);
                if (read == 0)
                    return totalRead;
                offset += read;
                length -= read;
                totalRead += read;
            }
            return totalRead;
        }
        public static async Task<int> FillBufferAsync(this Stream stream, byte[] buffer, int offset, int length)
        {
            int totalRead = 0;
            while (length > 0)
            {
                var read = await stream.ReadAsync(buffer, offset, length);
                if (read == 0)
                    return totalRead;
                offset += read;
                length -= read;
                totalRead += read;
            }
            return totalRead;
        }
        public static async Task<int> FillBufferAsync(this Stream stream, byte[] buffer, int offset, int length, CancellationToken token)
        {
            int totalRead = 0;
            while (length > 0)
            {
                var read = await stream.ReadAsync(buffer, offset, length, token);
                if (read == 0)
                    return totalRead;
                offset += read;
                length -= read;
                totalRead += read;
            }
            return totalRead;
        }
    }
}
