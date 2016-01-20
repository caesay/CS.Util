using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Network
{
    public class AcceptClientCallback
    {
        public NetClient Client { get; }

        public void Accept()
        {
            Client.StartReading();
        }
        public void Discard()
        {
            Client.Close();
        }

        internal AcceptClientCallback(NetClient client)
        {
            Client = client;
        }
    }
}
