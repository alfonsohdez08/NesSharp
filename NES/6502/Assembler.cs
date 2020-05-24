using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NES._6502
{
    internal static class Assembler
    {
        private static readonly Dictionary<string, HashSet<AddressingMode>> MnemonicAddrModes;

        private static readonly RegexOptions RegexOpts = RegexOptions.IgnoreCase | RegexOptions.ECMAScript;
        private static readonly Dictionary<AddressingMode, Regex> AddrModeRegexPatterns = new Dictionary<AddressingMode, Regex>()
        {
            {AddressingMode.Accumulator, new Regex(@"^(a| ){1}$", RegexOpts)},
            {AddressingMode.Absolute, new Regex(@"^\$([0-9]|[a-f]){1,4}$", RegexOpts)},
            {AddressingMode.AbsoluteX, new Regex(@"^\$([0-9]|[a-f]){1,4},x$", RegexOpts)},
            {AddressingMode.AbsoluteY, new Regex(@"^\$([0-9]|[a-f]){1,4},y$", RegexOpts)},
            {AddressingMode.Immediate, new Regex(@"^#\$([0-9]|[a-f]){1,2}$", RegexOpts)},
            {AddressingMode.Indirect, new Regex(@"^\(\$([0-9]|[a-f]){1,4}\)$", RegexOpts)},
            {AddressingMode.IndirectX, new Regex(@"^\(\$([0-9]|[a-f]){1,2},x\)$", RegexOpts)},
            {AddressingMode.IndirectY, new Regex(@"^\(\$([0-9]|[a-f]){1,2}\),y$", RegexOpts)},
            {AddressingMode.Relative, new Regex(@"^\$([0-9]|[a-f]){1,2}$", RegexOpts)},
            {AddressingMode.ZeroPage, new Regex(@"^\$([0-9]|[a-f]){1,2}$", RegexOpts)},
            {AddressingMode.ZeroPageX, new Regex(@"^\$([0-9]|[a-f]){1,2},x$", RegexOpts)},
            {AddressingMode.ZeroPageY, new Regex(@"^\$([0-9]|[a-f]){1,2},y$", RegexOpts)},
            {AddressingMode.Implied, null}
        };

        private static readonly Regex _16BitRegexPattern = new Regex(@"([0-9]|[a-f]){1,4}", RegexOpts);
        private static readonly Regex _8BitRegexPattern = new Regex(@"([0-9]|[a-f]){1,2}", RegexOpts);

        static Assembler()
        {
            MnemonicAddrModes = new Dictionary<string, HashSet<AddressingMode>>();
            for (int index = 0; index < Cpu.OpCodes.Length; index++)
            {
                if (Cpu.OpCodes[index] == null)
                    continue;

                string mnemonic = Cpu.OpCodes[index].Mnemonic;
                if (!MnemonicAddrModes.ContainsKey(mnemonic))
                    MnemonicAddrModes[mnemonic] = new HashSet<AddressingMode>();

                MnemonicAddrModes[mnemonic].Add(Cpu.OpCodes[index].AddressingMode);
            }
        }

        /// <summary>
        /// Assembles a program for a 6502 CPU.
        /// </summary>
        /// <param name="program">The program.</param>
        /// <returns>The program assembled.</returns>
        public static ProgramAssembled Assemble(string program)
        {
            var programHexDump = new List<string>();
            var instructions = new List<InstructionAssembled>();

            string[] lines = program.Split("\r\n");
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Split(';')[0]; // Strips the comments next to the instruction (a comment is started by using semi colon)

                (string, AddressingMode, string[]) instruction = ParseInstruction(line);
                var instructionAssembled = new InstructionAssembled(instruction.Item1, instruction.Item2, instruction.Item3, lines[i]);

                instructions.Add(instructionAssembled);
                programHexDump.AddRange(instruction.Item3);
            }

            var programAssembled = new ProgramAssembled(program, programHexDump.ToArray(), instructions);

            return programAssembled;
        }

        /// <summary>
        /// Parses a line of the program (holds the program's instruction).
        /// </summary>
        /// <param name="instructionLine">A program's instruction.</param>
        /// <returns>The mnemonic that represents the given instruction along with its addressing mode and hexadecimal dump.</returns>
        private static (string, AddressingMode, string[]) ParseInstruction(string instructionLine)
        {
            var hexDump = new List<string>();

            // Splits the whole instruction by whitespace
            string[] tokens = instructionLine.Split(' ');
            if (tokens.Length == 0)
                throw new ArgumentException($"Can not recognize the given instruction: {instructionLine}");

            if (tokens.Length > 2)
                throw new ArgumentException("The instruction isn't delimitted correctly. Ensure you specify a single whitespace between the mnemonic and the instruction operand.");

            string mnemonic = tokens[0].ToUpper();

            string operand = string.Empty;
            if (tokens.Length > 1) // Operand would not be supplied when using implied addressing mode
                operand = tokens[1];

            AddressingMode addressingMode = ParseAddressingMode(mnemonic, operand);

            string instructionHex = GetInstructionHexValue(mnemonic, addressingMode);
            hexDump.Add(instructionHex);

            string[] operandHex = GetOperandHexDump(operand, addressingMode);
            hexDump.AddRange(operandHex);

            return (mnemonic, addressingMode, hexDump.ToArray());
        }

        /// <summary>
        /// Retrieves the hexadecimal value of a instruction based on its mnemonic and addressing mode.
        /// </summary>
        /// <param name="mnemonic">The instruction's mnemonic.</param>
        /// <param name="addressingMode">The instruction's addressing mode.</param>
        /// <returns>The hexadecimal value based on the OpCodes matrix for the 6502 CPU.</returns>
        private static string GetInstructionHexValue(string mnemonic, AddressingMode addressingMode)
        {
            for (int index = 0; index < Cpu.OpCodes.Length; index++)
                if (Cpu.OpCodes[index]?.Mnemonic == mnemonic && Cpu.OpCodes[index].AddressingMode == addressingMode)
                    return GetHex((byte)index);

            throw new InvalidOperationException("Can not find the instruction based on the mnemonic and addressing mode specified.");
        }

        /// <summary>
        /// Retrieves the hexadecimal dump for the instruction operand based on its addressing mode.
        /// </summary>
        /// <param name="instructionOperand">The raw instruction operand.</param>
        /// <param name="addrMode">The instruction's addressing mode.</param>
        /// <returns>The hexadecimal dump of the given instruction operand.</returns>
        private static string[] GetOperandHexDump(string instructionOperand, AddressingMode addrMode)
        {
            var hexValues = new List<string>();

            switch (addrMode)
            {
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.Indirect:
                    {
                        Match m = _16BitRegexPattern.Match(instructionOperand);
                        short val = Convert.ToInt16(m.Value, 16);
                        byte lowByte = (byte)val;
                        byte highByte = (byte)(val >> 8);
                        //byte lowByte = (byte)(val & 0xFF);
                        //byte highByte = (byte)(val >> 8 & 0xFF);

                        // 6502 CPU is little endian (low byte first and then high byte)
                        hexValues.Add(GetHex(lowByte));
                        hexValues.Add(GetHex(highByte));
                    }
                    break;
                case AddressingMode.Immediate:
                case AddressingMode.IndirectX:
                case AddressingMode.IndirectY:
                case AddressingMode.Relative:
                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageX:
                case AddressingMode.ZeroPageY:
                    {
                        Match m = _8BitRegexPattern.Match(instructionOperand);
                        byte b = Convert.ToByte(m.Value, 16);

                        hexValues.Add(GetHex(b));
                    }
                    break;
            }           
            
            return hexValues.ToArray();
        }

        /// <summary>
        /// Determines the addressing mode for a given mnemonic and operand.
        /// </summary>
        /// <param name="mnemonic">The instruction's mnemonic.</param>
        /// <param name="operand">The instruction's operand.</param>
        /// <returns>The addressing mode for the given instruction.</returns>
        private static AddressingMode ParseAddressingMode(string mnemonic, string operand)
        {
            if (!MnemonicAddrModes.ContainsKey(mnemonic))
                throw new ArgumentException($"The given mnemonic {mnemonic} does not exist.");

            operand = StripSpaces(operand);

            HashSet<AddressingMode> addrModes = MnemonicAddrModes[mnemonic];
            if (addrModes.Count == 1)
                return addrModes.First();

            foreach (AddressingMode addrMode in addrModes)
            {
                Regex regex = AddrModeRegexPatterns[addrMode];
                if (regex.IsMatch(operand))
                    return addrMode;
            }

            static string StripSpaces(string s) => s.TrimStart().TrimEnd();

            throw new InvalidOperationException($"Can not identify the addressing mode for the mnemonic {mnemonic} based on the provided operand {operand}");
        }

        /// <summary>
        /// Retrieves the hexadecimal representation of a given byte.
        /// </summary>
        /// <param name="b">8 bit value.</param>
        /// <returns>The hexadecimal representation of the given byte.</returns>
        private static string GetHex(byte b)
        {
            string h = b.ToString("x");
            if (h.Length == 1) // puts a zero in case the hex representation is only one digit (just for convention)
                h = $"0{h}";
            return h;
        }
    }

    /// <summary>
    /// Contains the information about the program assembled (hexadecimal dump, instructions parsed, and the raw program).
    /// </summary>
    class ProgramAssembled
    {
        public string[] HexadecimalDump { get; private set; }
        public IReadOnlyCollection<InstructionAssembled> Instructions { get; private set; }
        public string ProgramDissassembled { get; private set; }

        public ProgramAssembled(string program, string[] programHexDump, List<InstructionAssembled> instructions)
        {
            ProgramDissassembled = program;
            HexadecimalDump = programHexDump;
            Instructions = instructions.AsReadOnly();
        }
    }

    /// <summary>
    /// Contains information about the instruction assembled (mnemonic code, addressing mode, hexadecimal dump, and the raw instruction line).
    /// </summary>
    class InstructionAssembled
    {
        public string Mnemonic { get; private set; }
        public AddressingMode AddressingMode { get; private set; }
        public string[] HexadecimalDump { get; private set; }
        public string RawInstructionLine { get; private set; }

        public InstructionAssembled(string mnemonic, AddressingMode addressingMode, string[] hexDump, string rawInstructionLine)
        {
            Mnemonic = mnemonic;
            AddressingMode = addressingMode;
            HexadecimalDump = hexDump;
            RawInstructionLine = rawInstructionLine;
        }
    }
}
