using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool IsInGAC(this AssemblyName assemblyFullName)
        {
            try
            {
                return Assembly.ReflectionOnlyLoad(assemblyFullName.FullName).GlobalAssemblyCache;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsInGAC(this Assembly assembly)
        {
            return assembly.GlobalAssemblyCache;
        }
    }
}
