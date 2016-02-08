using System;
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
            List<int> hashCodes = new List<int>();
            if (selector.HasFlag(MemberSelector.PublicProperties))
                hashCodes.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.GetValue(obj).GetHashCode()));
            if (selector.HasFlag(MemberSelector.PrivateProperties))
                hashCodes.AddRange(type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance).Select(p => p.GetValue(obj).GetHashCode()));
            if (selector.HasFlag(MemberSelector.PublicFields))
                hashCodes.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(p => p.GetValue(obj).GetHashCode()));
            if (selector.HasFlag(MemberSelector.PrivateFields))
                hashCodes.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(p => p.GetValue(obj).GetHashCode()));

            if (hashCodes.Count == 0)
                return obj.GetHashCode();
            if (hashCodes.Count == 1)
                return hashCodes[0];

            return GetAutoHashCode(null, hashCodes.ToArray());
        }
        public static int GetAutoHashCode(this object obj, params object[] members)
        {
            return GetAutoHashCode(obj, members.Select(o => o.GetHashCode()).ToArray());
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

        /// <summary>
        /// Provides a method to use a SecureString in such a way that guarentees the string memory won't be leaked or
        /// remain in managed memory after the action is completed.
        /// </summary>
        /// <param name="secureString">The secure string to load</param>
        /// <param name="action">The action to invoke with the plain-text string</param>
        public static void UseSecurely(this SecureString secureString, Action<string> action)
        {
            secureString.UseSecurely<object>((p) =>
            {
                action(p);
                return null;
            });
        }
        /// <summary>
        /// Provides a method to use a SecureString in such a way that guarentees the string memory won't be leaked or
        /// remain in managed memory after the action is completed.
        /// </summary>
        /// <param name="secureString">The secure string to load</param>
        /// <param name="action">The function to invoke with the plain-text string</param>
        public static unsafe T UseSecurely<T>(this SecureString secureString, Func<string, T> action)
        {
            T result = default(T);
            int length = secureString.Length;
            var insecurePassword = new string('\0', length);

            var gch = new GCHandle();
            RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(
                delegate
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        gch = GCHandle.Alloc(insecurePassword, GCHandleType.Pinned);
                    }

                    // copy the secure password bits over to the pinned insecurePassword
                    IntPtr passwordPtr = IntPtr.Zero;
                    RuntimeHelpers.ExecuteCodeWithGuaranteedCleanup(
                        delegate
                        {
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                            }
                            finally
                            {
                                passwordPtr = Marshal.SecureStringToBSTR(secureString);
                            }

                            var pPassword = (char*)passwordPtr;
                            var pInsecurePassword = (char*)gch.AddrOfPinnedObject();
                            for (int index = 0; index < length; index++)
                            {
                                pInsecurePassword[index] = pPassword[index];
                            }
                        },
                        delegate
                        {
                            if (passwordPtr != IntPtr.Zero)
                            {
                                // zero the unmanaged string
                                Marshal.ZeroFreeBSTR(passwordPtr);
                            }
                        }, null);

                    // execute specified action
                    result = action(insecurePassword);
                },
                delegate
                {
                    if (gch.IsAllocated)
                    {
                        // zero the managed string
                        var pInsecurePassword = (char*)gch.AddrOfPinnedObject();
                        for (int index = 0; index < length; index++)
                        {
                            pInsecurePassword[index] = '\0';
                        }
                        gch.Free();
                    }
                }, null);
            return result;
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
