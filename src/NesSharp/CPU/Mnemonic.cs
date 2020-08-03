﻿namespace NesSharp.CPU
{
    /// <summary>
    /// The 6502 CPU mnemonics.
    /// </summary>
    internal enum Mnemonic : byte
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
}
