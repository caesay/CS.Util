using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Generate an automatic hash code for an object based on members of that object.
        /// </summary>
        /// <param name="obj">The object to search.</param>
        /// <param name="selector">Choose what object members to use in hash code generation</param>
        /// <returns>A hash code</returns>
        public static int GetAutoHashCode(this object obj, MemberSelector selector = MemberSelector.PublicProperties)
        {
            var type = obj.GetType();
            List<object> hashCodes = new List<object>();
            if (selector.HasFlag(MemberSelector.PublicProperties))
                hashCodes.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.GetValue(obj)));
            if (selector.HasFlag(MemberSelector.PrivateProperties))
                hashCodes.AddRange(type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Select(p => p.GetValue(obj)));
            if (selector.HasFlag(MemberSelector.PublicFields))
                hashCodes.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(p => p.GetValue(obj)));
            if (selector.HasFlag(MemberSelector.PrivateFields))
                hashCodes.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(p => p.GetValue(obj)));

            return GetAutoHashCode(null, hashCodes.ToArray());
        }
        public static int GetAutoHashCode(this object obj, params object[] members)
        {
            return GetAutoHashCode(obj, members.Where(m => m != null).Select(o => o.GetHashCode()).ToArray());
        }
        public static int GetAutoHashCode(this object obj, params int[] members)
        {
            // this is pretty much randomly selected, some people on SOF said that they were good numbers :)
            const int b = 378551;
            int a = 63689;
            int hash = members.Count() + 1;

            unchecked
            {
                foreach (var code in members)
                {
                    hash = hash * a + code;
                    a = a * b;
                }
            }
            return hash;
        }

        /// <summary> Returns true if this value is equal to the default value for this type.</summary>
        public static bool IsDefault<T>(this T val)
        {
            return val.Equals(default(T));
        }
    }

    [Flags]
    public enum MemberSelector : short
    {
        PublicProperties = 1,
        PrivateProperties = 2,
        PublicFields = 4,
        PrivateFields = 8,
        PublicPropertiesAndFields = PublicProperties | PublicFields,
        PrivatePropertiesAndFields = PrivateProperties | PrivateFields,
        AllProperties = PublicProperties | PrivateProperties,
        AllFields = PublicFields | PrivateFields,
        AllPropertiesAndFields = PublicPropertiesAndFields | PrivatePropertiesAndFields,
    }
}
