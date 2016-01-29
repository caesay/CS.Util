using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CS.Util.Extensions;

namespace CS.Util.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var t1 = PrettyTime.Format(DateTime.Now.AddDays(-2));
            var t2 = PrettyTime.Format(DateTime.Now.AddSeconds(-5));
            var t3 = PrettyTime.Format(DateTime.Now.AddMinutes(-5));
            var t4 = PrettyTime.Format(DateTime.Now.AddHours(-5));

            var t5 = long.MaxValue.AsPrettyString();
        }
    }
}
