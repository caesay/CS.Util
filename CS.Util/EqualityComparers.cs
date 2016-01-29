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

    public class CustomEqualityComparer
    {
        public static CustomEqualityComparer<T> From<T>(Func<T, T, bool> equals)
        {
            return new CustomEqualityComparer<T>(equals, EqualityComparer<T>.Default.GetHashCode);
        }
        public static CustomEqualityComparer<T> From<T>(Func<T, int> getHashCode)
        {
            return new CustomEqualityComparer<T>(EqualityComparer<T>.Default.Equals, getHashCode);
        }
        public static CustomEqualityComparer<T> From<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            return new CustomEqualityComparer<T>(equals, getHashCode);
        }
    }
    public class CustomEqualityComparer<T> : CustomEqualityComparer, IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        public CustomEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
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
    }
}
