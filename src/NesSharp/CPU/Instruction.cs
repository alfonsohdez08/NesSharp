namespace NesSharp.CPU
{
    /// <summary>
    /// Represents a CPU instruction (including its mnemonic, addressing mode and machine cycles).
    /// </summary>
    class Instruction
    {
        /// <summary>
        /// The addressing mode of the instruction.
        /// </summary>
        public AddressingMode AddressingMode { get; private set; }

        /// <summary>
        /// The amount of cycles required in order to execute the instruction (each cycle represents either a memory read or write).
        /// </summary>
        public byte Cycles { get; private set; }

        /// <summary>
        /// Denotes whether increment the instruction cycles when instruction's operand address cross a page within memory.
        /// </summary>
        public bool AdditionalCycleWhenCrossPage { get; private set; }

        public Mnemonic Mnemonic { get; private set; }

        public Instruction(Mnemonic mnemonic, AddressingMode addressingMode, byte machineCycles, bool additionalCycle = false)
        {
            Mnemonic = mnemonic;
            AddressingMode = addressingMode;
            Cycles = machineCycles;
            AdditionalCycleWhenCrossPage = additionalCycle;
        }

        public override string ToString() => $"{Mnemonic} {AddressingMode}";
    }
}
