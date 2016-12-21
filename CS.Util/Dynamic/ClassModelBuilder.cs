using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Dynamic
{
    public class ClassModelBuilder
    {
        private List<PropertyDeclaration> _properties = new List<PropertyDeclaration>();
        private List<MethodDeclaration> _constructors = new List<MethodDeclaration>();
        private List<MethodDeclaration> _methods = new List<MethodDeclaration>();
        private List<Type> _interfaces = new List<Type>();
        private Dictionary<string, Type> _fields = new Dictionary<string, Type>();
        private ModuleBuilder _modBuilder;
        public ClassModelBuilder(ModuleBuilder modBuilder)
        {
            _modBuilder = modBuilder;
        }

        public ClassModelBuilder Interface<T>()
        {
            return Interface(typeof(T));
        }

        public ClassModelBuilder Interface(Type interfaceType)
        {
            _interfaces.Add(interfaceType);
            return this;
        }

        public ClassModelBuilder Field<T>(string name)
        {
            return Field(name, typeof(T));
        }

        public ClassModelBuilder Field(string name, Type type)
        {
            _fields[name] = type;
            return this;
        }

        public ClassModelBuilder Constructor()
        {
            return Constructor((ctx) => { });
        }

        public ClassModelBuilder Constructor(Action<DynamicClassContext> body)
        {
            return Constructor(Type.EmptyTypes, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Constructor<T1>(Action<DynamicClassContext, T1> body)
        {
            return Constructor(new[] { typeof(T1) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Constructor<T1, T2>(Action<DynamicClassContext, T1, T2> body)
        {
            return Constructor(new[] { typeof(T1), typeof(T2) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Constructor<T1, T2, T3>(Action<DynamicClassContext, T1, T2, T3> body)
        {
            return Constructor(new[] { typeof(T1), typeof(T2), typeof(T3) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Constructor<T1, T2, T3, T4>(Action<DynamicClassContext, T1, T2, T3, T4> body)
        {
            return Constructor(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Constructor<T1, T2, T3, T4, T5>(Action<DynamicClassContext, T1, T2, T3, T4, T5> body)
        {
            return Constructor(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Constructor<T1, T2, T3, T4, T5, T6>(Action<DynamicClassContext, T1, T2, T3, T4, T5, T6> body)
        {
            return Constructor(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        private ClassModelBuilder Constructor(Type[] parameters, Type returnType, MethodBodyReader body)
        {
            _constructors.Add(new MethodDeclaration() { ParameterTypes = parameters, ReturnType = returnType, Body = body });
            return this;
        }

        public ClassModelBuilder Method<TReturn>(string name, Func<DynamicClassContext, TReturn> body)
        {
            return Method(name, Type.EmptyTypes, typeof(TReturn), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, TReturn>(string name, Func<DynamicClassContext, T1, TReturn> body)
        {
            return Method(name, new[] { typeof(T1) }, typeof(TReturn), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, TReturn>(string name, Func<DynamicClassContext, T1, T2, TReturn> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2) }, typeof(TReturn), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, T3, TReturn>(string name, Func<DynamicClassContext, T1, T2, T3, TReturn> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2), typeof(T3) }, typeof(TReturn), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, T3, T4, TReturn>(string name, Func<DynamicClassContext, T1, T2, T3, T4, TReturn> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, typeof(TReturn), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, T3, T4, T5, TReturn>(string name, Func<DynamicClassContext, T1, T2, T3, T4, T5, TReturn> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, typeof(TReturn), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, T3, T4, T5, T6, TReturn>(string name, Func<DynamicClassContext, T1, T2, T3, T4, T5, T6, TReturn> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, typeof(TReturn), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method(string name, Action<DynamicClassContext> body)
        {
            return Method(name, Type.EmptyTypes, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1>(string name, Action<DynamicClassContext, T1> body)
        {
            return Method(name, new[] { typeof(T1) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2>(string name, Action<DynamicClassContext, T1, T2> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, T3>(string name, Action<DynamicClassContext, T1, T2, T3> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2), typeof(T3) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, T3, T4>(string name, Action<DynamicClassContext, T1, T2, T3, T4> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, T3, T4, T5>(string name, Action<DynamicClassContext, T1, T2, T3, T4, T5> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        public ClassModelBuilder Method<T1, T2, T3, T4, T5, T6>(string name, Action<DynamicClassContext, T1, T2, T3, T4, T5, T6> body)
        {
            return Method(name, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, typeof(void), MethodBodyReader.Read(body.Method));
        }

        private ClassModelBuilder Method(string name, Type[] parameters, Type returnType, MethodBodyReader body)
        {
            _methods.Add(new MethodDeclaration() { Name = name, ParameterTypes = parameters, ReturnType = returnType, Body = body });
            return this;
        }

        public ClassModelBuilder Property<T>(string name)
        {
            return Property<T>(name, null);
        }

        public ClassModelBuilder Property<T>(string name, Func<DynamicClassContext, T> getValue)
        {
            return Property<T>(name, getValue, null);
        }

        public ClassModelBuilder Property<T>(string name, Func<DynamicClassContext, T> getValue, Action<DynamicClassContext, T> setValue)
        {
            var get = getValue == null ? null : MethodBodyReader.Read(getValue.Method);
            var set = setValue == null ? null : MethodBodyReader.Read(setValue.Method);

            _properties.Add(new PropertyDeclaration { Name = name, Type = typeof(T), GetBody = get, SetBody = set });
            return this;
        }

        public ClassModelBuilder Property(string name, Type type)
        {
            return Property(name, type, null, null);
        }

        public ClassModelBuilder Property(string name, Type type, Func<DynamicClassContext, object> getValue)
        {
            return Property(name, type, getValue, null);
        }

        public ClassModelBuilder Property(string name, Type type, Func<DynamicClassContext, object> getValue, Action<DynamicClassContext, object> setValue)
        {
            var get = getValue == null ? null : MethodBodyReader.Read(getValue.Method);
            var set = setValue == null ? null : MethodBodyReader.Read(setValue.Method);

            _properties.Add(new PropertyDeclaration { Name = name, Type = type, GetBody = get, SetBody = set });
            return this;
        }

        public Type Build()
        {
            var builder = _modBuilder.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Public | TypeAttributes.Class);

            var fields = _fields.ToDictionary(kvp => kvp.Key, kvp => builder.DefineField(kvp.Key, kvp.Value, FieldAttributes.Private));

            var methods = _methods.ToDictionary(kvp => kvp.Name,
                kvp => new InterimMethod
                {
                    Builder = builder.DefineMethod(kvp.Name, MethodAttributes.Public | MethodAttributes.Virtual, kvp.ReturnType, kvp.ParameterTypes),
                    Body = kvp.Body,
                    AutoProp = false
                });

            MethodAttributes propAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            foreach (var prop in _properties)
            {
                bool auto = false, read = prop.GetBody != null, write = prop.SetBody != null;
                if (!read && !write)
                    read = write = auto = true;

                if (read)
                {
                    var name = "get_" + prop.Name;
                    var get = builder.DefineMethod(name, propAttr, prop.Type, Type.EmptyTypes);
                    methods.Add(name, new InterimMethod { Builder = get, Body = prop.GetBody, AutoProp = auto });
                }
                if (write)
                {
                    var name = "set_" + prop.Name;
                    var set = builder.DefineMethod(name, propAttr, typeof(void), new Type[] { prop.Type });
                    methods.Add(name, new InterimMethod { Builder = set, Body = prop.SetBody, AutoProp = auto });
                }
            }

            foreach (var iface in _interfaces)
                builder.AddInterfaceImplementation(iface);

            foreach (var cons in _constructors)
                BuildMethod(builder, builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, cons.ParameterTypes).GetILGenerator(), cons.Body, fields, methods, ".ctor");

            foreach (var meth in methods.Where(m => !m.Value.AutoProp))
                BuildMethod(builder, meth.Value.Builder.GetILGenerator(), meth.Value.Body, fields, methods, meth.Key);

            foreach (var prop in _properties)
                BuildProperty(builder, prop, methods);

            return builder.CreateType();
        }

        private void BuildProperty(TypeBuilder builder, PropertyDeclaration prop, Dictionary<string, InterimMethod> methods)
        {
            var propBuilder = builder.DefineProperty(prop.Name, PropertyAttributes.None, prop.Type, Type.EmptyTypes);

            InterimMethod get, set;
            methods.TryGetValue("get_" + prop.Name, out get);
            methods.TryGetValue("set_" + prop.Name, out set);

            FieldBuilder field = null;
            if (get?.AutoProp == true || set?.AutoProp == true)
                field = builder.DefineField("m_" + prop.Name, prop.Type, FieldAttributes.Private);

            if (get?.AutoProp == true)
            {
                ILGenerator getIL = get.Builder.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, field);
                getIL.Emit(OpCodes.Ret);
            }

            if (set?.AutoProp == true)
            {
                ILGenerator setIL = set.Builder.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, field);
                setIL.Emit(OpCodes.Ret);
            }

            if (set != null)
                propBuilder.SetSetMethod(set.Builder);

            if (get != null)
                propBuilder.SetGetMethod(get.Builder);
        }

        private void BuildMethod(TypeBuilder builder, ILGenerator gen, MethodBodyReader reader, Dictionary<string, FieldBuilder> fields, Dictionary<string, InterimMethod> methods, string memberName)
        {
            var writer = new MethodBodyBuilder(gen);

            var locals = reader.Locals.Select(lc => lc.LocalType);
            writer.DeclareLocals(locals);
            writer.DeclareExceptionHandlers(reader.ExceptionHandlers);

            // transforms all ldarg.x to ldarg.x-1 and output the index of the new ldarg.0's (context calls)
            int[] contexts;
            var body = TransformLdArgOpcodes(reader.Instructions, out contexts);

            // starts at the innermost context call and walks back to the beginning replacing them.
            foreach (var startIndex in contexts.OrderByDescending(x => x))
            {
                var endIndex = startIndex;
                while (endIndex < body.Length)
                {
                    endIndex++;
                    var endOp = body[endIndex];
                    if (endOp.OpCode == OpCodes.Callvirt && DynamicClassContext.Methods.Contains(endOp.Operand as MethodInfo))
                        break;
                }

                endIndex++;
                var ins = body.Skip(startIndex).Take(endIndex - startIndex).ToArray();
                ins = ProcessLdContextInstruction(ins, fields, methods, memberName);
                body = body.Take(startIndex).Concat(ins).Concat(body.Skip(endIndex)).ToArray();
            }

            writer.EmitBody(body);
        }

        private Instruction[] TransformLdArgOpcodes(Instruction[] oldBody, out int[] contextIndexs)
        {
            List<int> context = new List<int>();
            List<Instruction> newBody = new List<Instruction>();
            for (int index = 0; index < oldBody.Length; index++)
            {
                var ins = oldBody[index];
                var name = ins.OpCode.Name;

                // ignore everything but ldarg (load method parameter)
                if (!name.StartsWith("ldarg"))
                {
                    newBody.Add(ins);
                    continue;
                }

                // find the index of the argument currently being accessed
                int loc;
                int seperatorIndex = name.IndexOf('.');
                if (seperatorIndex > 0)
                {
                    var tmp = name.Substring(seperatorIndex + 1);
                    if (!int.TryParse(tmp, out loc))
                        throw new NotSupportedException("Unexpected Opcode: " + name);
                }
                else if (ins.Operand is int)
                {
                    loc = (int)ins.Operand;
                }
                else
                {
                    throw new NotSupportedException("Unexpected Opcode: " + name);
                }

                if (loc == 0) // accessing 'this'
                    throw new NotSupportedException("Cannot access 'this' in dynamic method body. This can happen if you try to use a local clousure variable.");

                if (loc == 1) // accessing context
                    context.Add(index);

                // subtract everything by 1 (to remove context)
                ins.OpCode = OpCodes.Ldarg;
                ins.Operand = loc - 1;
                newBody.Add(ins);
            }

            contextIndexs = context.ToArray();
            return newBody.ToArray();
        }

        private Instruction[] ProcessLdContextInstruction(Instruction[] body, Dictionary<string, FieldBuilder> fields, Dictionary<string, InterimMethod> methods, string memberName)
        {
            var call = body.Last().Operand as MethodInfo;
            if (call == null)
                throw new NotSupportedException("This error shouldn't happen.");

            body = body.Skip(1).Take(body.Length - 2).ToArray();

            if (call == DynamicClassContext.info_getField)
            {
                var fld = fields[(string)body[0].Operand];
                return new Instruction[]
                {
                    new Instruction(-1, OpCodes.Ldarg_0),
                    new Instruction(-1, OpCodes.Ldfld) { Operand = fld }
                };
            }
            else if (call == DynamicClassContext.info_setField)
            {
                var fld = fields[(string)body[0].Operand];

                List<Instruction> ret = new List<Instruction>();
                ret.Add(new Instruction(-1, OpCodes.Ldarg_0));
                ret.AddRange(body.Skip(1));
                ret.Add(new Instruction(-1, OpCodes.Stfld) { Operand = fld });

                return ret.ToArray();
            }
            else if (call == DynamicClassContext.info_getProperty)
            {
                var prp = methods["get_" + (string)body[0].Operand];
                return new Instruction[]
                {
                    new Instruction(-1, OpCodes.Ldarg_0),
                    new Instruction(-1, OpCodes.Callvirt) { Operand = prp.Builder }
                };
            }
            else if (call == DynamicClassContext.info_setProperty)
            {
                var prp = methods["set_" + (string)body[0].Operand];
                List<Instruction> ret = new List<Instruction>();
                ret.Add(new Instruction(-1, OpCodes.Ldarg_0));
                ret.AddRange(body.Skip(1));
                ret.Add(new Instruction(-1, OpCodes.Callvirt) { Operand = prp.Builder });

                return ret.ToArray();
            }
            else if (call == DynamicClassContext.info_getMyName)
            {
                return new Instruction[]
                {
                    new Instruction(-1, OpCodes.Ldstr) { Operand = memberName },
                };
            }
            else
            {
                throw new NotSupportedException($"Unsupported context method: {call.Name}.");
            }
        }

        private class PropertyDeclaration
        {
            public string Name;
            public Type Type;
            public MethodBodyReader GetBody;
            public MethodBodyReader SetBody;
        }

        private class MethodDeclaration
        {
            public string Name;
            public Type ReturnType;
            public Type[] ParameterTypes;
            public MethodBodyReader Body;
        }

        private class InterimMethod
        {
            public MethodBuilder Builder;
            public MethodBodyReader Body;
            public bool AutoProp;
        }

        public sealed class DynamicClassContext
        {
            internal DynamicClassContext()
            {
            }

            internal static MethodInfo[] Methods = new[] { info_getField, info_setField, info_getProperty, info_setProperty, info_getMyName };

            // these methods get re-written in IL
            internal static MethodInfo info_getField =>
                typeof(DynamicClassContext).GetMethod(nameof(Field), new Type[] { typeof(string) });
            public object Field(string name) { return null; }

            internal static MethodInfo info_setField =>
                typeof(DynamicClassContext).GetMethod(nameof(Field), new Type[] { typeof(string), typeof(object) });
            public void Field(string name, object value) { }


            internal static MethodInfo info_setProperty =>
                typeof(DynamicClassContext).GetMethod(nameof(Property), new Type[] { typeof(string), typeof(object) });
            public void Property(string name, object value) { }

            internal static MethodInfo info_getProperty =>
                typeof(DynamicClassContext).GetMethod(nameof(Property), new Type[] { typeof(string) });
            public object Property(string name) { return null; }

            internal static MethodInfo info_getMyName =>
                typeof(DynamicClassContext).GetMethod(nameof(MyName), new Type[] { });
            public string MyName() { return null; }
        }
    }

}
