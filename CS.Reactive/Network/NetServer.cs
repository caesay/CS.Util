using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CS.Reactive
{
    public class NetServer
    {
        public EndPoint LocalEndpoint => _listener.LocalEndpoint;
        public bool Pending => _listener.Pending();
        public bool Active => _listener.Active;
        public IObservable<ITracked<NetClient>> ClientConnected => _trackable;

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

        private ITrackableObservable<NetClient> _trackable;
        private Subject<NetClient> _subject; 
        private readonly TcpListenerEx _listener;
        private bool _ssl;
        private Task _acceptTask;
        private CancellationTokenSource _tokenSource;

        public NetServer(IPEndPoint localEP)
        {
            _listener = new TcpListenerEx(localEP);
            _listener.ExclusiveAddressUse = true;
            SetupObservable();
        }
        public NetServer(IPAddress localaddr, int port) 
        {
            _listener = new TcpListenerEx(localaddr, port);
            SetupObservable();
        }
        public NetServer(int port)
        {
            _listener = new TcpListenerEx(IPAddress.Any, port);
            SetupObservable();
        }

        public void Start()
        {
            Start((int)SocketOptionName.MaxConnections);
        }
        public void Start(int backLog)
        {
            if(_tokenSource?.IsCancellationRequested == false)
                _tokenSource.Cancel();

            _tokenSource = new CancellationTokenSource();
            _listener.Start(backLog);
            _acceptTask = AcceptLoop();
        }
        public void Stop()
        {
            if(_tokenSource?.IsCancellationRequested == false)
                _tokenSource.Cancel();

            _listener.Stop();

            var previous = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(null);
            _acceptTask.Wait(5000);
            SynchronizationContext.SetSynchronizationContext(previous);
        }

        private void SetupObservable()
        {
            _subject = new Subject<NetClient>();
            // if a subscriber observes the client, it will start reading from the stream
            _trackable = _subject.ToTrackableObservable((client) => client.StartReading());
            // otherwise it is unobserved and should be disposed.
            _trackable.Unobserved.Subscribe((next) => next.Dispose());
        }
        private async Task<NetClient> AcceptClientAsync()
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
        private async Task AcceptLoop()
        {
            while (!_tokenSource.IsCancellationRequested && _listener.Active)
            {
                try
                {
                    var client = await AcceptClientAsync();
                    _subject.OnNext(client);
                }
                catch (SocketException ex)
                {
                    // not really important.
                }
            }
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
