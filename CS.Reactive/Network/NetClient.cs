using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS.Reactive
{
    public class NetClient : IDisposable
    {
        public bool Connected => _client?.Connected == true;
        public EndPoint RemoteEndpoint => _client?.Client.RemoteEndPoint;
        public IObservable<PacketRecievedEventArgs> PacketRecieved => _observable.AsObservable();

        private readonly string _hostname;
        private readonly int _port;
        private readonly bool _ssl;
        private Stream _stream;
        private TcpClient _client;
        private Subject<PacketRecievedEventArgs> _observable;

        public NetClient(string hostname, int port)
            : this(hostname, port, false)
        {
        }
        public NetClient(string hostname, int port, bool ssl)
        {
            _ssl = ssl;
            _hostname = hostname;
            _port = port;
            _observable = new Subject<PacketRecievedEventArgs>();
        }
        internal NetClient(TcpClient client, Stream stream)
        {
            _client = client;
            _stream = stream;
            _observable = new Subject<PacketRecievedEventArgs>();
        }

        public void Connect()
        {
            if (Connected)
                return;

            if (String.IsNullOrEmpty(_hostname) || _port == default(int))
                throw new ArgumentException("Specified hostname and Port must not be null.");

            _client = new TcpClient();
            _client.Connect(_hostname, _port);
            if (_ssl)
            {
                SslStream sslStream = new SslStream(_client.GetStream(), false, ServerCertValidator, null);
                sslStream.AuthenticateAsClient(_hostname);
                _stream = sslStream;
            }
            else
            {
                _stream = _client.GetStream();
            }
            BeginRead();
        }
        public async Task ConnectAsync()
        {
            if (Connected)
                return;

            if (String.IsNullOrEmpty(_hostname) || _port == default(int))
                throw new ArgumentException("Specified hostname and Port must not be null.");

            _client = new TcpClient();
            await _client.ConnectAsync(_hostname, _port);
            if (_ssl)
            {
                SslStream sslStream = new SslStream(_client.GetStream(), false, ServerCertValidator, null);
                sslStream.AuthenticateAsClient(_hostname);
                _stream = sslStream;
            }
            else
            {
                _stream = _client.GetStream();
            }
            BeginRead();
        }
        public void Close()
        {
            _client?.Close();
            _stream?.Dispose();
            _client = null;
            _stream = null;
        }
        public void Dispose()
        {
            Close();
        }

        public void Write(byte[] packet)
        {
            if (!Connected)
                throw new IOException("Can not write to a closed connection");
            byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);
            _stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            _stream.Write(packet, 0, packet.Length);
            _stream.Flush();
        }
        public Task WriteAsync(byte[] packet)
        {
            return WriteAsync(packet, CancellationToken.None);
        }
        public async Task WriteAsync(byte[] packet, CancellationToken token)
        {
            if (!Connected)
                throw new IOException("Can not write to a closed connection");
            byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);
            await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length, token);
            await _stream.WriteAsync(packet, 0, packet.Length, token);
            await _stream.FlushAsync(token);
        }

        private byte[] _lengthBuffer;
        private byte[] _dataBuffer;
        private int _bytesReceived;
        private bool _reading;

        internal void StartReading()
        {
            if (_reading) return;
            _reading = true;
            BeginRead();
        }

        private void BeginRead()
        {
            var callback = new AsyncCallback(EndRead);
            if (_dataBuffer != null)
            {
                _stream.BeginRead(this._dataBuffer, this._bytesReceived, this._dataBuffer.Length - this._bytesReceived, callback, null);
            }
            else
            {
                if (_lengthBuffer == null)
                    _lengthBuffer = new byte[sizeof(int)];

                _stream.BeginRead(this._lengthBuffer, this._bytesReceived, this._lengthBuffer.Length - this._bytesReceived, callback, null);
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
                    _observable.OnNext(new PacketRecievedEventArgs(_dataBuffer));

                    // Start reading the length buffer again
                    this._dataBuffer = null;
                    this._bytesReceived = 0;
                    BeginRead();
                }
            }
        }
        private bool ServerCertValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            var ex = new ArgumentException($"Certificate error: {sslPolicyErrors}");
            _observable.OnError(ex);
            throw ex;
        }
    }
}
