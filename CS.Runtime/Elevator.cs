using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CS.Util.Dynamic;
using CS.Util.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CS.Util
{
    /// <summary>
    /// Elevator provides a method to compile a specified lambda into a separate assembly for execution. 
    /// The lambda can depend on the current or other references, and it will capture local lambda variables as well.
    /// </summary>
    public class Elevator
    {
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action> Compile(Action method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { }, null, options);
            return new ElevatorAssembly<Action>(() => assy.Run(new object[] {  }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action<T>> Compile<T>(Action<T> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new[] { typeof(T) }, null, options);
            return new ElevatorAssembly<Action<T>>((one) => assy.Run(new object[] { one }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action<T1, T2>> Compile<T1, T2>(Action<T1, T2> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new[] { typeof(T1), typeof(T2) }, null, options);
            return new ElevatorAssembly<Action<T1, T2>>((one, two) => assy.Run(new object[] { one, two }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action<T1, T2, T3>> Compile<T1, T2, T3>(Action<T1, T2, T3> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new[] { typeof(T1), typeof(T2), typeof(T3) }, null, options);
            return new ElevatorAssembly<Action<T1, T2, T3>>((one, two, three) => assy.Run(new object[] { one, two, three }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action<T1, T2, T3, T4>> Compile<T1, T2, T3, T4>(Action<T1, T2, T3, T4> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, options);
            return new ElevatorAssembly<Action<T1, T2, T3, T4>>((one, two, three, four) => assy.Run(new object[] { one, two, three, four }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action<T1, T2, T3, T4, T5>> Compile<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, null, options);
            return new ElevatorAssembly<Action<T1, T2, T3, T4, T5>>((one, two, three, four, five) => assy.Run(new object[] { one, two, three, four, five }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action<T1, T2, T3, T4, T5, T6>> Compile<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, null, options);
            return new ElevatorAssembly<Action<T1, T2, T3, T4, T5, T6>>((one, two, three, four, five, six) => assy.Run(new object[] { one, two, three, four, five, six }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action<T1, T2, T3, T4, T5, T6, T7>> Compile<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }, null, options);
            return new ElevatorAssembly<Action<T1, T2, T3, T4, T5, T6, T7>>((one, two, three, four, five, six, seven) => assy.Run(new object[] { one, two, three, four, five, six, seven }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Action<T1, T2, T3, T4, T5, T6, T7, T8>> Compile<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }, null, options);
            return new ElevatorAssembly<Action<T1, T2, T3, T4, T5, T6, T7, T8>>((one, two, three, four, five, six, seven, eight) => assy.Run(new object[] { one, two, three, four, five, six, seven, eight }), assy.Dispose);
        }

        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Func<TResult>> Compile<TResult>(Func<TResult> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { }, typeof(TResult), options);
            return new ElevatorAssembly<Func<TResult>>(() => (TResult)assy.Run(new object[] { }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Func<T1, TResult>> Compile<T1, TResult>(Func<T1, TResult> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { typeof(T1) }, typeof(TResult), options);
            return new ElevatorAssembly<Func<T1, TResult>>((one) => (TResult)assy.Run(new object[] { one }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Func<T1, T2, TResult>> Compile<T1, T2, TResult>(Func<T1, T2, TResult> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { typeof(T1), typeof(T2) }, typeof(TResult), options);
            return new ElevatorAssembly<Func<T1,T2, TResult>>((one, two) => (TResult)assy.Run(new object[] { one, two }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Func<T1, T2, T3, TResult>> Compile<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { typeof(T1), typeof(T2), typeof(T3) }, typeof(TResult), options);
            return new ElevatorAssembly<Func<T1, T2, T3, TResult>>((one, two, three) => (TResult)assy.Run(new object[] { one, two, three }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Func<T1, T2, T3, T4, TResult>> Compile<T1, T2, T3, T4, TResult>(Func<T1, T2, T3, T4, TResult> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, typeof(TResult), options);
            return new ElevatorAssembly<Func<T1, T2, T3, T4, TResult>>((one, two, three, four) => (TResult)assy.Run(new object[] { one, two, three, four }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Func<T1, T2, T3, T4, T5, TResult>> Compile<T1, T2, T3, T4, T5, TResult>(Func<T1, T2, T3, T4, T5, TResult> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, typeof(TResult), options);
            return new ElevatorAssembly<Func<T1, T2, T3, T4, T5, TResult>>((one, two, three, four, five) => (TResult)assy.Run(new object[] { one, two, three, four, five }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Func<T1, T2, T3, T4, T5, T6, TResult>> Compile<T1, T2, T3, T4, T5, T6, TResult>(Func<T1, T2, T3, T4, T5, T6, TResult> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, typeof(TResult), options);
            return new ElevatorAssembly<Func<T1, T2, T3, T4, T5, T6, TResult>>((one, two, three, four, five, six) => (TResult)assy.Run(new object[] { one, two, three, four, five, six }), assy.Dispose);
        }
        /// <summary>
        /// Compiles the provided delegate into a separate assembly, so that the code can be executed with different
        /// paramaters than the current process. (run as administrator, etc) 
        /// </summary>
        /// <param name="method">The delegate to compile</param>
        /// <param name="options">Optionally specify custom compilation options</param>
        /// <returns>Returns replacement delegate that will execute the original delegate but in a different process.</returns>
        public static ElevatorAssembly<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> Compile<T1, T2, T3, T4, T5, T6, T7, TResult>(Func<T1, T2, T3, T4, T5, T6, T7, TResult> method, ElevatorOptions options = null)
        {
            var assy = InternalGenerate(method.Method, method.Target, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }, typeof(TResult), options);
            return new ElevatorAssembly<Func<T1, T2, T3, T4, T5, T6, T7, TResult>>((one, two, three, four, five, six, seven) => (TResult)assy.Run(new object[] { one, two, three, four, five, six, seven }), assy.Dispose);
        }

        private static InternalAssemblyRunner InternalGenerate(MethodInfo method, object target, Type[] paramaterTypes, Type returnType,
            ElevatorOptions options)
        {
            if (options == null)
                options = new ElevatorOptions();
            if (returnType == null)
                returnType = typeof(void);

            var assyName = options.AssemblyName;
            var assyFile = assyName + ".exe";
            var dir = Path.Combine(options.TempPath, RandomEx.GetString(8));
            Directory.CreateDirectory(dir);

            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName(assyName), AssemblyBuilderAccess.Save, dir);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assyName, assyFile);
            var typeBuilder = moduleBuilder.DefineType("Program", TypeAttributes.Class | TypeAttributes.Public);

            var mainMethod = typeBuilder.DefineMethod("Main",
                MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static,
                typeof(int), new Type[] { typeof(string[]) });
            var dynName = RandomEx.GetString(8);
            var dynMethod = typeBuilder.DefineMethod(dynName,
                MethodAttributes.Public | MethodAttributes.Static,
                returnType, paramaterTypes);

            var ref1 = EmitModifiedMethod(dynMethod, method, target);
            var ref2 = EmitMainMethod(mainMethod, dynName, paramaterTypes);

            typeBuilder.CreateType();
            assemblyBuilder.SetEntryPoint(mainMethod, PEFileKinds.ConsoleApplication);
            assemblyBuilder.Save(assyFile);

            var references = assemblyBuilder.GetReferencedAssemblies()
                .Select(r => Assembly.ReflectionOnlyLoad(r.FullName))
                .Concat(ref1)
                .Concat(ref2)
                .Concat(options.Dependancies)
                .Distinct(new AssemblyFullNameEqualityComparer())
                .Where(a => !a.IsInGAC());
            foreach (var refAssy in references)
            {
                var refName = Path.GetFileName(refAssy.Location);
                File.Copy(refAssy.Location, Path.Combine(dir, refName));
            }

            return new InternalAssemblyRunner(Path.Combine(dir, assyFile), paramaterTypes, returnType, options);
        }
        private static Assembly[] EmitModifiedMethod(MethodBuilder methodBuilder, MethodInfo methodInfo, object target)
        {
            // init method builder
            var jsonObj = JsonConvert.SerializeObject(target);
            var reader = MethodBodyReader.Read(methodInfo);
            var writer = new MethodBodyBuilder(methodBuilder);

            // init method locals
            var locals = reader.Locals.Select(lc => lc.LocalType).Concat(new[] { typeof(JObject) });
            writer.DeclareLocals(locals);
            writer.DeclareExceptionHandlers(reader.ExceptionHandlers);

            // serialize static container class into method
            var gen = writer.Generator;
            gen.Emit(OpCodes.Ldstr, jsonObj);
            gen.Emit(OpCodes.Call,
                (typeof(JObject)).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public, null,
                    new[] { typeof(string) }, null));
            gen.Emit(OpCodes.Stloc, locals.Count() - 1);

            var obgetv = typeof(JObject).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(string) }, null);
            var tkval = typeof(JToken).GetMethod("ToObject", BindingFlags.Public | BindingFlags.Instance, null, new[] { typeof(Type) }, null);
            var gttype = (typeof(Type)).GetMethod("GetType", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string) }, null);

            // emit the original IL with the exception that ldarg.0 needs to load our JObject
            // instead of the delegate container class.
            var oldBody = reader.Instructions;
            List<Instruction> newBody = new List<Instruction>();
            for (int index = 0; index < oldBody.Length; index++)
            {
                var ins = oldBody[index];
                var name = ins.OpCode.Name;
                if (name.StartsWith("ldarg"))
                {
                    int loc;
                    int sepIndex = name.IndexOf('.');
                    if (sepIndex > 0)
                    {
                        var tmp = name.Substring(sepIndex + 1);
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

                    if (loc == 0)
                    {
                        if (oldBody.Length < index + 1 || oldBody[index + 1].OpCode.Name != "ldfld")
                            throw new NotSupportedException("Unsupported case: ldarg.0 but no ldfld afterwards.");

                        var field = (FieldInfo)oldBody[index + 1].Operand;
                        newBody.Add(new Instruction(-1, OpCodes.Ldloc) { Operand = locals.Count() - 1 });
                        newBody.Add(new Instruction(-1, OpCodes.Ldstr) { Operand = field.Name });
                        newBody.Add(new Instruction(-1, OpCodes.Callvirt) { Operand = obgetv });
                        newBody.Add(new Instruction(-1, OpCodes.Ldstr) { Operand = field.FieldType.AssemblyQualifiedName });
                        newBody.Add(new Instruction(-1, OpCodes.Call) { Operand = gttype });
                        newBody.Add(new Instruction(-1, OpCodes.Callvirt) { Operand = tkval });
                        if(field.FieldType.IsValueType)
                            newBody.Add(new Instruction(-1, OpCodes.Unbox_Any) { Operand = field.FieldType });
                        else 
                            newBody.Add(new Instruction(-1, OpCodes.Castclass) { Operand = field.FieldType });
                        index++;
                    }
                    else
                    {
                        ins.OpCode = OpCodes.Ldarg;
                        ins.Operand = loc - 1;
                        newBody.Add(ins);
                    }
                }
                else
                {
                    newBody.Add(ins);
                }
            }
            writer.EmitBody(newBody);
            return writer.ReferencedAssemblies;
        }
        private static Assembly[] EmitMainMethod(MethodBuilder methodBuilder, string targetName, Type[] targetTypes)
        {
            int paramCount = targetTypes.Count();
            string mthName = targetName;
            string[] paramaterTypes = targetTypes.Select(p => p.AssemblyQualifiedName).ToArray();
            Func<string[], int> main = args =>
            {
                string resultFile = null;
                try
                {
                    resultFile = args[1];
                    var json = JArray.Parse(File.ReadAllText(args[0]));
                    List<object> paramaters = new List<object>();
                    foreach (var token in json)
                    {
                        var type = Type.GetType(token.SelectToken("Key").ToString());
                        paramaters.Add(token.SelectToken("Value").ToObject(type));
                    }

                    if (paramaters.Count != paramCount)
                        throw new TargetParameterCountException("The provided paramaters don't match the target method");

                    Type meType = MethodBase.GetCurrentMethod().DeclaringType;
                    MethodInfo mi = meType.GetMethod(
                        mthName,
                        BindingFlags.Public | BindingFlags.Static,
                        null,
                        paramaterTypes.Select(Type.GetType).ToArray(),
                        null);

                    object result = mi.Invoke(null, paramaters.ToArray());

                    File.WriteAllText(resultFile, JsonConvert.SerializeObject(result));
                    return 0;
                }
                catch (Exception ex)
                {
                    if (resultFile != null)
                    {
                        if (ex is TargetInvocationException)
                            File.WriteAllText(resultFile, ex.InnerException.Message);
                        else
                            File.WriteAllText(resultFile, ex.Message);
                    }
                    return 1;
                }
            };

            return EmitModifiedMethod(methodBuilder, main.Method, main.Target);
        }

        private class InternalAssemblyRunner : IDisposable
        {
            private readonly string _exePath;
            private readonly Type[] _paraTypes;
            private readonly Type _returnType;
            private readonly ElevatorOptions _options;
            private bool _disposed;

            public InternalAssemblyRunner(string exePath, Type[] paraTypes, Type returnType, ElevatorOptions options)
            {
                _exePath = exePath;
                _paraTypes = paraTypes;
                _returnType = returnType;
                _options = options;
            }
            public object Run(object[] paras)
            {
                if (_disposed)
                    throw new ObjectDisposedException("assyRunner");

                // this should technically never happen...
                if (!Enumerable.SequenceEqual(paras.Select(s => s.GetType()), _paraTypes))
                    throw new ArgumentException("Invoked param types do not match.", nameof(paras));

                var dir = Path.GetDirectoryName(_exePath);
                var instStr = RandomEx.GetString(8);
                var argsPath = Path.Combine(dir, instStr) + ".init";
                var resultPath = Path.Combine(dir, instStr) + ".result";

                List<KeyValuePair<string, object>> wType =
                    paras.Select(o => new KeyValuePair<string, object>(o.GetType().AssemblyQualifiedName, o))
                    .ToList();

                var serialized = JsonConvert.SerializeObject(wType);
                File.WriteAllText(argsPath, serialized);

                ProcessStartInfo stInfo = new ProcessStartInfo(_exePath, "\"" + argsPath + "\" \"" + resultPath + "\"");
                stInfo.CreateNoWindow = true;
                if (_options.RunElevated)
                    stInfo.Verb = "runas";
                stInfo.WorkingDirectory = dir;
                stInfo.UseShellExecute = true;

                Stopwatch sw = new Stopwatch();
                sw.Start();
                Process p;
                try
                {
                    p = Process.Start(stInfo);
                }
                catch (Win32Exception win32ex)
                {
                    if(win32ex.HResult == -2147467259)
                        throw new ElevatorException("The option 'RunElevated'=true, and user denied the UAC prompt.", win32ex);
                    throw;
                }
                p.WaitForExit((int)_options.Timeout.TotalMilliseconds);

                // this is to prevent race conditions with WaitForExit returning before the process has finished cleaning up.
                if (p.HasExited == false && sw.ElapsedMilliseconds < _options.Timeout.TotalMilliseconds)
                    Thread.Sleep(20);

                sw.Stop();
                if (p.HasExited == false)
                {
                    p.Kill();
                    throw new TimeoutException("Operation has timed out.");
                }

                if (p.ExitCode == 0) // success
                {
                    if (_returnType == null || _returnType == typeof(void))
                    {
                        return null;
                    }
                    else
                    {
                        if (!File.Exists(resultPath))
                            throw new ElevatorException("Process exited with code 0 but with not with expected result.");

                        var resultJson = File.ReadAllText(resultPath);
                        File.Delete(resultPath);
                        if(File.Exists(argsPath))
                            File.Delete(argsPath);
                        try
                        {
                            return JsonConvert.DeserializeObject(resultJson, _returnType);
                        }
                        catch (Exception ex)
                        {
                            throw new ElevatorException("Process exited with code 0 but with not with expected result.", ex);
                        }
                    }
                }
                else if (p.ExitCode == 1) // exception
                {
                    if (!File.Exists(resultPath))
                        throw new ElevatorException("An unknown exception has occured.");

                    throw new ElevatorException(File.ReadAllText(resultPath));
                }
                else
                {
                    throw new ElevatorException("Process returned an invalid and unexpected exit code.");
                }
            }
            public void Dispose()
            {
                _disposed = true;
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    var dir = Path.GetDirectoryName(_exePath);
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            Directory.Delete(dir, true);
                            break;
                        }
                        catch
                        {
                            Thread.Sleep(2000);
                        }
                    }
                });
            }
        }
    }
    public class ElevatorException : Exception
    {
        internal ElevatorException()
        {
        }
        internal ElevatorException(string message)
            :base(message)
        {
        }
        internal ElevatorException(string message, Exception innerException)
            :base(message, innerException)
        {
        }
    }
    public class ElevatorOptions
    {
        public bool RunElevated { get; set; } = false;
        public List<Assembly> Dependancies { get; set; } = new List<Assembly>();
        public string TempPath { get; set; } = Path.GetTempPath();
        public string AssemblyName { get; set; } = RandomEx.GetString(8);
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }

    public class ElevatorAssembly<T> : IDisposable where T : class
    {
        /// <summary>
        /// Executes the compiled delegate in a separate process, with the specified options.
        /// </summary>
        /// <exception cref="ElevatorException">Throws if the delegate threw an exception, or if there was a problem running the assembly</exception>
        /// <exception cref="ObjectDisposedException">Throws if the delegate has been disposed.</exception>
        /// <exception cref="TimeoutException">Throws if process does not return in the specified time</exception>
        public T Run
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException("ElevatorAssembly");
                return _delegate;
            }
        }

        private readonly Action _dispose;
        private bool _disposed;
        private readonly T _delegate;

        internal ElevatorAssembly(T @delegate, Action dispose)
        {
            if (!typeof(T).IsSubclassOf(typeof(Delegate)))
            {
                throw new InvalidOperationException(typeof(T).Name + " is not a delegate type");
            }
            _delegate = @delegate;
            _dispose = dispose;
        }

        /// <summary>
        /// Disposes the compiled delegate and assembly, and cleans up temporary files asynchronously.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _dispose();
            }
        }
    }
}
