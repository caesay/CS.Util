using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace CS.Reactive.Network
{
    public class ByteArrayTransferProtocol : ITransferProtocol<byte[]>
    {
        public IObservable<MessageRecievedEventArgs<byte[]>> MessageRecieved => _observable.AsObservable();

        private Stream _stream;
        private byte[] _lengthBuffer;
        private byte[] _dataBuffer;
        private int _bytesReceived;
        private bool _reading;
        private Subject<MessageRecievedEventArgs<byte[]>> _observable;

        public ByteArrayTransferProtocol()
        {
            _observable = new Subject<MessageRecievedEventArgs<byte[]>>();
        }

        public void Dispose()
        {
            _observable?.OnCompleted();
            _stream?.Dispose();

        }

        public void BeginReadingAsync(Stream netStream)
        {
            _stream = netStream;
            if (_reading) return;
            _reading = true;
            BeginRead();
        }

        public async Task WriteAsync(byte[] packet)
        {
            byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);
            await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
            await _stream.WriteAsync(packet, 0, packet.Length);
            await _stream.FlushAsync();
        }

        public void Write(byte[] packet)
        {
            byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);
            _stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            _stream.Write(packet, 0, packet.Length);
            _stream.Flush();
        }

        private void BeginRead()
        {
            try
            {
                var callback = new AsyncCallback(EndRead);
                if (_dataBuffer != null)
                {
                    _stream.BeginRead(this._dataBuffer, this._bytesReceived,
                        this._dataBuffer.Length - this._bytesReceived, callback, null);
                }
                else
                {
                    if (_lengthBuffer == null)
                        _lengthBuffer = new byte[sizeof(int)];

                    _stream.BeginRead(this._lengthBuffer, this._bytesReceived,
                        this._lengthBuffer.Length - this._bytesReceived, callback, null);
                }
            }
            catch (ObjectDisposedException ex)
            {
                //not really important. just means that _stream has been disposed. we can just send an OnCompleted event.
                _observable.OnCompleted();
            }
            catch (Exception ex)
            {
                _observable.OnError(ex);
            }
        }
        private void EndRead(IAsyncResult result)
        {
            int bytesRead = -1;
            try
            {
                if (_stream == null || !_stream.CanRead)
                {
                    _observable.OnCompleted();
                    return;
                }
                bytesRead = _stream.EndRead(result);
            }
            catch (Exception ex)
            {
                _observable.OnError(ex);
            }

            // Get the number of bytes read into the buffer
            this._bytesReceived += bytesRead;

            // If we get a zero-length read, then that indicates the remote side graciously closed the connection
            if (bytesRead == 0)
            {
                _observable.OnCompleted();
                return;
            }

            if (this._dataBuffer == null)
            {
                // (We're currently receiving the length buffer)
                if (this._bytesReceived != sizeof(int))
                {
                    // We haven't gotten all the length buffer yet
                    BeginRead();
                }
                else
                {
                    // We've gotten the length buffer
                    int length = BitConverter.ToInt32(this._lengthBuffer, 0);

                    // Sanity check for length < 0
                    // This check will catch 50% of transmission errors that make it past both the IP and Ethernet checksums
                    if (length < 0)
                    {
                        _observable.OnError(new InvalidDataException("Packet length less than zero (corrupted message)"));
                        return;
                    }

                    // Zero-length packets are allowed as keepalives
                    if (length == 0)
                    {
                        this._bytesReceived = 0;
                        BeginRead();
                    }
                    else
                    {
                        // Create the data buffer and start reading into it
                        this._dataBuffer = new byte[length];
                        this._bytesReceived = 0;
                        BeginRead();
                    }
                }
            }
            else
            {
                if (this._bytesReceived != this._dataBuffer.Length)
                {
                    // We haven't gotten all the data buffer yet
                    BeginRead();
                }
                else
                {
                    // We've gotten an entire packet
                    _observable.OnNext(new MessageRecievedEventArgs<byte[]>(_dataBuffer));

                    // Start reading the length buffer again
                    this._dataBuffer = null;
                    this._bytesReceived = 0;
                    BeginRead();
                }
            }
        }
    }
}
