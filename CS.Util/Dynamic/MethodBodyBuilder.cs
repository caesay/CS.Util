using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using CS.Util.Extensions;

namespace CS.Util.Dynamic
{
    public class MethodBodyBuilder
    {
        public ILGenerator Generator { get { return _il; } }

        public Assembly[] ReferencedAssemblies
        {
            get
            {
                return _refAssemblies.Distinct().Where(r => !r.IsInGAC()).ToArray();
            }
        }

        private readonly ILGenerator _il;
        private readonly Dictionary<int, LocalBuilder> _locals = new Dictionary<int, LocalBuilder>();
        private readonly Dictionary<int, Action> _actions = new Dictionary<int, Action>();
        private readonly Dictionary<int, Label> _labels = new Dictionary<int, Label>();
        private readonly object _lock = new object();
        private readonly List<Assembly> _refAssemblies = new List<Assembly>();

        private bool _emit;

        public MethodBodyBuilder(MethodBuilder builder) : this(builder.GetILGenerator()) { }
        public MethodBodyBuilder(ConstructorBuilder builder) : this(builder.GetILGenerator()) { }

        public MethodBodyBuilder(ILGenerator generator)
        {
            _il = generator;
        }

        public static void BuildFromExistingMethod(string newMethodName, MethodInfo originalMethod, TypeBuilder builder)
        {
            var tMethod = builder.DefineMethod(newMethodName, originalMethod.Attributes, originalMethod.CallingConvention,
                originalMethod.ReturnType, originalMethod.GetParameters().Select(p => p.ParameterType).ToArray());

            var reader = MethodBodyReader.Read(originalMethod);

            MethodBodyBuilder bb = new MethodBodyBuilder(tMethod.GetILGenerator());
            bb.DeclareLocals(reader.Locals.Select(l => l.LocalType));
            bb.DeclareExceptionHandlers(reader.ExceptionHandlers);
            bb.EmitBody(reader.Instructions);
        }

