using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Reactive.Network
{
    public interface ITransferProtocol<TMsg> : IDisposable
    {
        IObservable<MessageRecievedEventArgs<TMsg>> MessageRecieved { get; }
        void BeginReadingAsync(Stream netStream);
        Task WriteAsync(TMsg msg);
        void Write(TMsg msg);
    }
}
