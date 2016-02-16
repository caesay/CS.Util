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
using CS.Reactive.Network;

namespace CS.Reactive.Network
{
    public class NetClient<TMsg> : MessageTransferClient<TMsg>
    {
        public override IObservable<MessageRecievedEventArgs<TMsg>> MessageRecieved => _protocol.MessageRecieved;

        private readonly string _hostname;
        private readonly int _port;
        private readonly bool _ssl;
        private ITransferProtocol<TMsg> _protocol;

        public NetClient(string hostname, int port, ITransferProtocol<TMsg> protocol)
            : this(hostname, port, false, protocol)
        {
        }
        public NetClient(string hostname, int port, bool ssl, ITransferProtocol<TMsg> protocol)
        {
            _ssl = ssl;
            _hostname = hostname;
            _port = port;
            _protocol = protocol;
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
            StartReading();
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
            StartReading();
        }

        internal override void StartReading()
        {
            _protocol.BeginReadingAsync(_stream);
        }
        public override void Write(TMsg packet)
        {
            _protocol.Write(packet);
        }
        public override Task WriteAsync(TMsg packet)
        {
            return _protocol.WriteAsync(packet);
        }

        private bool ServerCertValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            var ex = new ArgumentException($"Certificate error: {sslPolicyErrors}");
            throw ex;
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _protocol.Dispose();
        }
    }

    public class NetClient : NetClient<byte[]>
    {
        public NetClient(string hostname, int port) 
            : base(hostname, port, new ByteArrayTransferProtocol())
        {
        }

        public NetClient(string hostname, int port, bool ssl) 
            : base(hostname, port, ssl, new ByteArrayTransferProtocol())
        {
        }
    }

    internal class InboundNetClient<TMsg> : MessageTransferClient<TMsg>
    {
        public override IObservable<MessageRecievedEventArgs<TMsg>> MessageRecieved => _protocol.MessageRecieved;
        private ITransferProtocol<TMsg> _protocol;

        internal InboundNetClient(TcpClient client, Stream stream, ITransferProtocol<TMsg> protocol)
        {
            _client = client;
            _stream = stream;
            _protocol = protocol;
        }

        internal override void StartReading()
        {
            _protocol.BeginReadingAsync(_stream);
        }

        public override void Write(TMsg packet)
        {
            _protocol.Write(packet);
        }
        public override Task WriteAsync(TMsg packet)
        {
            return _protocol.WriteAsync(packet);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _protocol?.Dispose();
        }
    }
}
