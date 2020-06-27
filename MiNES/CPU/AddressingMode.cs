namespace MiNES.CPU
{
    /// <summary>
    /// The addressing modes used by the 6502 CPU.
    /// </summary>
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
}
