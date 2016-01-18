using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace CS.Util.Dynamic
{
    public class MethodBodyReader
    {
        public MethodBase Method { get { return _method; } }
        public Instruction[] Instructions { get { return _instructions.ToArray(); } }
        public ParameterInfo[] Paramaters { get { return _parameters; } }
        public LocalVariableInfo[] Locals { get { return _locals.ToArray(); } }
        public ExceptionHandlingClause[] ExceptionHandlers { get { return _body.ExceptionHandlingClauses.ToArray(); } }
        public int MaxStackSize { get { return _body.MaxStackSize; } }
        
        private static readonly OpCode[] OneByteCodes;
        private static readonly OpCode[] TwoByteCodes;

        static MethodBodyReader()
        {
            OneByteCodes = new OpCode[0xe1];
            TwoByteCodes = new OpCode[0x1f];

            var fields = typeof(OpCodes).GetFields(
                BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                var opcode = (OpCode)field.GetValue(null);
                if (opcode.OpCodeType == OpCodeType.Nternal)
                    continue;

                if (opcode.Size == 1)
                    OneByteCodes[opcode.Value] = opcode;
                else
                    TwoByteCodes[opcode.Value & 0xff] = opcode;
            }
        }

        private readonly MethodBase _method;
        private readonly MethodBody _body;
        private readonly Module _module;
        private readonly Type[] _typeArguments;
        private readonly Type[] _methodArguments;
        private readonly ByteBuffer _il;
        private readonly ParameterInfo[] _parameters;
        private readonly IList<LocalVariableInfo> _locals;
        private readonly List<Instruction> _instructions;

        private MethodBodyReader(MethodBase method)
        {
            _method = method;
            _body = method.GetMethodBody();
            if (_body == null)
                throw new ArgumentException("Method has no body");

            var bytes = _body.GetILAsByteArray();
            if (bytes == null)
                throw new ArgumentException("Can not get the body of the method");

            if (!(method is ConstructorInfo))
                _methodArguments = method.GetGenericArguments();

            if (method.DeclaringType != null)
                _typeArguments = method.DeclaringType.GetGenericArguments();

            _parameters = method.GetParameters();
            _locals = _body.LocalVariables;
            _module = method.Module;
            _il = new ByteBuffer(bytes);
            _instructions = new List<Instruction>((bytes.Length + 1) / 2);
        }

        public static MethodBodyReader Read(MethodBase method)
        {
            var reader = new MethodBodyReader(method);
            reader.ParseBody();
            return reader;
        }

        private void ParseBody()
        {
            while (_il.Position < _il.Length)
            {
                var pos = _il.Position;
                byte op = _il.ReadByte();
                var opCode = op != 0xfe
                    ? OneByteCodes[op]
                    : TwoByteCodes[_il.ReadByte()];

                var instruction = new Instruction(pos, opCode);

                ReadOperand(instruction);
                _instructions.Add(instruction);
            }
            ResolveBranches();
        }
        private void ReadOperand(Instruction instruction)
        {
            switch (instruction.OpCode.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.InlineSwitch:
                    int length = _il.ReadInt32();
                    int base_offset = _il.Position + (4 * length);
                    int[] branches = new int[length];
                    for (int i = 0; i < length; i++)
                        branches[i] = _il.ReadInt32() + base_offset;

                    instruction.Operand = branches;
                    break;
                case OperandType.ShortInlineBrTarget:
                    instruction.Operand = (((sbyte)_il.ReadByte()) + _il.Position);
                    break;
                case OperandType.InlineBrTarget:
                    instruction.Operand = _il.ReadInt32() + _il.Position;
                    break;
                case OperandType.ShortInlineI:
                    if (instruction.OpCode == OpCodes.Ldc_I4_S)
                        instruction.Operand = (sbyte)_il.ReadByte();
                    else
                        instruction.Operand = _il.ReadByte();
                    break;
                case OperandType.InlineI:
                    instruction.Operand = _il.ReadInt32();
                    break;
                case OperandType.ShortInlineR:
                    instruction.Operand = _il.ReadSingle();
                    break;
                case OperandType.InlineR:
                    instruction.Operand = _il.ReadDouble();
                    break;
                case OperandType.InlineI8:
                    instruction.Operand = _il.ReadInt64();
                    break;
                case OperandType.InlineSig:
                    instruction.Operand = _module.ResolveSignature(_il.ReadInt32());
                    break;
                case OperandType.InlineString:
                    instruction.Operand = _module.ResolveString(_il.ReadInt32());
                    break;
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.InlineMethod:
                case OperandType.InlineField:
                    instruction.Operand = _module.ResolveMember(_il.ReadInt32(), _typeArguments, _methodArguments);
                    break;
                case OperandType.ShortInlineVar:
                    instruction.Operand = GetVariable(instruction, _il.ReadByte());
                    break;
                case OperandType.InlineVar:
                    instruction.Operand = GetVariable(instruction, _il.ReadInt16());
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        private void ResolveBranches()
        {
            foreach (var instruction in _instructions)
            {
                switch (instruction.OpCode.OperandType)
                {
                    case OperandType.ShortInlineBrTarget:
                    case OperandType.InlineBrTarget:
                        instruction.Operand = GetInstruction(_instructions, (int)instruction.Operand);
                        break;
                    case OperandType.InlineSwitch:
                        var offsets = (int[])instruction.Operand;
                        var branches = new Instruction[offsets.Length];
                        for (int j = 0; j < offsets.Length; j++)
                            branches[j] = GetInstruction(_instructions, offsets[j]);

                        instruction.Operand = branches;
                        break;
                }
            }
        }
        private static Instruction GetInstruction(List<Instruction> instructions, int offset)
        {
            var size = instructions.Count;
            if (offset < 0 || offset > instructions[size - 1].Offset)
                return null;

            int min = 0;
            int max = size - 1;
            while (min <= max)
            {
                int mid = min + ((max - min) / 2);
                var instruction = instructions[mid];
                var instruction_offset = instruction.Offset;

                if (offset == instruction_offset)
                    return instruction;

                if (offset < instruction_offset)
                    max = mid - 1;
                else
                    min = mid + 1;
            }
            return null;
        }
        private object GetVariable(Instruction instruction, int index)
        {
            return TargetsLocalVariable(instruction.OpCode)
                ? (object)_locals[index]
                : (object)GetParameter(index);
        }
        private static bool TargetsLocalVariable(OpCode opcode)
        {
            return opcode.Name.Contains("loc");
        }
        private ParameterInfo GetParameter(int index)
        {
            return _parameters[_method.IsStatic ? index : index - 1];
        }
    }
}
