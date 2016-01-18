using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CS.Util.Dynamic
{
    public class Instruction
    {
        public int Offset { get; internal set; }
        public OpCode OpCode { get; internal set; }
        public object Operand { get; internal set; }


        internal Instruction(int offset, OpCode opcode)
        {
            this.Offset = offset;
            this.OpCode = opcode;
        }

        public override string ToString()
        {
            var instruction = new StringBuilder();

            AppendLabel(instruction, this);
            instruction.Append(':');
            instruction.Append(' ');
            instruction.Append(OpCode.Name);

            if (Operand == null)
                return instruction.ToString();

            instruction.Append(' ');

            switch (OpCode.OperandType)
            {
                case OperandType.ShortInlineBrTarget:
                case OperandType.InlineBrTarget:
                    AppendLabel(instruction, (Instruction) Operand);
                    break;
                case OperandType.InlineSwitch:
                    var labels = (Instruction[]) Operand;
                    for (int i = 0; i < labels.Length; i++)
                    {
                        if (i > 0)
                            instruction.Append(',');
                        AppendLabel(instruction, labels[i]);
                    }
                    break;
                case OperandType.InlineString:
                    instruction.Append('\"');
                    instruction.Append(Operand);
                    instruction.Append('\"');
                    break;
                default:
                    instruction.Append(Operand);
                    break;
            }

            return instruction.ToString();
        }

        private void AppendLabel(StringBuilder builder, Instruction instruction)
        {
            builder.Append("IL_");
            builder.Append(instruction.Offset.ToString("x4"));
        }
    }
}