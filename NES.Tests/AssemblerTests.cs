using NES._6502;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NES.Tests
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

            string[] expectedDump = new string[]
            {
                "a9", "ff", "18", "6d", "04", "ff", "00"
            };

            Assert.True(AreHexDumpEqual(programAssembled.HexadecimalDump, expectedDump));
        }

        [Fact]
        public void AddrModeImmediate()
        {
            _sb.AppendLine("lda #$d8");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.Immediate);
        }

        [Fact]
        public void AddrModeZeroPage()
        {
            _sb.AppendLine("lda $d8");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.ZeroPage);
        }

        [Fact]
        public void AddrModeZeroPageX()
        {
            _sb.AppendLine("lda $d8,x");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.ZeroPageX);
        }

        [Fact]
        public void AddrModeZeroPageY()
        {
            _sb.AppendLine("stx $d8,y");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.ZeroPageY);
        }

        [Fact]
        public void AddrModeAccumulatorExplicit()
        {
            _sb.AppendLine("asl a");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.Accumulator);
        }

        [Fact]
        public void AddrModeAccumulatorImplicit()
        {
            _sb.AppendLine("asl");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.Accumulator);
        }

        [Fact]
        public void AddrModeAbsolute()
        {
            _sb.AppendLine("adc $fdaa");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.Absolute);
        }

        [Fact]
        public void AddrModeAbsoluteX()
        {
            _sb.AppendLine("adc $fdaa,x");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.AbsoluteX);
        }

        [Fact]
        public void AddrModeAbsoluteY()
        {
            _sb.AppendLine("adc $fdaa,y");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.AbsoluteY);
        }

        [Fact]
        public void AddrModeIndirect()
        {
            _sb.AppendLine("jmp ($fdaa)");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.Indirect);
        }

        [Fact]
        public void AddrModeIndirectX()
        {
            _sb.AppendLine("adc ($fd,x)");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.IndirectX);
        }

        [Fact]
        public void AddrModeIndirectY()
        {
            _sb.AppendLine("adc ($fd),y");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.IndirectY);
        }

        [Fact]
        public void AddrModeRelative()
        {
            _sb.AppendLine("bvc $fd");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.Relative);
        }

        [Fact]
        public void AddrModeImplied()
        {
            _sb.AppendLine("sec");

            var programAssembled = Assembler.Assemble(_sb.ToString());

            Assert.True(programAssembled.Instructions.First().AddressingMode == AddressingMode.Implied);
        }

        private static bool AreHexDumpEqual(string[] dump1, string[] dump2)
        {
            if (dump1.Length != dump2.Length)
                return false;

            for (var i = 0; i < dump1.Length; i++)
                if (dump1[i] != dump2[i])
                    return false;

            return true;
        }
    }
}
