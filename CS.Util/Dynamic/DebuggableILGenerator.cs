using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Dynamic
{
    public interface ILGeneratorInterface : _ILGenerator
    {
        void Emit(OpCode opcode);
        void Emit(OpCode opcode, byte arg);
        void Emit(OpCode opcode, sbyte arg);
        void Emit(OpCode opcode, short arg);
        void Emit(OpCode opcode, int arg);
        void Emit(OpCode opcode, MethodInfo meth);
        void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes);
        void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes);
        void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes);
        void Emit(OpCode opcode, SignatureHelper signature);
        void Emit(OpCode opcode, ConstructorInfo con);
        void Emit(OpCode opcode, Type cls);
        void Emit(OpCode opcode, long arg);
        unsafe void Emit(OpCode opcode, float arg);
        unsafe void Emit(OpCode opcode, double arg);
        void Emit(OpCode opcode, Label label);
        void Emit(OpCode opcode, Label[] labels);
        void Emit(OpCode opcode, FieldInfo field);
        void Emit(OpCode opcode, String str);
        void Emit(OpCode opcode, LocalBuilder local);
        Label BeginExceptionBlock();
        void EndExceptionBlock();
        void BeginExceptFilterBlock();
        void BeginCatchBlock(Type exceptionType);
        void BeginFaultBlock();
        void BeginFinallyBlock();
        Label DefineLabel();
        void MarkLabel(Label loc);
        void ThrowException(Type excType);
        void EmitWriteLine(String value);
        void EmitWriteLine(LocalBuilder localBuilder);
        void EmitWriteLine(FieldInfo fld);
        LocalBuilder DeclareLocal(Type localType);
        LocalBuilder DeclareLocal(Type localType, bool pinned);
        void UsingNamespace(String usingNamespace);

        void MarkSequencePoint(ISymbolDocumentWriter document,
            int startLine,       // line number is 1 based 
            int startColumn,     // column is 0 based
            int endLine,         // line number is 1 based
            int endColumn)       // column is 0 based
            ;
        void BeginScope();
        void EndScope();
        int ILOffset { get; }
    }

    public class DebuggableILGenerator : ILGeneratorInterface
    {
        private ILGeneratorInterface _implementation;

        public DebuggableILGenerator(ILGenerator generator, ISymbolDocumentWriter writer, string sourcePath)
        {
            _implementation = (ILGeneratorInterface)new implProxy(generator, writer, sourcePath).GetTransparentProxy();
        }

        private class implProxy : RealProxy
        {
            private readonly ISymbolDocumentWriter _writer;
            private readonly string _sourcePath;
            private readonly StandardILGenerator _impl;
            public implProxy(ILGenerator generator, ISymbolDocumentWriter writer, string sourcePath)
                : base(typeof(ILGeneratorInterface))
            {
                _writer = writer;
                _sourcePath = sourcePath;
                _impl = new StandardILGenerator(generator);
            }

            private void LogIL(object[] args)
            {
                if (!(args[0] is OpCode))
                    return;

                OpCode code = ((OpCode)args[0]);
                object operand = args.Length > 1 ? args[1] : null;

                string message = "    " + code + " " + operand + Environment.NewLine;
                File.AppendAllText(_sourcePath, message);
                int line = File.ReadLines(_sourcePath).Count();

                _impl.MarkSequencePoint(_writer, line, 0, line, 150);
            }

            public override IMessage Invoke(IMessage msg)
            {
                var methodCall = msg as IMethodCallMessage;
                var methodInfo = methodCall.MethodBase as MethodInfo;
                try
                {
                    if (methodInfo.Name.StartsWith("Emit"))
                        LogIL(methodCall.InArgs);
                    var result = methodInfo.Invoke(_impl, methodCall.InArgs);
                    return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
                }
                catch (Exception e)
                {
                    return new ReturnMessage(e, methodCall);
                }
            }
        }

        #region ILGeneratorWrapperInterface

        public void GetTypeInfoCount(out uint pcTInfo)
        {
            _implementation.GetTypeInfoCount(out pcTInfo);
        }

        public void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            _implementation.GetTypeInfo(iTInfo, lcid, ppTInfo);
        }

        public void GetIDsOfNames(ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            _implementation.GetIDsOfNames(ref riid, rgszNames, cNames, lcid, rgDispId);
        }

        public void Invoke(uint dispIdMember, ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult,
            IntPtr pExcepInfo, IntPtr puArgErr)
        {
            _implementation.Invoke(dispIdMember, ref riid, lcid, wFlags, pDispParams, pVarResult, pExcepInfo, puArgErr);
        }

        public void Emit(OpCode opcode)
        {
            _implementation.Emit(opcode);
        }

        public void Emit(OpCode opcode, byte arg)
        {
            _implementation.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, sbyte arg)
        {
            _implementation.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, short arg)
        {
            _implementation.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, int arg)
        {
            _implementation.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, MethodInfo meth)
        {
            _implementation.Emit(opcode, meth);
        }

        public void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes,
            Type[] optionalParameterTypes)
        {
            _implementation.EmitCalli(opcode, callingConvention, returnType, parameterTypes, optionalParameterTypes);
        }

        public void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes)
        {
            _implementation.EmitCalli(opcode, unmanagedCallConv, returnType, parameterTypes);
        }

        public void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
        {
            _implementation.EmitCall(opcode, methodInfo, optionalParameterTypes);
        }

        public void Emit(OpCode opcode, SignatureHelper signature)
        {
            _implementation.Emit(opcode, signature);
        }

        public void Emit(OpCode opcode, ConstructorInfo con)
        {
            _implementation.Emit(opcode, con);
        }

        public void Emit(OpCode opcode, Type cls)
        {
            _implementation.Emit(opcode, cls);
        }

        public void Emit(OpCode opcode, long arg)
        {
            _implementation.Emit(opcode, arg);
        }

        public unsafe void Emit(OpCode opcode, float arg)
        {
            _implementation.Emit(opcode, arg);
        }

        public unsafe void Emit(OpCode opcode, double arg)
        {
            _implementation.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, Label label)
        {
            _implementation.Emit(opcode, label);
        }

        public void Emit(OpCode opcode, Label[] labels)
        {
            _implementation.Emit(opcode, labels);
        }

        public void Emit(OpCode opcode, FieldInfo field)
        {
            _implementation.Emit(opcode, field);
        }

        public void Emit(OpCode opcode, string str)
        {
            _implementation.Emit(opcode, str);
        }

        public void Emit(OpCode opcode, LocalBuilder local)
        {
            _implementation.Emit(opcode, local);
        }

        public Label BeginExceptionBlock()
        {
            return _implementation.BeginExceptionBlock();
        }

        public void EndExceptionBlock()
        {
            _implementation.EndExceptionBlock();
        }

        public void BeginExceptFilterBlock()
        {
            _implementation.BeginExceptFilterBlock();
        }

        public void BeginCatchBlock(Type exceptionType)
        {
            _implementation.BeginCatchBlock(exceptionType);
        }

        public void BeginFaultBlock()
        {
            _implementation.BeginFaultBlock();
        }

        public void BeginFinallyBlock()
        {
            _implementation.BeginFinallyBlock();
        }

        public Label DefineLabel()
        {
            return _implementation.DefineLabel();
        }

        public void MarkLabel(Label loc)
        {
            _implementation.MarkLabel(loc);
        }

        public void ThrowException(Type excType)
        {
            _implementation.ThrowException(excType);
        }

        public void EmitWriteLine(string value)
        {
            _implementation.EmitWriteLine(value);
        }

        public void EmitWriteLine(LocalBuilder localBuilder)
        {
            _implementation.EmitWriteLine(localBuilder);
        }

        public void EmitWriteLine(FieldInfo fld)
        {
            _implementation.EmitWriteLine(fld);
        }

        public LocalBuilder DeclareLocal(Type localType)
        {
            return _implementation.DeclareLocal(localType);
        }

        public LocalBuilder DeclareLocal(Type localType, bool pinned)
        {
            return _implementation.DeclareLocal(localType, pinned);
        }

        public void UsingNamespace(string usingNamespace)
        {
            _implementation.UsingNamespace(usingNamespace);
        }

        public void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
        {
            _implementation.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }

        public void BeginScope()
        {
            _implementation.BeginScope();
        }

        public void EndScope()
        {
            _implementation.EndScope();
        }

        public int ILOffset
        {
            get { return _implementation.ILOffset; }
        }

        #endregion
    }

    public class StandardILGenerator : ILGeneratorInterface
    {
        private readonly ILGenerator _generator;

        public StandardILGenerator(ILGenerator generator)
        {
            _generator = generator;
        }

        #region ILGeneratorWrapperInterface

        public void Emit(OpCode opcode)
        {
            _generator.Emit(opcode);
        }

        public void Emit(OpCode opcode, byte arg)
        {
            _generator.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, sbyte arg)
        {
            _generator.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, short arg)
        {
            _generator.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, int arg)
        {
            _generator.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, MethodInfo meth)
        {
            _generator.Emit(opcode, meth);
        }

        public void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes,
            Type[] optionalParameterTypes)
        {
            _generator.EmitCalli(opcode, callingConvention, returnType, parameterTypes, optionalParameterTypes);
        }

        public void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes)
        {
            _generator.EmitCalli(opcode, unmanagedCallConv, returnType, parameterTypes);
        }

        public void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
        {
            _generator.EmitCall(opcode, methodInfo, optionalParameterTypes);
        }

        public void Emit(OpCode opcode, SignatureHelper signature)
        {
            _generator.Emit(opcode, signature);
        }

        public void Emit(OpCode opcode, ConstructorInfo con)
        {
            _generator.Emit(opcode, con);
        }

        public void Emit(OpCode opcode, Type cls)
        {
            _generator.Emit(opcode, cls);
        }

        public void Emit(OpCode opcode, long arg)
        {
            _generator.Emit(opcode, arg);
        }

        public unsafe void Emit(OpCode opcode, float arg)
        {
            _generator.Emit(opcode, arg);
        }

        public unsafe void Emit(OpCode opcode, double arg)
        {
            _generator.Emit(opcode, arg);
        }

        public void Emit(OpCode opcode, Label label)
        {
            _generator.Emit(opcode, label);
        }

        public void Emit(OpCode opcode, Label[] labels)
        {
            _generator.Emit(opcode, labels);
        }

        public void Emit(OpCode opcode, FieldInfo field)
        {
            _generator.Emit(opcode, field);
        }

        public void Emit(OpCode opcode, string str)
        {
            _generator.Emit(opcode, str);
        }

        public void Emit(OpCode opcode, LocalBuilder local)
        {
            _generator.Emit(opcode, local);
        }

        public Label BeginExceptionBlock()
        {
            return _generator.BeginExceptionBlock();
        }

        public void EndExceptionBlock()
        {
            _generator.EndExceptionBlock();
        }

        public void BeginExceptFilterBlock()
        {
            _generator.BeginExceptFilterBlock();
        }

        public void BeginCatchBlock(Type exceptionType)
        {
            _generator.BeginCatchBlock(exceptionType);
        }

        public void BeginFaultBlock()
        {
            _generator.BeginFaultBlock();
        }

        public void BeginFinallyBlock()
        {
            _generator.BeginFinallyBlock();
        }

        public Label DefineLabel()
        {
            return _generator.DefineLabel();
        }

        public void MarkLabel(Label loc)
        {
            _generator.MarkLabel(loc);
        }

        public void ThrowException(Type excType)
        {
            _generator.ThrowException(excType);
        }

        public void EmitWriteLine(string value)
        {
            _generator.EmitWriteLine(value);
        }

        public void EmitWriteLine(LocalBuilder localBuilder)
        {
            _generator.EmitWriteLine(localBuilder);
        }

        public void EmitWriteLine(FieldInfo fld)
        {
            _generator.EmitWriteLine(fld);
        }

        public LocalBuilder DeclareLocal(Type localType)
        {
            return _generator.DeclareLocal(localType);
        }

        public LocalBuilder DeclareLocal(Type localType, bool pinned)
        {
            return _generator.DeclareLocal(localType, pinned);
        }

        public void UsingNamespace(string usingNamespace)
        {
            _generator.UsingNamespace(usingNamespace);
        }

        public void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
        {
            _generator.MarkSequencePoint(document, startLine, startColumn, endLine, endColumn);
        }

        public void BeginScope()
        {
            _generator.BeginScope();
        }

        public void EndScope()
        {
            _generator.EndScope();
        }

        public int ILOffset => _generator.ILOffset;
        public void GetTypeInfoCount(out uint pcTInfo)
        {
            ((_ILGenerator)_generator).GetTypeInfoCount(out pcTInfo);
        }

        public void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            ((_ILGenerator)_generator).GetTypeInfo(iTInfo, lcid, ppTInfo);
        }

        public void GetIDsOfNames(ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            ((_ILGenerator)_generator).GetIDsOfNames(ref riid, rgszNames, cNames, lcid, rgDispId);
        }

        public void Invoke(uint dispIdMember, ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult,
            IntPtr pExcepInfo, IntPtr puArgErr)
        {
            ((_ILGenerator)_generator).Invoke(dispIdMember, ref riid, lcid, wFlags, pDispParams, pVarResult, pExcepInfo, puArgErr);
        }

        #endregion
    }
}