        public void DeclareLocals(IEnumerable<Type> types)
        {
            lock (_lock)
            {
                CanEmitOrThrow();
                foreach (var t in types)
                {
                    var bl = _il.DeclareLocal(t);
                    _locals.Add(bl.LocalIndex, bl);
                }
            }
        }
        public void DeclareExceptionHandlers(IEnumerable<ExceptionHandlingClause> exceptionHandlers)
        {
            lock (_lock)
            {
                CanEmitOrThrow();
                var generator = _il;
                foreach (ExceptionHandlingClause exceptionHandler in exceptionHandlers)
                {
                    ActionAddAfter(exceptionHandler.TryOffset, delegate
                    {
                        generator.BeginExceptionBlock();
                    });

                    if (exceptionHandler.Flags.HasFlag(ExceptionHandlingClauseOptions.Filter))
                    {
                        ActionAddAfter(exceptionHandler.FilterOffset, delegate
                        {
                            generator.BeginExceptFilterBlock();
                        });
                    }

                    ActionAddAfter(exceptionHandler.HandlerOffset, delegate
                    {
                        // if Clause or Filter - its weird, because Clause is 0 and this makes weird bugs, for instance:
                        // Anything(n) & Clause(0) = Anything(n)... 
                        // so we need to check if it == 0, or == 1 for the case where Filter is also set
                        if ((int)exceptionHandler.Flags == 0 || (int)exceptionHandler.Flags == 1)
                        {
                            generator.BeginCatchBlock(exceptionHandler.CatchType);
                        }
                        else if (exceptionHandler.Flags.HasFlag(ExceptionHandlingClauseOptions.Fault))
                        {
                            generator.BeginFaultBlock();
                        }
                        else if (exceptionHandler.Flags.HasFlag(ExceptionHandlingClauseOptions.Finally))
                        {
                            generator.BeginFinallyBlock();
                        }
                    });

                    ActionAddBefore(exceptionHandler.HandlerOffset + exceptionHandler.HandlerLength, delegate
                    {
                        generator.EndExceptionBlock();
                    });
                }
            }
        }
        public void EmitBody(IEnumerable<Instruction> body)
        {
            lock (_lock)
            {
                CanEmitOrThrow();
                _emit = true;
                var instructions = body.ToArray();
                var generator = _il;
                CreateLabels(instructions);
                foreach (Instruction instruction in instructions)
                {
                    if (instruction.Offset > 0)
                    {
                        ActionRun(instruction.Offset);
                        generator.MarkLabel(_labels[instruction.Offset]);
                    }

                    switch (instruction.OpCode.OperandType)
                    {
                        case OperandType.InlineVar:
                        case OperandType.ShortInlineVar:
                            if (instruction.Operand is LocalVariableInfo)
                                generator.Emit(instruction.OpCode,
                                    _locals[((LocalVariableInfo)instruction.Operand).LocalIndex]);
                            else if (instruction.Operand is int)
                            {
                                if (TargetsLocalVariable(instruction.OpCode))
                                    generator.Emit(instruction.OpCode, _locals[(int) instruction.Operand]);
                                else
                                    generator.Emit(instruction.OpCode, (int)instruction.Operand);
                            }
                            else
                                throw new NotSupportedException("Operand for InlineVar is not of expected type");
                            break;

                        case OperandType.InlineField:
                            var inlineField = (FieldInfo)instruction.Operand;
                            _refAssemblies.Add(inlineField.DeclaringType.Assembly);
                            generator.Emit(instruction.OpCode, inlineField);
                            break;

                        case OperandType.InlineMethod:
                            var inlineMethod = (MethodBase)instruction.Operand;
                            _refAssemblies.Add(inlineMethod.DeclaringType.Assembly);
                            EmitWithMethodOperand(instruction.OpCode, inlineMethod);
                            break;

                        case OperandType.InlineType:
                            var inlineType = (Type)instruction.Operand;
                            _refAssemblies.Add(inlineType.Assembly);
                            generator.Emit(instruction.OpCode, inlineType);
                            break;

                        case OperandType.InlineTok:
                            Type inlineTokenType;
                            if (instruction.Operand is Type)
                            {
                                inlineTokenType = (Type)instruction.Operand;
                                generator.Emit(instruction.OpCode, (Type)instruction.Operand);
                            }
                            else if (instruction.Operand is FieldInfo)
                            {
                                inlineTokenType = ((FieldInfo)instruction.Operand).DeclaringType;
                                generator.Emit(instruction.OpCode, (FieldInfo)instruction.Operand);
                            }
                            else if (instruction.Operand is MethodBase)
                            {
                                inlineTokenType = ((MethodBase)instruction.Operand).DeclaringType;
                                EmitWithMethodOperand(instruction.OpCode, (MethodBase)instruction.Operand);
                            }
                            else
                                throw new NotSupportedException(string.Format("Unexpected token operand type: {0}.",
                                    instruction.Operand.GetType()));
                            _refAssemblies.Add(inlineTokenType.Assembly);
                            break;

                        case OperandType.ShortInlineI:
                            generator.Emit(instruction.OpCode, (sbyte)instruction.Operand);
                            break;

                        case OperandType.InlineI:
                            generator.Emit(instruction.OpCode, (int)instruction.Operand);
                            break;

                        case OperandType.InlineI8:
                            generator.Emit(instruction.OpCode, (long)instruction.Operand);
                            break;

                        case OperandType.ShortInlineR:
                            generator.Emit(instruction.OpCode, (float)instruction.Operand);
                            break;

                        case OperandType.InlineR:
                            generator.Emit(instruction.OpCode, (double)instruction.Operand);
                            break;

                        case OperandType.InlineString:
                            generator.Emit(instruction.OpCode, (string)instruction.Operand);
                            break;

                        case OperandType.InlineSwitch:
                            var targets = (Instruction[])instruction.Operand;
                            var labs = targets.Select(i => _labels[i.Offset]).ToArray();
                            generator.Emit(instruction.OpCode, labs);
                            break;

                        case OperandType.InlineNone:
                            generator.Emit(instruction.OpCode);
                            break;

                        case OperandType.ShortInlineBrTarget:
                        case OperandType.InlineBrTarget:
                            var target = (Instruction)instruction.Operand;
                            generator.Emit(instruction.OpCode, _labels[target.Offset]);
                            break;

                        case OperandType.InlineSig:
                        case OperandType.InlinePhi:
                        default:
                            throw new NotSupportedException(string.Format("Unexpected operand type: {0}.", instruction.OpCode.OperandType));
                    }
                }
            }
        }

        bool TargetsLocalVariable(OpCode opcode)
        {
            return opcode.Name.Contains("loc");
        }

        class ThisParameter : ParameterInfo
        {
            public ThisParameter(MethodBase method)
            {
                this.MemberImpl = method;
                this.ClassImpl = method.DeclaringType;
                this.NameImpl = "this";
                this.PositionImpl = -1;
            }
        }

        private void CanEmitOrThrow()
        {
            if (_emit)
                throw new InvalidOperationException("Can not modify method if already emitted.");
        }
        private void CreateLabels(IEnumerable<Instruction> body)
        {
            foreach (Instruction instruction in body)
            {
                if (instruction.Offset > 0)
                    _labels.Add(instruction.Offset, _il.DefineLabel());
            }
        }
        private void EmitWithMethodOperand(OpCode opcode, MethodBase methodBase)
        {
            MethodInfo methodInfo = methodBase as MethodInfo;
            if (methodInfo != null)
                _il.Emit(opcode, methodInfo);
            else
                _il.Emit(opcode, (ConstructorInfo)methodBase);
        }
        private void ActionAddBefore(int offset, Action action)
        {
            Action old;
            if (_actions.TryGetValue(offset, out old))
                _actions[offset] = (Action)Delegate.Combine(action, old);
            else
                _actions.Add(offset, action);
        }
        private void ActionAddAfter(int offset, Action action)
        {
            Action old;
            if (_actions.TryGetValue(offset, out old))
                _actions[offset] = (Action)Delegate.Combine(old, action);
            else
                _actions.Add(offset, action);
        }
        private void ActionRun(int offset)
        {
            Action block;
            if (_actions.TryGetValue(offset, out block))
                block();
        }
    }
}
