using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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

        public static T GetAutoDeepClone<T>(this T obj)
        {
            return (T) DeepCopyInternal(obj);
        }
        private static object DeepCopyInternal(object obj)
        {
            if (obj == null)
                return null;
            Type type = obj.GetType();
            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }
            if (type.IsArray)
            {
                Type elementType = Type.GetType(
                     type.FullName.Replace("[]", string.Empty));
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(DeepCopyInternal(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }
            if (type.IsClass)
            {
                var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, Type.EmptyTypes, null);
                object copied = constructor == null 
                    ? FormatterServices.GetUninitializedObject(type) 
                    : constructor.Invoke(new object[0]);

                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;
                    field.SetValue(copied, DeepCopyInternal(fieldValue));
                }
                return copied;
            }
            throw new ArgumentException("Unknown type");
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
