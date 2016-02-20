using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CS.Reactive.Network
{
    public abstract class MessageTransferClient<TMsg> : IDisposable, IMessageTransferClient<TMsg>
    {
        public bool Connected => _client?.Connected == true;
        public EndPoint RemoteEndpoint => _client?.Client.RemoteEndPoint;
        public abstract IObservable<MessageRecievedEventArgs<TMsg>> MessageRecieved { get; }

        protected Stream _stream;
        protected TcpClient _client;
        private bool _disposed;

        protected MessageTransferClient()
        {

        }
        ~MessageTransferClient()
        {
            Dispose(false);
        }

        internal abstract void StartReading();
        public abstract void Write(TMsg packet);
        public abstract Task WriteAsync(TMsg packet);

        protected virtual void Dispose(bool disposing)
        {
            _client?.Close();
            _stream?.Dispose();
            _client = null;
            _stream = null;
        }
        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
