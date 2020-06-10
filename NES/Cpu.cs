using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NES.ByteExtensions;

namespace NES
{

    enum AddressingMode
    {
        ZeroPage,
        ZeroPageX,
        ZeroPageY,
        Immediate,
        Relative,
        Absolute,
        AbsoluteX,
        AbsoluteY,
        Indirect,
        IndirectX,
        IndirectY,
        Implied,
        Accumulator
    }

    /// <summary>
    /// The 6502 CPU.
    /// </summary>
    class Cpu
    {

        #region Mnemonics
        public const string ADC_INSTRUCTION = "ADC";
        public const string AND_INSTRUCTION = "AND";
        public const string ASL_INSTRUCTION = "ASL";
        public const string BCC_INSTRUCTION = "BCC";
        public const string BCS_INSTRUCTION = "BCS";
        public const string BEQ_INSTRUCTION = "BEQ";
        public const string BIT_INSTRUCTION = "BIT";
        public const string BMI_INSTRUCTION = "BMI";
        public const string BNE_INSTRUCTION = "BNE";
        public const string BPL_INSTRUCTION = "BPL";
        public const string BRK_INSTRUCTION = "BRK";
        public const string BVC_INSTRUCTION = "BVC";
        public const string BVS_INSTRUCTION = "BVS";
        public const string CLC_INSTRUCTION = "CLC";
        public const string CLD_INSTRUCTION = "CLD";
        public const string CLI_INSTRUCTION = "CLI";
        public const string CLV_INSTRUCTION = "CLV";
        public const string CMP_INSTRUCTION = "CMP";
        public const string CPX_INSTRUCTION = "CPX";
        public const string CPY_INSTRUCTION = "CPY";
        public const string DEC_INSTRUCTION = "DEC";
        public const string DEX_INSTRUCTION = "DEX";
        public const string DEY_INSTRUCTION = "DEY";
        public const string EOR_INSTRUCTION = "EOR";
        public const string INC_INSTRUCTION = "INC";
        public const string INX_INSTRUCTION = "INX";
        public const string INY_INSTRUCTION = "INY";
        public const string JMP_INSTRUCTION = "JMP";
        public const string JSR_INSTRUCTION = "JSR";
        public const string LDA_INSTRUCTION = "LDA";
        public const string LDX_INSTRUCTION = "LDX";
        public const string LDY_INSTRUCTION = "LDY";
        public const string LSR_INSTRUCTION = "LSR";
        public const string NOP_INSTRUCTION = "NOP";
        public const string ORA_INSTRUCTION = "ORA";
        public const string PHA_INSTRUCTION = "PHA";
        public const string PHP_INSTRUCTION = "PHP";
        public const string PLA_INSTRUCTION = "PLA";
        public const string PLP_INSTRUCTION = "PLP";
        public const string ROL_INSTRUCTION = "ROL";
        public const string ROR_INSTRUCTION = "ROR";
        public const string RTI_INSTRUCTION = "RTI";
        public const string RTS_INSTRUCTION = "RTS";
        public const string SBC_INSTRUCTION = "SBC";
        public const string SEC_INSTRUCTION = "SEC";
        public const string SED_INSTRUCTION = "SED";
        public const string SEI_INSTRUCTION = "SEI";
        public const string STA_INSTRUCTION = "STA";
        public const string STX_INSTRUCTION = "STX";
        public const string STY_INSTRUCTION = "STY";
        public const string TAX_INSTRUCTION = "TAX";
        public const string TAY_INSTRUCTION = "TAY";
        public const string TSX_INSTRUCTION = "TSX";
        public const string TXA_INSTRUCTION = "TXA";
        public const string TXS_INSTRUCTION = "TXS";
        public const string TYA_INSTRUCTION = "TYA";

        // Unofficial opcodes
        public const string ARR_INSTRUCTION = "ARR";
        public const string ATX_INSTRUCTION = "ATX";
        public const string AXS_INSTRUCTION = "AXS";
        public const string DCP_INSTRUCTION = "DCP";
        public const string ISC_INSTRUCTION = "ISC";
        //public const string KIL_INSTRUCTION = "KIL";
        public const string LAX_INSTRUCTION = "LAX";
        public const string RLA_INSTRUCTION = "RLA";
        public const string RRA_INSTRUCTION = "RRA";
        public const string SLO_INSTRUCTION = "SLO";
        public const string SRE_INSTRUCTION = "SRE";
        public const string XAA_INSTRUCTION = "XAA";
        public const string ANC_INSTRUCTION = "ANC";
        public const string SAX_INSTRUCTION = "SAX";
        public const string SHX_INSTRUCTION = "SHX";
        public const string AHX_INSTRUCTION = "AHX";
        public const string ALR_INSTRUCTION = "ALR";
        public const string TAS_INSTRUCTION = "TAS";
        public const string SHY_INSTRUCTION = "SHY";
        public const string LAS_INSTRUCTION = "LAS";
        #endregion

