using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Reactive.Network
{
    public class MessageRecievedEventArgs<TMsg> : EventArgs
    {
        public TMsg Message { get; }

        public MessageRecievedEventArgs(TMsg message)
        {
            Message = message;
        }
    }
}
