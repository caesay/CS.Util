using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CS.Util;

namespace CS.Util.Dynamic
{
    public delegate object CompiledMethodDelegate(object target, object[] args);
    public delegate object ImplementedMethodDelegate(MethodInfo method, object[] args);

    public static class DynamicTypeFactory
    {
        private static readonly AssemblyBuilder asmBuilder;
        private static readonly ModuleBuilder modBuilder;
        private const int ranLength = 6;

        static DynamicTypeFactory()
        {
            asmBuilder = Thread.GetDomain()
                .DefineDynamicAssembly(new AssemblyName("dynamic_type_factory"), AssemblyBuilderAccess.Run);
            modBuilder = asmBuilder.DefineDynamicModule("dynamic_type_factory");
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

        public static Type Merge<T1, T2, T3, T4, T5, T6>()
        {
            return Merge(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        }

        public static Type Merge(params Type[] types)
        {
            if (!types.All(t => t.IsInterface))
                throw new ArgumentException("One or more provided types are not an interface.");

            var name = $"dynMerge({RandomEx.GetString(ranLength)})_" + String.Join("_", types.Select(t => t.Name));

            var typeBuilder = modBuilder.DefineType(
                name, TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract);

            foreach (Type t in types)
                typeBuilder.AddInterfaceImplementation(t);

            return typeBuilder.CreateType();
        }

        public static object Implement(Type target, ImplementedMethodDelegate implementer)
        {
            if (!target.IsInterface)
                throw new ArgumentException("target must be an interface type.");

            var name = $"dynImplement({RandomEx.GetString(ranLength)})_" + target.Name;
            var typeBuilder = modBuilder.DefineType(name, TypeAttributes.Public | TypeAttributes.Class);
            typeBuilder.AddInterfaceImplementation(target);

            var implementerField = typeBuilder.DefineField("_implementer", typeof(ImplementedMethodDelegate), FieldAttributes.Private);

            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.Standard | CallingConventions.HasThis,
                new Type[] { typeof(ImplementedMethodDelegate) });

            // emit constructor
            var constructorIl = constructorBuilder.GetILGenerator();
            constructorIl.Emit(OpCodes.Ldarg_0);
            constructorIl.Emit(OpCodes.Ldarg_1);
            constructorIl.Emit(OpCodes.Stfld, implementerField);
            constructorIl.Emit(OpCodes.Ret);

            // implement all interface methods.
            var methods = target.GetMethods().Concat(target.GetInterfaces().SelectMany(inter => inter.GetMethods()));
            foreach (var method in methods)
            {
                var methodParamaters = method.GetParameters().Select(p => p.ParameterType).ToArray();
                var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Virtual, method.CallingConvention,
                    method.ReturnType, methodParamaters);

                var methodIl = methodBuilder.GetILGenerator();
                var objLocalIndex = methodIl.DeclareLocal(typeof(object[])).LocalIndex;

                // create new object[] to hold paramaters and store it to local 
                methodIl.Emit(OpCodes.Ldc_I4, methodParamaters.Length);
                methodIl.Emit(OpCodes.Newarr, typeof(object));
                methodIl.Emit(OpCodes.Stloc, objLocalIndex);

                // fill object array with method parameters
                for (int i = 0; i < methodParamaters.Length; i++)
                {
                    methodIl.Emit(OpCodes.Ldloc, objLocalIndex);
                    methodIl.Emit(OpCodes.Ldc_I4, i);
                    methodIl.Emit(OpCodes.Ldarg, i + 1);
                    if (methodParamaters[i].IsValueType)
                        methodIl.Emit(OpCodes.Box, methodParamaters[i]);
                    methodIl.Emit(OpCodes.Stelem_Ref);
                }

                // load current delegate, MethodInfo and object array to the stack
                methodIl.Emit(OpCodes.Ldarg_0);
                methodIl.Emit(OpCodes.Ldfld, implementerField);
                methodIl.Emit(OpCodes.Ldtoken, method);
                methodIl.Emit(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle) }));
                methodIl.Emit(OpCodes.Castclass, typeof(MethodInfo));
                methodIl.Emit(OpCodes.Ldloc, objLocalIndex);

                // call it.
                methodIl.Emit(method.IsFinal ? OpCodes.Call : OpCodes.Callvirt, typeof(ImplementedMethodDelegate).GetMethod("Invoke"));

                if (method.ReturnType == typeof(void))
                {
                    methodIl.Emit(OpCodes.Pop);
                }
                else if (method.ReturnType.IsValueType)
                    methodIl.Emit(OpCodes.Unbox_Any, method.ReturnType);

                methodIl.Emit(OpCodes.Ret);
            }

            var createdType = typeBuilder.CreateType();
            var constructor = createdType.GetConstructor(new Type[] { typeof(ImplementedMethodDelegate) });
            return constructor.Invoke(new object[] { implementer });
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

        //private static bool CheckObjectContainsMethod(MethodInfo m, object obj)
        //{
        //    var whereMethods = obj.GetType()
        //        .GetMethods()
        //        .Where(method => method.Name.Equals(m.Name))
        //        .Where(method => CheckTypesEqual(method.ReturnType, m.ReturnType))
        //        .Where(method =>
        //        {
        //            var p1 = method.GetParameters();
        //            var p2 = m.GetParameters();
        //            if (p1.Length != p2.Length)
        //                return false;
        //            for (int i = 0; i < p1.Length; i++)
        //            {
        //                if (!CheckTypesEqual(p1[i].ParameterType, p2[i].ParameterType))
        //                    return false;
        //            }
        //            return true;
        //        });
        //    return whereMethods.SingleOrDefault() != null;
        //}

        //private static bool CheckTypesEqual(Type t1, Type t2)
        //{
        //    if (t1.IsGenericParameter || t2.IsGenericParameter)
        //        return true;
        //    if (t1.IsGenericType && t2.IsGenericType)
        //    {
        //        if (t1.GetGenericTypeDefinition() != t2.GetGenericTypeDefinition())
        //            return false;
        //        var t1args = t1.GetGenericArguments();
        //        var t2args = t2.GetGenericArguments();
        //        if (t1args.Length != t2args.Length)
        //            return false;
        //        for (int i = 0; i < t1args.Length; i++)
        //        {
        //            if (!CheckTypesEqual(t1args[i], t2args[i]))
        //                return false;
        //        }
        //        return true;
        //    }
        //    return t1 == t2;
        //}
    }

    //public abstract class TypeProxy : ISerializable
    //{
    //    public abstract void InvokeEvent(EventAction action, EventInfo eventInfo);
    //    public abstract object InvokeMethod(MethodInfo methodInfo, object[] args);

    //    protected Widget(SerializationInfo info, StreamingContext context)
    //    {
    //        // Perform your deserialization here...
    //        this.SerialNumber = (string)info.GetValue("SerialNumber", typeof(string));
    //    }

    //    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public enum EventAction
    //{
    //    Add,
    //    Remove
    //}
}
