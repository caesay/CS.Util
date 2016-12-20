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

        public ClassModelBuilder Constructor(string name, Type[] parameters, Action<DynamicClassContext, object[]> methodBody)
        {

            return Constructor(name, parameters, typeof(void), MethodBodyReader.Read(methodBody.Method));
        }

        public ClassModelBuilder Constructor(string name, Type[] parameters, Type returnType, Func<DynamicClassContext, object[], object> methodBody)
        {
            return Constructor(name, parameters, returnType, MethodBodyReader.Read(methodBody.Method));
        }

        private ClassModelBuilder Constructor(string name, Type[] parameters, Type returnType, MethodBodyReader body)
        {
            _constructors.Add(new MethodDeclaration() { Name = name, ParameterTypes = parameters, ReturnType = returnType, Body = body });
            return this;
        }

        public ClassModelBuilder Method(string name, Type[] parameters, Action<DynamicClassContext, object[]> methodBody)
        {

            return Method(name, parameters, typeof(void), MethodBodyReader.Read(methodBody.Method));
        }

        public ClassModelBuilder Method(string name, Type[] parameters, Type returnType, Func<DynamicClassContext, object[], object> methodBody)
        {
            return Method(name, parameters, returnType, MethodBodyReader.Read(methodBody.Method));
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

            foreach (var iface in _interfaces)
                builder.AddInterfaceImplementation(iface);

            foreach (var prop in _properties)
                BuildProperty(builder, prop, fields);

            return builder.CreateType();
        }

        private void BuildProperty(TypeBuilder builder, PropertyDeclaration prop, Dictionary<string, FieldBuilder> fields)
        {
            var propBuilder = builder.DefineProperty(prop.Name, PropertyAttributes.None, prop.Type, Type.EmptyTypes);

            MethodBuilder get = null;
            MethodBuilder set = null;

            MethodAttributes attr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            if (prop.GetBody == null && prop.SetBody == null)
            {
                FieldBuilder field = builder.DefineField("m" + prop.Name, prop.Type, FieldAttributes.Private);

                get = builder.DefineMethod("get_" + prop.Name, attr, prop.Type, Type.EmptyTypes);
                ILGenerator getIL = get.GetILGenerator();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, field);
                getIL.Emit(OpCodes.Ret);

                set = builder.DefineMethod("set_" + prop.Name, attr, typeof(void), new Type[] { prop.Type });
                ILGenerator setIL = set.GetILGenerator();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, field);
                setIL.Emit(OpCodes.Ret);
            }
            else
            {
                if (prop.GetBody != null)
                {
                    get = builder.DefineMethod("get_" + prop.Name, attr, prop.Type, Type.EmptyTypes);
                    EmitContextMethod(builder, get, prop.GetBody, fields);
                }
                if (prop.SetBody != null)
                {
                    set = builder.DefineMethod("set_" + prop.Name, attr, typeof(void), new Type[] { prop.Type });
                    EmitContextMethod(builder, set, prop.SetBody, fields);
                }
            }

            if (set != null)
                propBuilder.SetSetMethod(set);
            if (get != null)
                propBuilder.SetGetMethod(get);
        }

        private void EmitContextMethod(TypeBuilder builder, MethodBuilder method, MethodBodyReader reader, Dictionary<string, FieldBuilder> fields)
        {
            var writer = new MethodBodyBuilder(method);

            var locals = reader.Locals.Select(lc => lc.LocalType);
            writer.DeclareLocals(locals);
            writer.DeclareExceptionHandlers(reader.ExceptionHandlers);

            var gen = writer.Generator;

            var oldBody = reader.Instructions;
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
                {
                    throw new NotSupportedException("Cannot access 'this' in dynamic method body.");
                }
                else if (loc == 1)
                {
                    var endIndex = index + 1;
                    while (endIndex < oldBody.Length)
                    {
                        var checkIns = oldBody[endIndex];
                        endIndex++;
                        if (checkIns.OpCode == OpCodes.Callvirt)
                            break;
                    }

                    var newInstructions = ProcessLdContextInstruction(oldBody.Skip(index).Take(endIndex - index).ToArray(), fields);
                    newBody.AddRange(newInstructions);

                    index = endIndex;
                }
                else 
                {
                    ins.OpCode = OpCodes.Ldarg;
                    ins.Operand = loc - 1;
                    newBody.Add(ins);
                }
            }

            writer.EmitBody(newBody);
        }

        private Instruction[] ProcessLdContextInstruction(Instruction[] body, Dictionary<string, FieldBuilder> fields)
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
        public sealed class DynamicClassContext
        {
            internal DynamicClassContext()
            {
            }

            // these methods get re-written in IL

            internal static MethodInfo info_getField =>
                typeof(DynamicClassContext).GetMethod(nameof(Field), new Type[] { typeof(string) });
            public object Field(string name) { return null; }

            internal static MethodInfo info_setField =>
                typeof(DynamicClassContext).GetMethod(nameof(Field), new Type[] { typeof(string), typeof(string) });
            public void Field(string name, string value) { }
        }
    }

}
