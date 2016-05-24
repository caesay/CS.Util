using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Finds the index of all instances of the search string inside of the source string.
        /// </summary>
        /// <param name="source">The string to search.</param>
        /// <param name="search">The search text.</param>
        /// <returns>An enumerable containing a list of indexes</returns>
        public static IEnumerable<int> IndexOfAll(this string source, string search)
        {
            return IndexOfAll(source, search, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Finds the index of all instances of the search string inside of the source string.
        /// </summary>
        /// <param name="source">The string to search.</param>
        /// <param name="search">The search text.</param>
        /// <param name="comparison">The comparison type to use when searching for indexes</param>
        /// <returns>An enumerable containing a list of indexes</returns>
        public static IEnumerable<int> IndexOfAll(this string source, string search, StringComparison comparison)
        {
            if (source == null)
                yield break;
            if (String.IsNullOrEmpty(search))
                throw new ArgumentException("search string must not be empty", nameof(search));
            for (int i = 0; ;)
            {
                i = source.IndexOf(search, i, comparison);
                if (i < 0)
                    break;
                yield return i;
                i++;
            }
        }

        /// <summary>
        /// Compares the string against a given pattern.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="pattern">The pattern to match, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
        public static bool IsLike(this string str, string pattern)
        {
            return new Regex(
                "^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }
    }
}
