using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CS.Util;

namespace CS.Util.Dynamic
{
    public static class DynamicTypeFactory
    {
        public delegate object CompiledMethodDelegate(object target, object[] args);

        private static readonly AssemblyBuilder asmBuilder;
        private static readonly ModuleBuilder modBuilder;
        static DynamicTypeFactory()
        {
            asmBuilder = Thread.GetDomain()
                .DefineDynamicAssembly(new AssemblyName("dynamic_type_factory"), AssemblyBuilderAccess.Run);
            modBuilder = asmBuilder.DefineDynamicModule("dynamic_type_factory_module");
        }
        public static Type Merge<T1, T2>()
        {
            return Merge(typeof(T1), typeof(T2));
        }
        public static Type Merge<T1, T2, T3>()
        {
            return Merge(typeof(T1), typeof(T2), typeof(T3));
        }
        public static Type Merge<T1, T2, T3, T4>()
        {
            return Merge(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }
        public static Type Merge<T1, T2, T3, T4, T5>()
        {
            return Merge(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }
        public static Type Merge(params Type[] types)
        {
            if (!types.All(t => t.IsInterface))
                throw new ArgumentException("One or more provided types are not an interface.");

            var name = $"dynMerge({RandomEx.GetString(4)})_" + String.Join("_", types.Select(t => t.Name));

            var typeBuilder = modBuilder.DefineType(
                name, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            foreach (Type t in types)
                typeBuilder.AddInterfaceImplementation(t);

            return typeBuilder.CreateType();
        }

        public static CompiledMethodDelegate Compile(MethodInfo method)
        {
            ParameterInfo[] parms = method.GetParameters();
            int numberOfParameters = parms.Length;
            Type[] args = { typeof(object), typeof(object[]) };
            DynamicMethod dynam = new DynamicMethod(String.Empty, typeof(object), args, typeof(CompiledMethodDelegate));
            ILGenerator il = dynam.GetILGenerator();
            Label argsOk = il.DefineLabel();
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Ldc_I4, numberOfParameters);
            il.Emit(OpCodes.Beq, argsOk);
            il.Emit(OpCodes.Newobj, typeof(TargetParameterCountException).GetConstructor(Type.EmptyTypes));
            il.Emit(OpCodes.Throw);
            il.MarkLabel(argsOk);
            if (!method.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            int i = 0;
            while (i < numberOfParameters)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);
                Type parmType = parms[i].ParameterType;
                if (parmType.IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, parmType);
                }
                i++;
            }
            il.Emit(method.IsFinal ? OpCodes.Call : OpCodes.Callvirt, method);
            if (method.ReturnType != typeof(void))
            {
                if (method.ReturnType.IsValueType)
                {
                    il.Emit(OpCodes.Box, method.ReturnType);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldnull);
            }
            il.Emit(OpCodes.Ret);
            return (CompiledMethodDelegate)dynam.CreateDelegate(typeof(CompiledMethodDelegate));
        }
    }
}
