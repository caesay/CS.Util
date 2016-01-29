using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Reactive
{
    public class PacketRecievedEventArgs : EventArgs
    {
        public byte[] Bytes { get; }

        public PacketRecievedEventArgs(byte[] bytes)
        {
            Bytes = bytes;
        }
    }
}
