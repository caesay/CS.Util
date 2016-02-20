using System;
using System.Net;
using System.Threading.Tasks;

namespace CS.Reactive.Network
{
    public interface IMessageTransferClient<TMsg>
    {
        bool Connected { get; }
        IObservable<MessageRecievedEventArgs<TMsg>> MessageRecieved { get; }
        EndPoint RemoteEndpoint { get; }

        void Dispose();
        void Write(TMsg packet);
        Task WriteAsync(TMsg packet);
    }
}