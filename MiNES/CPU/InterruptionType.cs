namespace MiNES.CPU
{
    /// <summary>
    /// The interruptions available for the 6502 CPU.
    /// </summary>
    enum InterruptionType
    {
        NMI,
        IRQ,
        BRK,
        RESET
    }
}