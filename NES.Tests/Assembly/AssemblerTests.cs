using NES._6502;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NES.Tests.Assembly
{
    public class AssemblerTests
    {
        private StringBuilder _sb;

        public AssemblerTests()
        {
            _sb = new StringBuilder();
        }

        [Fact]
        public void AssembleAdditionProgram()
        {
            _sb.AppendLine("lda #$ff");
            _sb.AppendLine("clc");
            _sb.AppendLine("adc $ff04");
            _sb.AppendLine("brk");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            foreach (InstructionAssembled i in programAssembled.Instructions)
            {
            }
        }

    }
}