        #region Instruction Set Operation Codes Matrix
        public static readonly Instruction[] OpCodes = new Instruction[256]
             {
            new Instruction(BRK_INSTRUCTION, AddressingMode.Implied, 7), new Instruction(ORA_INSTRUCTION, AddressingMode.IndirectX, 6), null,  new Instruction(SLO_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ORA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ASL_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(SLO_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(PHP_INSTRUCTION, AddressingMode.Implied, 3), new Instruction(ORA_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ASL_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(ANC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(NOP_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ORA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ASL_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(SLO_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BPL_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(ORA_INSTRUCTION, AddressingMode.IndirectY, 5), null, new Instruction(SLO_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ORA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ASL_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SLO_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(CLC_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(ORA_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SLO_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(ORA_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(ASL_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(SLO_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(JSR_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(AND_INSTRUCTION, AddressingMode.IndirectX, 6), null,    new Instruction(RLA_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(BIT_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(AND_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ROL_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(RLA_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(PLP_INSTRUCTION, AddressingMode.Implied, 4), new Instruction(AND_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ROL_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(ANC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(BIT_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(AND_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ROL_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(RLA_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BMI_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(AND_INSTRUCTION, AddressingMode.IndirectY, 5), null,    new Instruction(RLA_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(AND_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ROL_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(RLA_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SEC_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(AND_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(RLA_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(AND_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(ROL_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(RLA_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(RTI_INSTRUCTION, AddressingMode.Implied, 6), new Instruction(EOR_INSTRUCTION, AddressingMode.IndirectX, 6), null,     new Instruction(SRE_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(EOR_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LSR_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(SRE_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(PHA_INSTRUCTION, AddressingMode.Implied, 3), new Instruction(EOR_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LSR_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(ALR_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(JMP_INSTRUCTION, AddressingMode.Absolute, 3), new Instruction(EOR_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LSR_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(SRE_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BVC_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(EOR_INSTRUCTION, AddressingMode.IndirectY, 5), null, new Instruction(SRE_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(EOR_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LSR_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SRE_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(CLI_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(EOR_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(NOP_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(SRE_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(EOR_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(LSR_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(SRE_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(RTS_INSTRUCTION, AddressingMode.Implied, 6), new Instruction(ADC_INSTRUCTION, AddressingMode.IndirectX, 6), null, new Instruction(RRA_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ADC_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ROR_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(RRA_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(PLA_INSTRUCTION, AddressingMode.Implied, 4), new Instruction(ADC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ROR_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(ARR_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(JMP_INSTRUCTION, AddressingMode.Indirect, 5), new Instruction(ADC_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ROR_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(RRA_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BVS_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(ADC_INSTRUCTION, AddressingMode.IndirectY, 5), null,    new Instruction(RRA_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ADC_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ROR_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(RRA_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SEI_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(ADC_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(NOP_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(RRA_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(ADC_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(ROR_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(RRA_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(STA_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(SAX_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(STY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(STA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(STX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(SAX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(DEY_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(TXA_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(XAA_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(STY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(STA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(STX_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(SAX_INSTRUCTION, AddressingMode.Absolute, 4),
            new Instruction(BCC_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(STA_INSTRUCTION, AddressingMode.IndirectY, 6), null,    new Instruction(AHX_INSTRUCTION, AddressingMode.IndirectY, 6), new Instruction(STY_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(STA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(STX_INSTRUCTION, AddressingMode.ZeroPageY, 4), new Instruction(SAX_INSTRUCTION, AddressingMode.ZeroPageY, 4), new Instruction(TYA_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(STA_INSTRUCTION, AddressingMode.AbsoluteY, 5), new Instruction(TXS_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(TAS_INSTRUCTION, AddressingMode.AbsoluteY, 5), new Instruction(SHY_INSTRUCTION, AddressingMode.AbsoluteX, 5), new Instruction(STA_INSTRUCTION, AddressingMode.AbsoluteX, 5), new Instruction(SHX_INSTRUCTION, AddressingMode.AbsoluteY, 5), new Instruction(AHX_INSTRUCTION, AddressingMode.AbsoluteY, 5),
            new Instruction(LDY_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(LDX_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LAX_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(LDY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LDA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LDX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LAX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(TAY_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(TAX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LAX_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LDY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LDA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LDX_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LAX_INSTRUCTION, AddressingMode.Absolute, 4),
            new Instruction(BCS_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.IndirectY, 5), null,     new Instruction(LAX_INSTRUCTION, AddressingMode.IndirectY, 5), new Instruction(LDY_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LDA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LDX_INSTRUCTION, AddressingMode.ZeroPageY, 4), new Instruction(LAX_INSTRUCTION, AddressingMode.ZeroPageY, 4), new Instruction(CLV_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(TSX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LAS_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(LDY_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(LDA_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(LDX_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(LAX_INSTRUCTION, AddressingMode.AbsoluteY, 4),
            new Instruction(CPY_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(DCP_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(CPY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(CMP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(DEC_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(DCP_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(INY_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(DEX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(AXS_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(CPY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(CMP_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(DEC_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(DCP_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BNE_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.IndirectY, 5), null, new Instruction(DCP_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(CMP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(DEC_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(DCP_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(CLD_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(DCP_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(CMP_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(DEC_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(DCP_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(CPX_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2),  new Instruction(ISC_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(CPX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(SBC_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(INC_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(ISC_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(INX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(CPX_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(SBC_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(INC_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(ISC_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BEQ_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.IndirectY, 5), null,     new Instruction(ISC_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(SBC_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(INC_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(ISC_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SED_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(ISC_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(SBC_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(INC_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(ISC_INSTRUCTION, AddressingMode.AbsoluteX, 7)
        };
        #endregion

        /// <summary>
        /// Accumulators.
        /// </summary>
        private readonly Register<byte> _a = new Register<byte>();

        /// <summary>
        /// X register (general purpose).
        /// </summary>
        private readonly Register<byte> _x = new Register<byte>();

        /// <summary>
        /// Y register (general purpose).
        /// </summary>
        private readonly Register<byte> _y = new Register<byte>();

        /// <summary>
        /// Status register (each bit represents a flag).
        /// </summary>
        private readonly Flags _flags = new Flags();

        /// <summary>
        /// Holds the address of the outer 
        /// </summary>
        private readonly Register<byte> _stackPointer;

        /// <summary>
        /// The Program Counter register (holds the memory address of the next instruction).
        /// </summary>
        private readonly Register<ushort> _programCounter;

        /// <summary>
        /// The CPU bus (interacts with other components within the NES).
        /// </summary>
        private readonly CpuBus _bus;

        private ushort _pcAddress => _programCounter.GetValue();

        /// <summary>
        /// Instruction's operand memory address (the location in memory where resides the instruction's operand).
        /// <remarks>
        /// For instance, for immediate addressing mode, it stores the adddres where it should pickup the value.
        /// </remarks>
        /// </summary>
        private ushort _operandAddress;

#if CPU_NES_TEST
        private readonly List<byte> _instructionHex = new List<byte>();

        public string TestLineResult { get; private set; }

        private string GetRegistersSnapshot()
        {
            return $"A:{FormatByte(_a.GetValue())} X:{FormatByte(_x.GetValue())} Y:{FormatByte(_y.GetValue())} P:{FormatByte(_flags.GetValue())} SP:{FormatByte(_stackPointer.GetValue())}";
        }

        private string ParseInstruction(Instruction instruction)
        {
            var instructionSb = new StringBuilder(instruction.Mnemonic);

            if (!(instruction.AddressingMode == AddressingMode.Accumulator || instruction.AddressingMode == AddressingMode.Implied))
            {
                var operandParsed = new StringBuilder($"${FetchOperand()}");
                if (instruction.AddressingMode == AddressingMode.Immediate)
                    operandParsed.Insert(0, '#');
                else if (instruction.AddressingMode == AddressingMode.AbsoluteX || instruction.AddressingMode == AddressingMode.ZeroPageX)
                    operandParsed.Append(",X");
                else if (instruction.AddressingMode == AddressingMode.AbsoluteY || instruction.AddressingMode == AddressingMode.ZeroPageY)
                    operandParsed.Append(",Y");
                else if (instruction.AddressingMode == AddressingMode.Indirect)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append(')');
                }
                else if (instruction.AddressingMode == AddressingMode.IndirectX)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append(",X)");
                }
                else if (instruction.AddressingMode == AddressingMode.IndirectY)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append("),Y");
                }

                instructionSb.Append($" {operandParsed.ToString()}");
            }

            string FetchOperand()
            {
                ushort val = _operandAddress;
                if (instruction.AddressingMode == AddressingMode.Immediate)
                    val = _bus.Read(_operandAddress);
                else if (instruction.AddressingMode == AddressingMode.Relative)
                    val = (ushort)(_pcAddress + (sbyte)(_bus.Read(_operandAddress))); // Perform the addition no matter what the condition result

                return ParseOperand(val, instruction.AddressingMode);
            }

            return instructionSb.ToString();
        }

        private static string ParseOperand(ushort operand, AddressingMode addressingMode)
        {
            string op = operand.ToString("X");

            // This representation is not accurate because it attempts to follow the format of the nes_cpu_test.log
            switch (addressingMode)
            {
                // Addressing modes whose represetantion must be 2 bytes
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.Indirect:
                case AddressingMode.Relative:
                    op = op.PadLeft(4, '0');
                    break;
                // Addressing modes whose represetantion must be 1 byte
                case AddressingMode.Immediate:
                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageX:
                case AddressingMode.ZeroPageY:
                case AddressingMode.IndirectX:
                case AddressingMode.IndirectY:
                    op = op.PadLeft(2, '0');
                    break;
            }

            return op;
        }

        private static string FormatByte(byte b) => b.ToString("X").PadLeft(2, '0');
#endif

        /// <summary>
        /// Creates an instance of the 6502 CPU.
        /// </summary>
        /// <param name="bus">The CPU bus (used for interact with other components within the NES).</param>
        public Cpu(CpuBus bus, ushort startingAddress = 0x0000)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            _bus = bus;
            _stackPointer = new Register<byte>(0xFF);
            _programCounter = new Register<ushort>(startingAddress);

            Reset();
        }

        private void Reset()
        {
#if CPU_NES_TEST
            _flags.SetValue(0x0024); // ‭0010 0100‬
#else
            _flags.SetValue(0x0034); // 00‭11 0100‬
#endif
            Push(_pcAddress.GetHighByte());
            Push(_pcAddress.GetLowByte());
        }

        /// <summary>
        /// Runs the program loaded in the memory.
        /// </summary>
        public void Run()
        {
            do
            {

            } while (ExecuteInstruction());
        }

        /// <summary>
        /// Executes the program instruction by instruction (useful for debugging, or for execute the first N instructions of a program).
        /// </summary>
        /// <returns>True if there are more instruction to executed; otherwise false.</returns>
        public bool StepInstruction() => ExecuteInstruction();

        /// <summary>
        /// Executes a CPU instruction.
        /// </summary>
        private bool ExecuteInstruction()
        {
            // Fetches the OpCode from the memory
            byte opCode = _bus.Read(_pcAddress);

#if CPU_NES_TEST
            _instructionHex.Add(opCode);
            ushort opCodeAddress = _pcAddress;
            string registersSnapshot = GetRegistersSnapshot();
#endif

            // Advances either to the current instruction operand or next instruction (depending on the addressing mode of the instruction)
            IncrementPC();

            //Instruction instruction = OpCodes[opCode];
            Instruction instruction = OpCodes[opCode];
            if (instruction == null) // Illegal opcode
                return true;

            FetchOperand(instruction.AddressingMode);

#if CPU_NES_TEST
            string instructionDisassembled = ParseInstruction(instruction);
            string instructionHexDump = string.Join(" ", _instructionHex.Select(i => i.ToString("X").PadLeft(2, '0')));

            //TestLineResult = $"{opCodeAddress.ToString("X")}  {instructionHexDump.PadRight(10, ' ')}{instructionDisassembled.PadRight(32, ' ')}{registersSnapshot}";
            TestLineResult = $"{opCodeAddress.ToString("X").PadLeft(4, '0')} {instructionHexDump.PadRight(10, ' ')}{registersSnapshot}";
            _instructionHex.Clear();
#endif

            // Executes the instruction based on its mnemonic code
            switch (instruction.Mnemonic)
            {
                case ADC_INSTRUCTION:
                    ADC();
                    break;
                case AND_INSTRUCTION:
                    AND();
                    break;
                case ASL_INSTRUCTION:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ASL_ACC();
                    else
                        ASL();
                    break;
                case BCC_INSTRUCTION:
                    BCC();
                    break;
                case BCS_INSTRUCTION:
                    BCS();
                    break;
                case BEQ_INSTRUCTION:
                    BEQ();
                    break;
                case BIT_INSTRUCTION:
                    BIT();
                    break;
                case BMI_INSTRUCTION:
                    BMI();
                    break;
                case BNE_INSTRUCTION:
                    BNE();
                    break;
                case BPL_INSTRUCTION:
                    BPL();
                    break;
                case BRK_INSTRUCTION:
                    BRK();
                    //break;
                    return false;
                case BVC_INSTRUCTION:
                    BVC();
                    break;
                case BVS_INSTRUCTION:
                    BVS();
                    break;
                case CLC_INSTRUCTION:
                    CLC();
                    break;
                case CLD_INSTRUCTION:
                    CLD();
                    break;
                case CLI_INSTRUCTION:
                    CLI();
                    break;
                case CLV_INSTRUCTION:
                    CLV();
                    break;
                case CMP_INSTRUCTION:
                    CMP();
                    break;
                case CPX_INSTRUCTION:
                    CPX();
                    break;
                case CPY_INSTRUCTION:
                    CPY();
                    break;
                case DEC_INSTRUCTION:
                    DEC();
                    break;
                case DEX_INSTRUCTION:
                    DEX();
                    break;
                case DEY_INSTRUCTION:
                    DEY();
                    break;
                case EOR_INSTRUCTION:
                    EOR();
                    break;
                case INC_INSTRUCTION:
                    INC();
                    break;
                case INX_INSTRUCTION:
                    INX();
                    break;
                case INY_INSTRUCTION:
                    INY();
                    break;
                case JMP_INSTRUCTION:
                    JMP();
                    break;
                case JSR_INSTRUCTION:
                    JSR();
                    break;
                case LDA_INSTRUCTION:
                    LDA();
                    break;
                case LDX_INSTRUCTION:
                    LDX();
                    break;
                case LDY_INSTRUCTION:
                    LDY();
                    break;
                case LSR_INSTRUCTION:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        LSR_ACC();
                    else
                        LSR();
                    break;
                case NOP_INSTRUCTION:
                    NOP();
                    break;
                case ORA_INSTRUCTION:
                    ORA();
                    break;
                case PHA_INSTRUCTION:
                    PHA();
                    break;
                case PHP_INSTRUCTION:
                    PHP();
                    break;
                case PLA_INSTRUCTION:
                    PLA();
                    break;
                case PLP_INSTRUCTION:
                    PLP();
                    break;
                case ROL_INSTRUCTION:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ROL_ACC();
                    else
                        ROL();
                    break;
                case ROR_INSTRUCTION:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ROR_ACC();
                    else
                        ROR();
                    break;
                case RTI_INSTRUCTION:
                    RTI();
                    break;
                case RTS_INSTRUCTION:
                    RTS();
                    break;
                case SBC_INSTRUCTION:
                    SBC();
                    break;
                case SEC_INSTRUCTION:
                    SEC();
                    break;
                case SED_INSTRUCTION:
                    SED();
                    break;
                case SEI_INSTRUCTION:
                    SEI();
                    break;
                case STA_INSTRUCTION:
                    STA();
                    break;
                case STX_INSTRUCTION:
                    STX();
                    break;
                case STY_INSTRUCTION:
                    STY();
                    break;
                case TAX_INSTRUCTION:
                    TAX();
                    break;
                case TAY_INSTRUCTION:
                    TAY();
                    break;
                case TSX_INSTRUCTION:
                    TSX();
                    break;
                case TXA_INSTRUCTION:
                    TXA();
                    break;
                case TXS_INSTRUCTION:
                    TXS();
                    break;
                case TYA_INSTRUCTION:
                    TYA();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return true;
        }

        /// <summary>
        /// Transfers the content of the register Y to the Accumulator.
        /// </summary>
        private void TYA()
        {
            byte val = _y.GetValue();

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _a.SetValue(val);
        }

        /// <summary>
        /// Transfers the content of the register X to Stack Pointer register.
        /// </summary>
        private void TXS()
        {
            _stackPointer.SetValue(_x.GetValue());
        }

        /// <summary>
        /// Transfers the content of the register X to the Accumulator.
        /// </summary>
        private void TXA()
        {
            byte val = _x.GetValue();

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _a.SetValue(_x.GetValue());
        }

        /// <summary>
        /// Transfers the content of the Stack Pointer register to the X register.
        /// </summary>
        private void TSX()
        {
            byte val = _stackPointer.GetValue();

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _x.SetValue(val);
        }

        /// <summary>
        /// Transfers the content of Acummulator to the register Y.
        /// </summary>
        private void TAY()
        {
            byte val = _a.GetValue();

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _y.SetValue(val);
        }

        /// <summary>
        /// Transfers the content of Acummulator to the register X.
        /// </summary>
        private void TAX()
        {
            byte val = _a.GetValue();

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _x.SetValue(val);
        }

        /// <summary>
        /// Stores the content of the register Y to a memory slot.
        /// </summary>
        private void STY()
        {
            _bus.Write(_operandAddress, _y.GetValue());
        }

        /// <summary>
        /// Stores the content of the register Y to a memory slot.
        /// </summary>
        private void STX()
        {
            _bus.Write(_operandAddress, _x.GetValue());
        }

        /// <summary>
        /// Sets the disable interrupt flag.
        /// </summary>
        private void SEI()
        {
            _flags.SetFlag(StatusFlag.DisableInterrupt, true);
        }

        /// <summary>
        /// Sets the decimal flag.
        /// </summary>
        private void SED()
        {
            _flags.SetFlag(StatusFlag.Decimal, true);
        }

        /// <summary>
        /// Pops/pulls the processor flags and the program counter from the stack (first pop is the processor flags, then the next 2 pops operations 
        /// are for fetch the low byte and high byte of the program counter).
        /// </summary>
        private void RTI()
        {
            byte flags = Pop();
            _flags.SetValue(flags);

            byte pcLowByte = Pop();
            byte pcHighByte = Pop();
            _programCounter.SetValue(ParseBytes(pcLowByte, pcHighByte));

        }

        /// <summary>
        /// Pops the low byte and high byte of the program counter (set when jumping into a routine).
        /// </summary>
        private void RTS()
        {
            byte pcLowByte = Pop();
            byte pcHighByte = Pop();

            ushort pcAddress = ParseBytes(pcLowByte, pcHighByte);
            _programCounter.SetValue((ushort)(pcAddress + 1));
            //_programCounter.SetValue(pcAddress);
        }

        /// <summary>
        /// Pops/pulls the processor status flags from the stack.
        /// </summary>
        private void PLP()
        {
            _flags.SetValue(Pop());
        }

        /// <summary>
        /// Pops/pulls a byte from the stack and store into the Accumulator.
        /// </summary>
        private void PLA()
        {
            byte val = Pop();

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _a.SetValue(val);
        }

        /// <summary>
        /// Pushes a copy of the processor status flag into the stack.
        /// </summary>
        private void PHP()
        {
            /*
             * The copy of the CPU flags that would be pushed onto the stack will have set the bits 4 and 5
             * (this setting it's only to value that would be pushed onto the stack, not in the actual CPU flags).
             * Source: https://stackoverflow.com/questions/52017657/6502-emulator-testing-nestest 
             */
            byte flags = _flags.GetValue();
            flags |= 0x30; // Sets the bit 4 and 5 to the copy of the CPU flags

            Push(flags);
        }

        /// <summary>
        /// Pushes a copy of the accumulator into the stack.
        /// </summary>
        private void PHA()
        {
            Push(_a.GetValue());
        }

        /// <summary>
        /// Doesn't do anything.
        /// </summary>
        private void NOP()
        {
            //IncrementPC();
        }

        /// <summary>
        /// Loads a value into the register Y.
        /// </summary>
        private void LDY()
        {
            byte val = _bus.Read(_operandAddress);

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _y.SetValue(val);
        }

        /// <summary>
        /// Loads a value into the register X.
        /// </summary>
        private void LDX()
        {
            byte val = _bus.Read(_operandAddress);

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _x.SetValue(val);
        }

        /// <summary>
        /// Pushes the program counter address minus one into the stack and sets the program counter to the target memory address.
        /// </summary>
        private void JSR()
        {
            // The PC at this point points to the next instruction
            //ushort returnAddress = _programCounter.GetValue();
            ushort returnAddress = (ushort)(_programCounter.GetValue() - 1);

            // Pushes the high byte
            Push(returnAddress.GetHighByte());

            // Pushes the low byte
            Push(returnAddress.GetLowByte());

            _programCounter.SetValue(_operandAddress);
        }

        /// <summary>
        /// Sets the program counter to the address specified in the JMP instruction.
        /// </summary>
        private void JMP()
        {
            _programCounter.SetValue(_operandAddress);
        }

        /// <summary>
        /// Increments the register Y by one.
        /// </summary>
        private void INY()
        {
            byte val = (byte)(_y.GetValue() + 1);

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _y.SetValue(val);
        }

        /// <summary>
        /// Increments the register X by one.
        /// </summary>
        private void INX()
        {
            byte val = (byte)(_x.GetValue() + 1);

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _x.SetValue(val);
        }

        /// <summary>
        /// Increments by one the value allocated in the memory adddress specified.
        /// </summary>
        private void INC()
        {
            byte val = (byte)(_bus.Read(_operandAddress) + 1);

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _bus.Write(_operandAddress, val);
        }

        /// <summary>
        /// Decrements the register Y by one.
        /// </summary>
        private void DEY()
        {
            byte val = (byte)(_y.GetValue() - 1);

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _y.SetValue(val);
        }

        /// <summary>
        /// Decrements the register X by one.
        /// </summary>
        private void DEX()
        {
            byte val = (byte)(_x.GetValue() - 1);

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _x.SetValue(val);
        }

        /// <summary>
        /// Decrements by one the value allocated in the memory address specified.
        /// </summary>
        private void DEC()
        {
            byte val = (byte)(_bus.Read(_operandAddress) - 1);

            _flags.SetFlag(StatusFlag.Zero, val == 0);
            _flags.SetFlag(StatusFlag.Negative, val.IsNegative());

            _bus.Write(_operandAddress, val);
        }

        /// <summary>
        /// Compares the content of the register Y against a value held in memory.
        /// </summary>
        private void CPY()
        {
            Compare(_y.GetValue(), _bus.Read(_operandAddress));
        }

        /// <summary>
        /// Compares the content of the register X against a value held in memory.
        /// </summary>
        private void CPX()
        {
            Compare(_x.GetValue(), _bus.Read(_operandAddress));
        }

        /// <summary>
        /// Compares the content of the accumulator against a value held in memory.
        /// </summary>
        private void CMP()
        {
            Compare(_a.GetValue(), _bus.Read(_operandAddress));
        }

        /// <summary>
        /// Compares two bytes and updates the CPU flags based on the result.
        /// </summary>
        /// <param name="register">A value coming from a register (Accumulator, X, Y).</param>
        /// <param name="memory">A value held in memory.</param>
        private void Compare(byte register, byte memory)
        {
            _flags.SetFlag(StatusFlag.Carry, register >= memory);
            _flags.SetFlag(StatusFlag.Zero, register == memory);
            _flags.SetFlag(StatusFlag.Negative, ((byte)(register - memory)).IsNegative());
        }

        /// <summary>
        /// Clears the overflow flag (by setting to false).
        /// </summary>
        private void CLV()
        {
            _flags.SetFlag(StatusFlag.Overflow, false);
        }

        /// <summary>
        /// Clears the interrupt flag (by setting to false).
        /// </summary>
        private void CLI()
        {
            _flags.SetFlag(StatusFlag.DisableInterrupt, false);
        }

        /// <summary>
        /// Clears the decimal flag.
        /// </summary>
        private void CLD()
        {
            _flags.SetFlag(StatusFlag.Decimal, false);
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if overflow flag is set.
        /// </summary>
        private void BVS()
        {
            if (_flags.GetFlag(StatusFlag.Overflow))
                _programCounter.SetValue((ushort)(_programCounter.GetValue() + (sbyte)_bus.Read(_operandAddress)));
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if overflow flag is not set.
        /// </summary>
        private void BVC()
        {
            if (!_flags.GetFlag(StatusFlag.Overflow))
                _programCounter.SetValue((ushort)(_programCounter.GetValue() + (sbyte)_bus.Read(_operandAddress)));
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if negative flag is not set.
        /// </summary>
        private void BPL()
        {
            if (!_flags.GetFlag(StatusFlag.Negative))
                _programCounter.SetValue((ushort)(_programCounter.GetValue() + (sbyte)_bus.Read(_operandAddress)));
        }

        /// <summary>
        /// Forces CPU interruption.
        /// </summary>
        private void BRK()
        {
            byte lowByte = (byte)_pcAddress;
            byte highByte = (byte)(_pcAddress >> 8);

            // Pushes the program counter
            Push(highByte);
            Push(lowByte);

            // Pushes the CPU flags
            byte flags = _flags.GetValue();
            flags |= 0x30; // Sets the bit 4 and 5 to the copy of the CPU flags
            Push(flags);

            _flags.SetFlag(StatusFlag.DisableInterrupt, true);

            byte irqLowByte = _bus.Read(0xFFFE);
            byte irqHighByte = _bus.Read(0xFFFF);
            ushort address = ParseBytes(irqLowByte, irqHighByte);

            _programCounter.SetValue(address);
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if zero flag is not set.
        /// </summary>
        private void BNE()
        {
            if (!_flags.GetFlag(StatusFlag.Zero))
                _programCounter.SetValue((ushort)(_programCounter.GetValue() + (sbyte)_bus.Read(_operandAddress)));
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if negative flag is set.
        /// </summary>
        private void BMI()
        {
            if (_flags.GetFlag(StatusFlag.Negative))
                _programCounter.SetValue((ushort)(_programCounter.GetValue() + (sbyte)_bus.Read(_operandAddress)));
        }

        /// <summary>
        /// This instructions is used to test if one or more bits are set in a target memory location. The mask pattern in A is ANDed with the value 
        /// in memory to set or clear the zero flag, but the result is not kept. Bits 7 and 6 of the value from memory are copied into the N and V flags.
        /// (source: http://www.obelisk.me.uk/6502/reference.html#BIT)
        /// </summary>
        private void BIT()
        {
            byte accumulator = _a.GetValue();
            byte memory = _bus.Read(_operandAddress);

            _flags.SetFlag(StatusFlag.Zero, (accumulator & memory) == 0);
            _flags.SetFlag(StatusFlag.Overflow, (memory & 0x0040) == 0x0040); // bit no. 6 from the memory value
            _flags.SetFlag(StatusFlag.Negative, memory.IsNegative());
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if zero flag is set.
        /// </summary>
        private void BEQ()
        {
            if (_flags.GetFlag(StatusFlag.Zero))
                _programCounter.SetValue((ushort)(_programCounter.GetValue() + (sbyte)_bus.Read(_operandAddress)));
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if carry flag is set.
        /// </summary>
        private void BCS()
        {
            if (_flags.GetFlag(StatusFlag.Carry))
                _programCounter.SetValue((ushort)(_programCounter.GetValue() + (sbyte)_bus.Read(_operandAddress)));
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if carry flag is not set.
        /// </summary>
        private void BCC()
        {
            if (!_flags.GetFlag(StatusFlag.Carry))
                _programCounter.SetValue((ushort)(_programCounter.GetValue() + (sbyte)_bus.Read(_operandAddress)));
        }

        /// <summary>
        /// Adds a value to the accumulator's value.
        /// </summary>
        private void ADC()
        {            
            /*
             * Overflow happens when the result can't fit into a signed byte: RESULT < -128 OR RESULT > 127
             * In order to this happen, both operands should hava the same sign: same sign operands
             * produces a bigger magnitude (the magnitude of a signed number is the actual number next to the sign: +5 magnitude equals to 5).
             */

            byte accValue = _a.GetValue();
            byte val = _bus.Read(_operandAddress);

            int temp = accValue + val + (_flags.GetFlag(StatusFlag.Carry) ? 1 : 0);

            byte result = (byte)temp;

            // If result is greater than 255, enable the Carry flag
            _flags.SetFlag(StatusFlag.Carry, temp > 255);

            // If the bit no. 7 is set, then enable the Negative flag
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            // If result equals 0, enable the Zero flag
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            // If two numbers of the same sign produce a number whose sign is different, then there's an overflow
            _flags.SetFlag(StatusFlag.Overflow, ((accValue ^ result) & (val ^ result) & 0x0080) == 0x0080);

            _a.SetValue(result);
        }

        /// <summary>
        /// Substracts a value from the accumulator's value.
        /// </summary>

        private void SBC()
        {
            /* The one complement of a number N is defined as N - 255. In binary, this is achieved by
             * flipping each bit of the binary representation of N. The two complement is just the one complement
             * plus one: 2 complement = 1 complement + 1 = (255 - N) + 1 = 256 - N. The two complement is used
             * for represent negative numbers.
             * 
             * The substraction M - N can be represented as: M + (-N) = M + (256 - N) = M + (2 complement of N)
             * due to there's not a borrow flag, we use the carry flag as our borrow flag by using its complement: B = 1 - C
             * The substraction of N from M is expressed as:
             *  M - N - B = M + (-N) - (1 - C) = M + (2 complement of N) - 1 + C
             *  M + (256 - N) - 1 + C = M + (255 - N) + C = M + (1 complement of N) + C
             */

            /* Same story as ADC: overflow happens when the result can't fit into a signed byte: RESULT < -128 OR RESULT > 127
             * However, in substraction, the overflow would happen when both operands initially have different signs. The sign of substracting number
             * will change when taking its one complement (from (-) to (+), and (+) to (-)). For example, M = (+), N = (-), so when making the
             * substraction, we end with M + (-N) = M + (+N); it gets converted to (+) because the one complement of N. Another example would be, M = (-),
             * N = (+), so the substraction is -M + (+N) = -M + (-N); it gets converted to (-) because the one complement of N.
             */

            byte accValue = _a.GetValue();
            byte val = _bus.Read(_operandAddress);
            byte complement = (byte)(~val);

            int temp = accValue + complement + (_flags.GetFlag(StatusFlag.Carry) ? 1 : 0);
            byte result = (byte)temp;

            // If carry flag is set, it means a borror did not happen, otherwise it did happen
            _flags.SetFlag(StatusFlag.Carry, temp > 255);

            _flags.SetFlag(StatusFlag.Zero, result == 0);

            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            // In the substraction, the sign check is done in the complement
            _flags.SetFlag(StatusFlag.Overflow, ((accValue ^ result) & (complement ^ result) & 0x0080) == 0x0080);

            _a.SetValue(result);
        }

        /// <summary>                                                                                    
        /// Loads a value located in an address into the accumulator.
        /// </summary>
        private void LDA()
        {
            _a.SetValue(_bus.Read(_operandAddress));

            _flags.SetFlag(StatusFlag.Zero, _a.GetValue() == 0);
            _flags.SetFlag(StatusFlag.Negative, _a.GetValue().IsNegative());
        }

        /// <summary>
        /// Stores the accumulator value into memory.
        /// </summary>
        private void STA()
        {
            byte accValue = _a.GetValue();

            _bus.Write(_operandAddress, accValue);
        }

        /// <summary>
        /// Turn on the Carry Flag by setting 1.
        /// </summary>
        private void SEC()
        {
            _flags.SetFlag(StatusFlag.Carry, true);
        }

        /// <summary>
        /// Clears the Carry Flag by setting 0.
        /// </summary>
        private void CLC()
        {
            _flags.SetFlag(StatusFlag.Carry, false);
        }

        /// <summary>
        /// Shifts each bit of a memory content one place to the left.
        /// </summary>
        private void ASL()
        {
            byte val = _bus.Read(_operandAddress);
            
            byte result = ShiftLeft(val);

            _bus.Write(_operandAddress, result);
        }

        /// <summary>
        /// Shifts each bit of current accumulator value one place to the left.
        /// </summary>
        private void ASL_ACC()
        {
            byte val = _a.GetValue();
            
            byte result = ShiftLeft(val);

            _a.SetValue(result);
        }

        /// <summary>
        /// Performs a shift operation (left direction) in the given value (updates the CPU flags).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The value after performing the left shift.</returns>
        private byte ShiftLeft(byte val)
        {
            byte result = (byte)(val << 1);

            _flags.SetFlag(StatusFlag.Carry, (val & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            return result;
        }

        /// <summary>
        /// Shifts each bit of the memory content one place to the right.
        /// </summary>
        private void LSR()
        {
            byte val = _bus.Read(_operandAddress);
            
            byte result = ShiftRight(val);

            _bus.Write(_operandAddress, result);
        }

        /// <summary>
        /// Shifts each bit of current accumulator value one place to the right.
        /// </summary>
        private void LSR_ACC()
        {
            byte val = _a.GetValue();
            
            byte result = ShiftRight(val);

            _a.SetValue(result);
        }

        /// <summary>
        /// Performs a shift operation (right direction) in the given value (updates the CPU flags).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The value after performing the right shift.</returns>
        private byte ShiftRight(byte val)
        {
            byte result = (byte)(val >> 1);

            _flags.SetFlag(StatusFlag.Carry, (val & 0x01) == 0x01);
            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            return result;
        }

        /// <summary>
        /// Moves each of the bits of an memory content one place to the left.
        /// </summary>
        private void ROL()
        {
            byte val = _bus.Read(_operandAddress);
            
            byte result = RotateLeft(val);

            _bus.Write(_operandAddress, result);
        }

        /// <summary>
        /// Moves each of the bits of the accumulator value one place to the left.
        /// </summary>
        private void ROL_ACC()
        {
            byte val = _a.GetValue();
            
            byte result = RotateLeft(val);

            _a.SetValue(result);
        }

        /// <summary>
        /// "Rotates" (shift) to the left side the given value (updates the CPU flags).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The value after performing the left side rotation (shift).</returns>
        private byte RotateLeft(byte val)
        {
            int r = val << 1;
            r |= (_flags.GetFlag(StatusFlag.Carry) ? 0x01 : 0); // places the carry flag into the bit 0
            byte result = (byte)r;

            _flags.SetFlag(StatusFlag.Carry, (val & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            return result;
        }

        /// <summary>
        /// Moves each of the bits of an memory content one place to the right.
        /// </summary>
        private void ROR()
        {
            byte val = _bus.Read(_operandAddress);
            
            byte result = RotateRight(val);

            _bus.Write(_operandAddress, result);
        }

        /// <summary>
        /// Moves each of the bits of the accumulator value one place to the right.
        /// </summary>
        private void ROR_ACC()
        {
            byte val = _a.GetValue();
            
            byte result = RotateRight(val);

            _a.SetValue(result);
        }

        /// <summary>
        /// "Rotates" (shift) to the right side the given value (updates the CPU flags).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The value after performing the right side rotation (shift).</returns>
        private byte RotateRight(byte val)
        {
            int r = val >> 1;
            r |= (_flags.GetFlag(StatusFlag.Carry) ? 0x0080 : 0); // places the carry flag into bit no.7
            byte result = (byte)r;

            _flags.SetFlag(StatusFlag.Carry, (val & 0x01) == 0x01);
            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            return result;
        }

        /// <summary>
        /// Performs a logical AND operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void AND()
        {
            byte val = _bus.Read(_operandAddress);
            byte result = (byte)(val & _a.GetValue());

            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            _a.SetValue(result);
        }

        /// <summary>
        /// Performs a logical Exclusive OR (NOR) operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void EOR()
        {
            byte val = _bus.Read(_operandAddress);
            byte result = (byte)(val ^ _a.GetValue());

            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            _a.SetValue(result);
        }

        /// <summary>
        /// Performs a logical Inclusive OR operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void ORA()
        {
            byte val = _bus.Read(_operandAddress);
            byte result = (byte)(val | _a.GetValue());

            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            _a.SetValue(result);
        }

#region Addressing modes

        /// <summary>
        /// Fetchs the instruction operand based on the instruction addressing mode.
        /// </summary>
        /// <param name="mode">The instruction's addressing mode.</param>
        private void FetchOperand(AddressingMode mode)
        {
            if (mode == AddressingMode.Accumulator || mode == AddressingMode.Implied)
                return;

#if CPU_NES_TEST
            _instructionHex.Add(_bus.Read(_pcAddress));
#endif
            ushort operandAddress;
            switch (mode)
            {
                case AddressingMode.ZeroPage:
                    operandAddress = _bus.Read(_pcAddress);
                    break;
                case AddressingMode.ZeroPageX:
                    operandAddress = (byte)(_bus.Read(_pcAddress) + _x.GetValue()); // If carry in the high byte (result greater than 255), requires an additiona cycle
                    break;
                case AddressingMode.ZeroPageY:
                    operandAddress = (byte)(_bus.Read(_pcAddress) + _y.GetValue()); // If carry in the high byte (result greater than 255), requires an additiona cycle
                    break;
                case AddressingMode.Immediate:
                case AddressingMode.Relative:
                    operandAddress = _pcAddress;
                    break;
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.Indirect:
                    ushort addressParsed;
                    {
                        byte lowByte = _bus.Read(_pcAddress);

                        IncrementPC();

                        byte highByte = _bus.Read(_pcAddress);
#if CPU_NES_TEST
                        _instructionHex.Add(highByte);
#endif

                        addressParsed = ParseBytes(lowByte, highByte);
                    }
                    if (mode == AddressingMode.Indirect)
                    {
                        // The content located in the address parsed is the LSB (Least Significant Byte) of the target address
                        byte lowByte = _bus.Read(addressParsed);

                        /*
                         * There's a bug in the hardware when parsing the effective address in the Indirect
                         * addressing mode: if the LSB (least significant byte) of the absolute address is 0xFF, then incrementing
                         * by one the absolute address (incrementing by one is required for get the MSB of the effective address) 
                         * would produce a wrap around the page; example below:
                         * 
                         * Absolute address: 0x02FF.
                         * LSB from the effetive address is at 0x02FF.
                         * MSB from the effective address should be at 0x02FF + 0x0001 = 0x0300; but because the bug explained above, it's
                         * at 0x0200 (we stayed in the same page 0x02)
                         */
                        if (addressParsed.GetLowByte() == 0xFF)
                            addressParsed ^= (0x00FF);
                        else
                            addressParsed++;

                        byte highByte = _bus.Read(addressParsed);

                        operandAddress = ParseBytes(lowByte, highByte);
                    }
                    else if (mode == AddressingMode.Absolute)
                        operandAddress = addressParsed;
                    else if (mode == AddressingMode.AbsoluteX)
                        operandAddress = (ushort)(addressParsed + _x.GetValue());
                    else
                        operandAddress = (ushort)(addressParsed + _y.GetValue());
                    break;
                case AddressingMode.IndirectX:
                    {
                        byte address = (byte)(_bus.Read(_pcAddress) + _x.GetValue()); // Wraps around the zero page

                        byte lowByte = _bus.Read(address++);
                        byte highByte = _bus.Read(address);

                        operandAddress = ParseBytes(lowByte, highByte);
                    }
                    break;
                case AddressingMode.IndirectY:
                    {
                        byte zeroPageAddress = _bus.Read(_pcAddress);

                        byte lowByte = _bus.Read(zeroPageAddress++);
                        byte highByte = _bus.Read(zeroPageAddress);

                        operandAddress = (ushort)(ParseBytes(lowByte, highByte) + _y.GetValue());
                    }
                    break;
                default:
                    throw new NotImplementedException($"The given addressing mode is not supported: {mode}.");
            }

            _operandAddress = operandAddress;
            IncrementPC(); // Points to the next opcode (instruction)
        }

        #endregion

        /// <summary>
        /// Increments the address allocated in the Program Counter.
        /// </summary>
        private void IncrementPC()
        {
            _programCounter.SetValue((ushort)(_pcAddress + 1));
        }

        /// <summary>
        /// Parses the address for a stack operation (the stack uses the second page (page 1) from the CPU memory).
        /// </summary>
        /// <param name="stackPointerAddress">The address where the stack points.</param>
        /// <returns>An address somewhere in the page 1.</returns>
        private static ushort ParseStackAddress(byte stackPointerAddress)
        {
            return (ushort)(0x0100 | stackPointerAddress);
        }

        /// <summary>
        /// Pushes a value onto the stack (it updates the stack pointer after pushing the value).
        /// </summary>
        /// <param name="b">The value that would be pushed into the stack.</param>
        private void Push(byte b)
        {
            byte stackPointer = _stackPointer.GetValue();

            /* When pushing a byte onto the stack, the stack pointer is decremented by one (the stack pointer groes down). Also the stack pointer
             * points to the next available slot in the stack's memory page.
             */
            ushort address = ParseStackAddress(stackPointer--);

            _bus.Write(address, b);

            _stackPointer.SetValue(stackPointer);
        }

        /// <summary>
        /// Pops a value from the stack (it updates the stack pointer after popping the value).
        /// </summary>
        /// <returns>The top value in the stack.</returns>
        private byte Pop()
        {
            byte stackPointer = _stackPointer.GetValue();

            // When popping a byte from the stack, the stack pointer is incremented by one
            ushort address = ParseStackAddress(++stackPointer);

            byte val = _bus.Read(address);

            _stackPointer.SetValue(stackPointer);

            return val;
        }
    }

    /// <summary>
    /// Represents a CPU instruction (including its mnemonic, addressing mode and machine cycles).
    /// </summary>
    class Instruction
    {
        /// <summary>
        /// The mnemonic code that represents the underlying instruction.
        /// </summary>
        public string Mnemonic { get; private set; }

        /// <summary>
        /// The addressing mode of the instruction.
        /// </summary>
        public AddressingMode AddressingMode { get; private set; }

        /// <summary>
        /// The amount of machine cycles required in order to execute the instruction.
        /// </summary>
        public byte MachineCycles { get; private set; }

        public Instruction(string mnemonic, AddressingMode addressingMode, byte machineCycles)
        {
            Mnemonic = mnemonic;
            AddressingMode = addressingMode;
            MachineCycles = machineCycles;
        }

        public override string ToString() => $"{Mnemonic} {AddressingMode}";
    }
}
