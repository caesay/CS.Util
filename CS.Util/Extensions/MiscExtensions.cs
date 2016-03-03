using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class MiscExtensions
    {
        public static Bitmap GetThumbnail(this FileInfo file, Size size,
            ThumbnailOptions options = ThumbnailOptions.ThumbnailOnly)
        {
            return WindowsThumbnailProvider.GetThumbnail(file.FullName, size.Width, size.Height, options);
        }

        public static IEnumerable<Enum> GetUniqueFlags(this Enum flags)
        {
            ulong flag = 1;
            foreach (var value in Enum.GetValues(flags.GetType()).Cast<Enum>())
            {
                ulong bits = Convert.ToUInt64(value);
                while (flag < bits)
                {
                    flag <<= 1;
                }

                if (flag == bits && flags.HasFlag(value))
                {
                    yield return value;
                }
            }
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
}
