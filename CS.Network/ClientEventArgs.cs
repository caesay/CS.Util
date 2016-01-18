using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Network
{
    public class ClientEventArgs<T>
    {
        public T Value { get; }
        public ClientEventArgs(T value)
        {
            Value = value;
        }
    }
}
