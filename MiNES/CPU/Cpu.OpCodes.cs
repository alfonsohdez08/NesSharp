using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.CPU
{

    enum Mnemonic : byte
    {
        ADC,
        AND,
        ASL,
        BCC,
        BCS,
        BEQ,
        BIT,
        BMI,
        BNE,
        BPL,
        BRK,
        BVC,
        BVS,
        CLC,
        CLD,
        CLI,
        CLV,
        CMP,
        CPX,
        CPY,
        DEC,
        DEX,
        DEY,
        EOR,
        INC,
        INX,
        INY,
        JMP,
        JSR,
        LDA,
        LDX,
        LDY,
        LSR,
        NOP,
        ORA,
        PHA,
        PHP,
        PLA,
        PLP,
        ROL,
        ROR,
        RTI,
        RTS,
        SBC,
        SEC,
        SED,
        SEI,
        STA,
        STX,
        STY,
        TAX,
        TAY,
        TSX,
        TXA,
        TXS,
        TYA,
        ARR,
        ATX,
        AXS,
        DCP,
        ISB,
        LAX,
        RLA,
        RRA,
        SLO,
        SRE,
        XAA,
        ANC,
        SAX,
        SHX,
        AHX,
        ALR,
        TAS,
        SHY,
        LAS
    }

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
            new Instruction(Mnemonic.BRK, AddressingMode.Implied, 7), new Instruction(Mnemonic.ORA, AddressingMode.IndirectX, 6), null,  new Instruction(Mnemonic.SLO, AddressingMode.IndirectX, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.ORA, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.ASL, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.SLO, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.PHP, AddressingMode.Implied, 3), new Instruction(Mnemonic.ORA, AddressingMode.Immediate, 2), new Instruction(Mnemonic.ASL, AddressingMode.Accumulator, 2), new Instruction(Mnemonic.ANC, AddressingMode.Immediate, 2), new Instruction(Mnemonic.NOP, AddressingMode.Absolute, 4), new Instruction(Mnemonic.ORA, AddressingMode.Absolute, 4), new Instruction(Mnemonic.ASL, AddressingMode.Absolute, 6), new Instruction(Mnemonic.SLO, AddressingMode.Absolute, 6),
            new Instruction(Mnemonic.BPL, AddressingMode.Relative, 2, true), new Instruction(Mnemonic.ORA, AddressingMode.IndirectY, 5, true), null, new Instruction(Mnemonic.SLO, AddressingMode.IndirectY, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.ORA, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.ASL, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.SLO, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.CLC, AddressingMode.Implied, 2), new Instruction(Mnemonic.ORA, AddressingMode.AbsoluteY, 4, true), new Instruction(Mnemonic.NOP, AddressingMode.Implied, 2), new Instruction(Mnemonic.SLO, AddressingMode.AbsoluteY, 7), new Instruction(Mnemonic.NOP, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.ORA, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.ASL, AddressingMode.AbsoluteX, 7), new Instruction(Mnemonic.SLO, AddressingMode.AbsoluteX, 7),
            new Instruction(Mnemonic.JSR, AddressingMode.Absolute, 6), new Instruction(Mnemonic.AND, AddressingMode.IndirectX, 6), null,    new Instruction(Mnemonic.RLA, AddressingMode.IndirectX, 8), new Instruction(Mnemonic.BIT, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.AND, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.ROL, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.RLA, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.PLP, AddressingMode.Implied, 4), new Instruction(Mnemonic.AND, AddressingMode.Immediate, 2), new Instruction(Mnemonic.ROL, AddressingMode.Accumulator, 2), new Instruction(Mnemonic.ANC, AddressingMode.Immediate, 2), new Instruction(Mnemonic.BIT, AddressingMode.Absolute, 4), new Instruction(Mnemonic.AND, AddressingMode.Absolute, 4), new Instruction(Mnemonic.ROL, AddressingMode.Absolute, 6), new Instruction(Mnemonic.RLA, AddressingMode.Absolute, 6),
            new Instruction(Mnemonic.BMI, AddressingMode.Relative, 2, true), new Instruction(Mnemonic.AND, AddressingMode.IndirectY, 5, true), null,    new Instruction(Mnemonic.RLA, AddressingMode.IndirectY, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.AND, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.ROL, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.RLA, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.SEC, AddressingMode.Implied, 2), new Instruction(Mnemonic.AND, AddressingMode.AbsoluteY, 4, true), new Instruction(Mnemonic.NOP, AddressingMode.Implied, 2), new Instruction(Mnemonic.RLA, AddressingMode.AbsoluteY, 7), new Instruction(Mnemonic.NOP, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.AND, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.ROL, AddressingMode.AbsoluteX, 7), new Instruction(Mnemonic.RLA, AddressingMode.AbsoluteX, 7),
            new Instruction(Mnemonic.RTI, AddressingMode.Implied, 6), new Instruction(Mnemonic.EOR, AddressingMode.IndirectX, 6), null,     new Instruction(Mnemonic.SRE, AddressingMode.IndirectX, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.EOR, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.LSR, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.SRE, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.PHA, AddressingMode.Implied, 3), new Instruction(Mnemonic.EOR, AddressingMode.Immediate, 2), new Instruction(Mnemonic.LSR, AddressingMode.Accumulator, 2), new Instruction(Mnemonic.ALR, AddressingMode.Immediate, 2), new Instruction(Mnemonic.JMP, AddressingMode.Absolute, 3), new Instruction(Mnemonic.EOR, AddressingMode.Absolute, 4), new Instruction(Mnemonic.LSR, AddressingMode.Absolute, 6), new Instruction(Mnemonic.SRE, AddressingMode.Absolute, 6),
            new Instruction(Mnemonic.BVC, AddressingMode.Relative, 2, true), new Instruction(Mnemonic.EOR, AddressingMode.IndirectY, 5, true), null, new Instruction(Mnemonic.SRE, AddressingMode.IndirectY, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.EOR, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.LSR, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.SRE, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.CLI, AddressingMode.Implied, 2), new Instruction(Mnemonic.EOR, AddressingMode.AbsoluteY, 4, true), new Instruction(Mnemonic.NOP, AddressingMode.Accumulator, 2), new Instruction(Mnemonic.SRE, AddressingMode.AbsoluteY, 7), new Instruction(Mnemonic.NOP, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.EOR, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.LSR, AddressingMode.AbsoluteX, 7), new Instruction(Mnemonic.SRE, AddressingMode.AbsoluteX, 7),
            new Instruction(Mnemonic.RTS, AddressingMode.Implied, 6), new Instruction(Mnemonic.ADC, AddressingMode.IndirectX, 6), null, new Instruction(Mnemonic.RRA, AddressingMode.IndirectX, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.ADC, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.ROR, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.RRA, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.PLA, AddressingMode.Implied, 4), new Instruction(Mnemonic.ADC, AddressingMode.Immediate, 2), new Instruction(Mnemonic.ROR, AddressingMode.Accumulator, 2), new Instruction(Mnemonic.ARR, AddressingMode.Immediate, 2), new Instruction(Mnemonic.JMP, AddressingMode.Indirect, 5), new Instruction(Mnemonic.ADC, AddressingMode.Absolute, 4), new Instruction(Mnemonic.ROR, AddressingMode.Absolute, 6), new Instruction(Mnemonic.RRA, AddressingMode.Absolute, 6),
            new Instruction(Mnemonic.BVS, AddressingMode.Relative, 2, true), new Instruction(Mnemonic.ADC, AddressingMode.IndirectY, 5, true), null,    new Instruction(Mnemonic.RRA, AddressingMode.IndirectY, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.ADC, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.ROR, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.RRA, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.SEI, AddressingMode.Implied, 2), new Instruction(Mnemonic.ADC, AddressingMode.AbsoluteY, 4, true), new Instruction(Mnemonic.NOP, AddressingMode.Accumulator, 2), new Instruction(Mnemonic.RRA, AddressingMode.AbsoluteY, 7), new Instruction(Mnemonic.NOP, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.ADC, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.ROR, AddressingMode.AbsoluteX, 7), new Instruction(Mnemonic.RRA, AddressingMode.AbsoluteX, 7),
            new Instruction(Mnemonic.NOP, AddressingMode.Immediate, 2), new Instruction(Mnemonic.STA, AddressingMode.IndirectX, 6), new Instruction(Mnemonic.NOP, AddressingMode.Immediate, 2), new Instruction(Mnemonic.SAX, AddressingMode.IndirectX, 6), new Instruction(Mnemonic.STY, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.STA, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.STX, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.SAX, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.DEY, AddressingMode.Implied, 2), new Instruction(Mnemonic.NOP, AddressingMode.Immediate, 2), new Instruction(Mnemonic.TXA, AddressingMode.Implied, 2), new Instruction(Mnemonic.XAA, AddressingMode.Immediate, 2), new Instruction(Mnemonic.STY, AddressingMode.Absolute, 4), new Instruction(Mnemonic.STA, AddressingMode.Absolute, 4), new Instruction(Mnemonic.STX, AddressingMode.Absolute, 4), new Instruction(Mnemonic.SAX, AddressingMode.Absolute, 4),
            new Instruction(Mnemonic.BCC, AddressingMode.Relative, 2, true), new Instruction(Mnemonic.STA, AddressingMode.IndirectY, 6), null,    new Instruction(Mnemonic.AHX, AddressingMode.IndirectY, 6), new Instruction(Mnemonic.STY, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.STA, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.STX, AddressingMode.ZeroPageY, 4), new Instruction(Mnemonic.SAX, AddressingMode.ZeroPageY, 4), new Instruction(Mnemonic.TYA, AddressingMode.Implied, 2), new Instruction(Mnemonic.STA, AddressingMode.AbsoluteY, 5), new Instruction(Mnemonic.TXS, AddressingMode.Implied, 2), new Instruction(Mnemonic.TAS, AddressingMode.AbsoluteY, 5), new Instruction(Mnemonic.SHY, AddressingMode.AbsoluteX, 5), new Instruction(Mnemonic.STA, AddressingMode.AbsoluteX, 5), new Instruction(Mnemonic.SHX, AddressingMode.AbsoluteY, 5), new Instruction(Mnemonic.AHX, AddressingMode.AbsoluteY, 5),
            new Instruction(Mnemonic.LDY, AddressingMode.Immediate, 2), new Instruction(Mnemonic.LDA, AddressingMode.IndirectX, 6), new Instruction(Mnemonic.LDX, AddressingMode.Immediate, 2), new Instruction(Mnemonic.LAX, AddressingMode.IndirectX, 6), new Instruction(Mnemonic.LDY, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.LDA, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.LDX, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.LAX, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.TAY, AddressingMode.Implied, 2), new Instruction(Mnemonic.LDA, AddressingMode.Immediate, 2), new Instruction(Mnemonic.TAX, AddressingMode.Implied, 2), new Instruction(Mnemonic.LAX, AddressingMode.Immediate, 2), new Instruction(Mnemonic.LDY, AddressingMode.Absolute, 4), new Instruction(Mnemonic.LDA, AddressingMode.Absolute, 4), new Instruction(Mnemonic.LDX, AddressingMode.Absolute, 4), new Instruction(Mnemonic.LAX, AddressingMode.Absolute, 4),
            new Instruction(Mnemonic.BCS, AddressingMode.Relative, 2, true), new Instruction(Mnemonic.LDA, AddressingMode.IndirectY, 5, true), null,     new Instruction(Mnemonic.LAX, AddressingMode.IndirectY, 5, true), new Instruction(Mnemonic.LDY, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.LDA, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.LDX, AddressingMode.ZeroPageY, 4), new Instruction(Mnemonic.LAX, AddressingMode.ZeroPageY, 4), new Instruction(Mnemonic.CLV, AddressingMode.Implied, 2), new Instruction(Mnemonic.LDA, AddressingMode.AbsoluteY, 4, true), new Instruction(Mnemonic.TSX, AddressingMode.Implied, 2), new Instruction(Mnemonic.LAS, AddressingMode.AbsoluteY, 4), new Instruction(Mnemonic.LDY, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.LDA, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.LDX, AddressingMode.AbsoluteY, 4, true), new Instruction(Mnemonic.LAX, AddressingMode.AbsoluteY, 4, true),
            new Instruction(Mnemonic.CPY, AddressingMode.Immediate, 2), new Instruction(Mnemonic.CMP, AddressingMode.IndirectX, 6), new Instruction(Mnemonic.NOP, AddressingMode.Immediate, 2), new Instruction(Mnemonic.DCP, AddressingMode.IndirectX, 8), new Instruction(Mnemonic.CPY, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.CMP, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.DEC, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.DCP, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.INY, AddressingMode.Implied, 2), new Instruction(Mnemonic.CMP, AddressingMode.Immediate, 2), new Instruction(Mnemonic.DEX, AddressingMode.Implied, 2), new Instruction(Mnemonic.AXS, AddressingMode.Immediate, 2), new Instruction(Mnemonic.CPY, AddressingMode.Absolute, 4), new Instruction(Mnemonic.CMP, AddressingMode.Absolute, 4), new Instruction(Mnemonic.DEC, AddressingMode.Absolute, 6), new Instruction(Mnemonic.DCP, AddressingMode.Absolute, 6),
            new Instruction(Mnemonic.BNE, AddressingMode.Relative, 2, true), new Instruction(Mnemonic.CMP, AddressingMode.IndirectY, 5, true), null, new Instruction(Mnemonic.DCP, AddressingMode.IndirectY, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.CMP, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.DEC, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.DCP, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.CLD, AddressingMode.Implied, 2), new Instruction(Mnemonic.CMP, AddressingMode.AbsoluteY, 4, true), new Instruction(Mnemonic.NOP, AddressingMode.Implied, 2), new Instruction(Mnemonic.DCP, AddressingMode.AbsoluteY, 7), new Instruction(Mnemonic.NOP, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.CMP, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.DEC, AddressingMode.AbsoluteX, 7), new Instruction(Mnemonic.DCP, AddressingMode.AbsoluteX, 7),
            new Instruction(Mnemonic.CPX, AddressingMode.Immediate, 2), new Instruction(Mnemonic.SBC, AddressingMode.IndirectX, 6), new Instruction(Mnemonic.NOP, AddressingMode.Immediate, 2),  new Instruction(Mnemonic.ISB, AddressingMode.IndirectX, 8), new Instruction(Mnemonic.CPX, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.SBC, AddressingMode.ZeroPage, 3), new Instruction(Mnemonic.INC, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.ISB, AddressingMode.ZeroPage, 5), new Instruction(Mnemonic.INX, AddressingMode.Implied, 2), new Instruction(Mnemonic.SBC, AddressingMode.Immediate, 2), new Instruction(Mnemonic.NOP, AddressingMode.Implied, 2), new Instruction(Mnemonic.SBC, AddressingMode.Immediate, 2), new Instruction(Mnemonic.CPX, AddressingMode.Absolute, 4), new Instruction(Mnemonic.SBC, AddressingMode.Absolute, 4), new Instruction(Mnemonic.INC, AddressingMode.Absolute, 6), new Instruction(Mnemonic.ISB, AddressingMode.Absolute, 6),
            new Instruction(Mnemonic.BEQ, AddressingMode.Relative, 2, true), new Instruction(Mnemonic.SBC, AddressingMode.IndirectY, 5, true), null,     new Instruction(Mnemonic.ISB, AddressingMode.IndirectY, 8), new Instruction(Mnemonic.NOP, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.SBC, AddressingMode.ZeroPageX, 4), new Instruction(Mnemonic.INC, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.ISB, AddressingMode.ZeroPageX, 6), new Instruction(Mnemonic.SED, AddressingMode.Implied, 2), new Instruction(Mnemonic.SBC, AddressingMode.AbsoluteY, 4, true), new Instruction(Mnemonic.NOP, AddressingMode.Implied, 2), new Instruction(Mnemonic.ISB, AddressingMode.AbsoluteY, 7), new Instruction(Mnemonic.NOP, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.SBC, AddressingMode.AbsoluteX, 4, true), new Instruction(Mnemonic.INC, AddressingMode.AbsoluteX, 7), new Instruction(Mnemonic.ISB, AddressingMode.AbsoluteX, 7)
        };
        #endregion
    }
}
