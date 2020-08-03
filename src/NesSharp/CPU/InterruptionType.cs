namespace NesSharp.CPU
{
    /// <summary>
    /// The 6502 CPU interruptions.
    /// </summary>
    internal enum InterruptionType
    {
        NMI,
        IRQ,
        BRK,
        RESET
    }
}