using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class PathExtensions
    {
        /// <summary>
        /// Determines whether this file is a symbolic link.
        /// </summary>
        /// <param name="it">the file in question.</param>
        /// <returns><code>true</code> if the file is, indeed, a symbolic link, <code>false</code> otherwise.</returns>
        public static bool IsSymbolicLink(this FileInfo it)
        {
            return SymbolicLink.GetTarget(it.FullName) != null;
        }

        /// <summary>
        /// Returns the full path to the target of this symbolic link.
        /// </summary>
        /// <param name="it">The symbolic link in question.</param>
        /// <returns>The path to the target of the symbolic link.</returns>
        /// <exception cref="System.ArgumentException">If the file in question is not a symbolic link.</exception>
        public static string GetSymbolicLinkTarget(this FileInfo it)
        {
            if (!it.IsSymbolicLink())
                throw new ArgumentException("File specified is not a symbolic link.");

            return SymbolicLink.GetTarget(it.FullName);
        }

        public static bool IsSymbolicLink(this DirectoryInfo di)
        {
            return SymbolicLink.GetTarget(di.FullName) != null;
        }

        public static string GetSymbolicLinkTarget(this DirectoryInfo it)
        {
            if (!it.IsSymbolicLink())
                throw new ArgumentException("Directory specified is not a symbolic link.");

            return SymbolicLink.GetTarget(it.FullName);
        }
    }
}
