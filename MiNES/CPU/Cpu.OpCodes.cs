using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.CPU
{
    partial class Cpu
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
        public const string ISB_INSTRUCTION = "ISB";
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
        internal static readonly Instruction[] OpCodes = new Instruction[256]
             {
            new Instruction(BRK_INSTRUCTION, AddressingMode.Implied, 7), new Instruction(ORA_INSTRUCTION, AddressingMode.IndirectX, 6), null,  new Instruction(SLO_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ORA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ASL_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(SLO_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(PHP_INSTRUCTION, AddressingMode.Implied, 3), new Instruction(ORA_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ASL_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(ANC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(NOP_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ORA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ASL_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(SLO_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BPL_INSTRUCTION, AddressingMode.Relative, 2, true), new Instruction(ORA_INSTRUCTION, AddressingMode.IndirectY, 5, true), null, new Instruction(SLO_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ORA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ASL_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SLO_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(CLC_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(ORA_INSTRUCTION, AddressingMode.AbsoluteY, 4, true), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SLO_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(ORA_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(ASL_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(SLO_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(JSR_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(AND_INSTRUCTION, AddressingMode.IndirectX, 6), null,    new Instruction(RLA_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(BIT_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(AND_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ROL_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(RLA_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(PLP_INSTRUCTION, AddressingMode.Implied, 4), new Instruction(AND_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ROL_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(ANC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(BIT_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(AND_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ROL_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(RLA_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BMI_INSTRUCTION, AddressingMode.Relative, 2, true), new Instruction(AND_INSTRUCTION, AddressingMode.IndirectY, 5, true), null,    new Instruction(RLA_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(AND_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ROL_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(RLA_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SEC_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(AND_INSTRUCTION, AddressingMode.AbsoluteY, 4, true), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(RLA_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(AND_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(ROL_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(RLA_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(RTI_INSTRUCTION, AddressingMode.Implied, 6), new Instruction(EOR_INSTRUCTION, AddressingMode.IndirectX, 6), null,     new Instruction(SRE_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(EOR_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LSR_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(SRE_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(PHA_INSTRUCTION, AddressingMode.Implied, 3), new Instruction(EOR_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LSR_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(ALR_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(JMP_INSTRUCTION, AddressingMode.Absolute, 3), new Instruction(EOR_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LSR_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(SRE_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BVC_INSTRUCTION, AddressingMode.Relative, 2, true), new Instruction(EOR_INSTRUCTION, AddressingMode.IndirectY, 5, true), null, new Instruction(SRE_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(EOR_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LSR_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SRE_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(CLI_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(EOR_INSTRUCTION, AddressingMode.AbsoluteY, 4, true), new Instruction(NOP_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(SRE_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(EOR_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(LSR_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(SRE_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(RTS_INSTRUCTION, AddressingMode.Implied, 6), new Instruction(ADC_INSTRUCTION, AddressingMode.IndirectX, 6), null, new Instruction(RRA_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ADC_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ROR_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(RRA_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(PLA_INSTRUCTION, AddressingMode.Implied, 4), new Instruction(ADC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ROR_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(ARR_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(JMP_INSTRUCTION, AddressingMode.Indirect, 5), new Instruction(ADC_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ROR_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(RRA_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BVS_INSTRUCTION, AddressingMode.Relative, 2, true), new Instruction(ADC_INSTRUCTION, AddressingMode.IndirectY, 5, true), null,    new Instruction(RRA_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ADC_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ROR_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(RRA_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SEI_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(ADC_INSTRUCTION, AddressingMode.AbsoluteY, 4, true), new Instruction(NOP_INSTRUCTION, AddressingMode.Accumulator, 2), new Instruction(RRA_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(ADC_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(ROR_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(RRA_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(STA_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(SAX_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(STY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(STA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(STX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(SAX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(DEY_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(TXA_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(XAA_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(STY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(STA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(STX_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(SAX_INSTRUCTION, AddressingMode.Absolute, 4),
            new Instruction(BCC_INSTRUCTION, AddressingMode.Relative, 2, true), new Instruction(STA_INSTRUCTION, AddressingMode.IndirectY, 6), null,    new Instruction(AHX_INSTRUCTION, AddressingMode.IndirectY, 6), new Instruction(STY_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(STA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(STX_INSTRUCTION, AddressingMode.ZeroPageY, 4), new Instruction(SAX_INSTRUCTION, AddressingMode.ZeroPageY, 4), new Instruction(TYA_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(STA_INSTRUCTION, AddressingMode.AbsoluteY, 5), new Instruction(TXS_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(TAS_INSTRUCTION, AddressingMode.AbsoluteY, 5), new Instruction(SHY_INSTRUCTION, AddressingMode.AbsoluteX, 5), new Instruction(STA_INSTRUCTION, AddressingMode.AbsoluteX, 5), new Instruction(SHX_INSTRUCTION, AddressingMode.AbsoluteY, 5), new Instruction(AHX_INSTRUCTION, AddressingMode.AbsoluteY, 5),
            new Instruction(LDY_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(LDX_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LAX_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(LDY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LDA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LDX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LAX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(TAY_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(TAX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LAX_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LDY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LDA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LDX_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LAX_INSTRUCTION, AddressingMode.Absolute, 4),
            new Instruction(BCS_INSTRUCTION, AddressingMode.Relative, 2, true), new Instruction(LDA_INSTRUCTION, AddressingMode.IndirectY, 5, true), null,     new Instruction(LAX_INSTRUCTION, AddressingMode.IndirectY, 5, true), new Instruction(LDY_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LDA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LDX_INSTRUCTION, AddressingMode.ZeroPageY, 4), new Instruction(LAX_INSTRUCTION, AddressingMode.ZeroPageY, 4), new Instruction(CLV_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.AbsoluteY, 4, true), new Instruction(TSX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LAS_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(LDY_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(LDA_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(LDX_INSTRUCTION, AddressingMode.AbsoluteY, 4, true), new Instruction(LAX_INSTRUCTION, AddressingMode.AbsoluteY, 4, true),
            new Instruction(CPY_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(DCP_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(CPY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(CMP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(DEC_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(DCP_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(INY_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(DEX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(AXS_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(CPY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(CMP_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(DEC_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(DCP_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BNE_INSTRUCTION, AddressingMode.Relative, 2, true), new Instruction(CMP_INSTRUCTION, AddressingMode.IndirectY, 5, true), null, new Instruction(DCP_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(CMP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(DEC_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(DCP_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(CLD_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.AbsoluteY, 4, true), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(DCP_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(CMP_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(DEC_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(DCP_INSTRUCTION, AddressingMode.AbsoluteX, 7),
            new Instruction(CPX_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(NOP_INSTRUCTION, AddressingMode.Immediate, 2),  new Instruction(ISB_INSTRUCTION, AddressingMode.IndirectX, 8), new Instruction(CPX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(SBC_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(INC_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(ISB_INSTRUCTION, AddressingMode.ZeroPage, 5), new Instruction(INX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(CPX_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(SBC_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(INC_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(ISB_INSTRUCTION, AddressingMode.Absolute, 6),
            new Instruction(BEQ_INSTRUCTION, AddressingMode.Relative, 2, true), new Instruction(SBC_INSTRUCTION, AddressingMode.IndirectY, 5, true), null,     new Instruction(ISB_INSTRUCTION, AddressingMode.IndirectY, 8), new Instruction(NOP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(SBC_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(INC_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(ISB_INSTRUCTION, AddressingMode.ZeroPageX, 6), new Instruction(SED_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.AbsoluteY, 4, true), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(ISB_INSTRUCTION, AddressingMode.AbsoluteY, 7), new Instruction(NOP_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(SBC_INSTRUCTION, AddressingMode.AbsoluteX, 4, true), new Instruction(INC_INSTRUCTION, AddressingMode.AbsoluteX, 7), new Instruction(ISB_INSTRUCTION, AddressingMode.AbsoluteX, 7)
        };
        #endregion
    }
}
