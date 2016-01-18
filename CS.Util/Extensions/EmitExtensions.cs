using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Extensions
{
    public static class EmitExtensions
    {
        public static void EmitWriteLine(this ILGenerator gen, object obj)
        {
            EmitWriteLine(gen, obj.ToString());
        }
        public static void EmitWriteLine(this ILGenerator gen, string message)
        {
            gen.Emit(OpCodes.Ldstr, message);
            gen.Emit(OpCodes.Call,
               (typeof(Console)).GetMethod("WriteLine", BindingFlags.Static | BindingFlags.Public, null,
                   new[] { typeof(string) }, null));
        }
        public static void EmitType(this ILGenerator gen, Type t, bool assemblyQualified = true)
        {
            if (assemblyQualified)
            {
                gen.Emit(OpCodes.Ldstr, t.AssemblyQualifiedName);
                gen.Emit(OpCodes.Call,
                    (typeof(Type)).GetMethod("GetType", BindingFlags.Static | BindingFlags.Public, null,
                        new[] { typeof(string) }, null));
            }
            else
            {
                gen.Emit(OpCodes.Ldtoken, t);
                gen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) }));
            }
        }
    }
}
