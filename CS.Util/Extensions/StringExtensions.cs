using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<int> IndexOfAll(this string source, string search)
        {
            if (String.IsNullOrEmpty(search))
                throw new ArgumentException("search must not be empty", "search");
            for (int i = 0; ; i += search.Length)
            {
                i = source.IndexOf(search, i);
                if (i < 0)
                    break;
                yield return i;
            }
        }
    }
}
