using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace CS.Network
{
    public class NetServer
    {
        public EndPoint LocalEndpoint => _listener.LocalEndpoint;
        public bool Pending => _listener.Pending();
        public bool Active => _listener.Active;
        public bool SSL
        {
            get { return _ssl; }
            set
            {
                if(value && Certificate == null)
                    throw new InvalidOperationException("Certificate property must be non-null.");
                _ssl = value;
            }
        }
        public X509Certificate2 Certificate { get; set; }

        private readonly TcpListenerEx _listener;
        private bool _ssl;

        public NetServer(IPEndPoint localEP)
        {
            _listener = new TcpListenerEx(localEP);
            _listener.ExclusiveAddressUse = true;
        }
        public NetServer(IPAddress localaddr, int port) 
        {
            _listener = new TcpListenerEx(localaddr, port);
        }
        public NetServer(int port)
        {
            _listener = new TcpListenerEx(IPAddress.Any, port);
        }

        public void Start()
        {
            _listener.Start();
        }
        public void Start(int backLog)
        {
            _listener.Start(backLog);
        }
        public void Stop()
        {
            _listener.Stop();
        }

        public NetClient AcceptClient()
        {
            var tcp = _listener.AcceptTcpClient();
            Stream stream = tcp.GetStream();
            if (SSL)
            {
                var sslStream = new SslStream(stream, false);
                sslStream.AuthenticateAsServer(Certificate, false, SslProtocols.Tls12, true);
                stream = sslStream;
            }
            return new NetClient(tcp, stream);
        }
        public async Task<NetClient> AcceptClientAsync()
        {
            var tcp = await _listener.AcceptTcpClientAsync();
            Stream stream = tcp.GetStream();
            if (SSL)
            {
                var sslStream = new SslStream(stream, false);
                sslStream.AuthenticateAsServer(Certificate, false, SslProtocols.Tls12, true);
                stream = sslStream;
            }
            return new NetClient(tcp, stream);
        }

        private class TcpListenerEx : TcpListener
        {
            public TcpListenerEx(IPEndPoint localEP) : base(localEP)
            {
            }
            public TcpListenerEx(IPAddress localaddr, int port) : base(localaddr, port)
            {
            }

            public new bool Active => base.Active;
        }
    }
}
