using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS.Network
{
    public class NetClient : IDisposable
    {
        public bool Connected => _client?.Connected == true;

        public event EventHandler<ClientEventArgs<Exception>> Error;
        public event EventHandler<byte[]> PacketRecieved;
        public event EventHandler Disconnected;

        private readonly string _hostname;
        private readonly int _port;
        private readonly bool _ssl;
        private Stream _stream;
        private TcpClient _client;

        public NetClient(string hostname, int port)
            : this(hostname, port, false)
        {
        }
        public NetClient(string hostname, int port, bool ssl)
        {
            _ssl = ssl;
            _hostname = hostname;
            _port = port;
        }
        internal NetClient(TcpClient client, Stream stream)
        {
            _client = client;
            _stream = stream;
            BeginRead();
        }

        public void Connect()
        {
            if (Connected)
                return;

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
            _client.Close();
            _stream.Dispose();
            _client = null;
            _stream = null;
        }
        public void Dispose()
        {
            Close();
        }

        public void Write(byte[] packet)
        {
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
            byte[] lengthPrefix = BitConverter.GetBytes(packet.Length);
            await _stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length, token);
            await _stream.WriteAsync(packet, 0, packet.Length, token);
            await _stream.FlushAsync(token);
        }

        private byte[] lengthBuffer;
        private byte[] dataBuffer;
        private int bytesReceived;

        private void BeginRead()
        {
            var callback = new AsyncCallback(EndRead);
            if (dataBuffer != null)
            {
                _stream.BeginRead(this.dataBuffer, this.bytesReceived, this.dataBuffer.Length - this.bytesReceived, callback, null);
            }
            else
            {
                if (lengthBuffer == null)
                    lengthBuffer = new byte[sizeof(int)];

                _stream.BeginRead(this.lengthBuffer, this.bytesReceived, this.lengthBuffer.Length - this.bytesReceived, callback, null);
            }
        }
        private void EndRead(IAsyncResult result)
        {
            int bytesRead = -1;
            try
            {
                bytesRead = _stream.EndRead(result);
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new ClientEventArgs<Exception>(ex));
                return;
            }

            // Get the number of bytes read into the buffer
            this.bytesReceived += bytesRead;

            // If we get a zero-length read, then that indicates the remote side graciously closed the connection
            if (bytesRead == 0)
            {
                Disconnected?.Invoke(this, new EventArgs());
                return;
            }

            if (this.dataBuffer == null)
            {
                // (We're currently receiving the length buffer)
                if (this.bytesReceived != sizeof(int))
                {
                    // We haven't gotten all the length buffer yet
                    BeginRead();
                }
                else
                {
                    // We've gotten the length buffer
                    int length = BitConverter.ToInt32(this.lengthBuffer, 0);

                    // Sanity check for length < 0
                    // This check will catch 50% of transmission errors that make it past both the IP and Ethernet checksums
                    if (length < 0)
                    {
                        Error?.Invoke(this, new ClientEventArgs<Exception>(new InvalidDataException("Packet length less than zero (corrupted message)")));
                        return;
                    }

                    // Zero-length packets are allowed as keepalives
                    if (length == 0)
                    {
                        this.bytesReceived = 0;
                        BeginRead();
                    }
                    else
                    {
                        // Create the data buffer and start reading into it
                        this.dataBuffer = new byte[length];
                        this.bytesReceived = 0;
                        BeginRead();
                    }
                }
            }
            else
            {
                if (this.bytesReceived != this.dataBuffer.Length)
                {
                    // We haven't gotten all the data buffer yet
                    BeginRead();
                }
                else
                {
                    // We've gotten an entire packet
                    PacketRecieved?.Invoke(this, dataBuffer);

                    // Start reading the length buffer again
                    this.dataBuffer = null;
                    this.bytesReceived = 0;
                    BeginRead();
                }
            }
        }
        private bool ServerCertValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            var ex = new ArgumentException($"Certificate error: {sslPolicyErrors}");
            Error?.Invoke(this, new ClientEventArgs<Exception>(ex));
            throw ex;
        }
    }
}
