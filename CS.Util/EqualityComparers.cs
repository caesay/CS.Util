using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util
{
    public class AssemblyFullNameEqualityComparer : IEqualityComparer<Assembly>
    {
        public bool Equals(Assembly x, Assembly y)
        {
            if (x.FullName == y.FullName) return true;
            if (x.Location == y.Location) return true;
            return false;
        }

        public int GetHashCode(Assembly obj)
        {
            return obj.FullName.GetHashCode();
        }
    }
   
    public class DynamicComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public DynamicComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            _equals = @equals;
            _getHashCode = getHashCode;
        }

        public bool Equals(T x, T y)
        {
            return _equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _getHashCode(obj);
        }

        public static DynamicComparer<T> From(Func<T, T, bool> equals)
        {
            return new DynamicComparer<T>(equals, EqualityComparer<T>.Default.GetHashCode);
        }
        public static DynamicComparer<T> From(Func<T, int> getHashCode)
        {
            return new DynamicComparer<T>(EqualityComparer<T>.Default.Equals, getHashCode);
        }
        public static DynamicComparer<T> From(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            return new DynamicComparer<T>(equals, getHashCode);
        }
    }
}
