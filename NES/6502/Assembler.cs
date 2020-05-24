using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NES._6502
{
    static class Assembler
    {
        private static readonly Dictionary<string, HashSet<AddressingMode>> MnemonicAddrModes;

        private static readonly RegexOptions RegexOpts = RegexOptions.IgnoreCase | RegexOptions.ECMAScript;
        private static readonly Dictionary<AddressingMode, Regex> AddrModeRegexPatterns = new Dictionary<AddressingMode, Regex>()
        {
            {AddressingMode.Accumulator, new Regex(@"(a| ){1}(\r\n|\n)", RegexOpts)},
            {AddressingMode.Absolute, new Regex(@"\$([0-9]|[a-f]){1,4}(\r\n|\n)", RegexOpts)},
            {AddressingMode.AbsoluteX, new Regex(@"\$([0-9]|[a-f]){1,4},x(\r\n|\n)", RegexOpts)},
            {AddressingMode.AbsoluteY, new Regex(@"\$([0-9]|[a-f]){1,4},y(\r\n|\n)", RegexOpts)},
            {AddressingMode.Immediate, new Regex(@"^#\$([0-9]|[a-f]){1,2}(\r\n|\n)", RegexOpts)},
            {AddressingMode.Implied, null},
            {AddressingMode.Indirect, new Regex(@"\(\$([0-9]|[a-f]){1,4}\)(\r\n|\n)", RegexOpts)},
            {AddressingMode.IndirectX, new Regex(@"\(\$([0-9]|[a-f]){1,2},x\)(\r\n|\n)", RegexOpts)},
            {AddressingMode.IndirectY, new Regex(@"\(\$([0-9]|[a-f]){1,2}\),y(\r\n|\n)", RegexOpts)},
            {AddressingMode.Relative, new Regex(@"^\$([0-9]|[a-f]){1,2}(\r\n|\n)", RegexOpts)},
            {AddressingMode.ZeroPage, new Regex(@"^\$([0-9]|[a-f]){1,2}(\r\n|\n)", RegexOpts)},
            {AddressingMode.ZeroPageX, new Regex(@"^\$([0-9]|[a-f]){1,2},x(\r\n|\n)", RegexOpts)},
            {AddressingMode.ZeroPageY, new Regex(@"^\$([0-9]|[a-f]){1,2},y(\r\n|\n)", RegexOpts)},
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
        /// Assembles a program.
        /// </summary>
        /// <param name="program">The program.</param>
        /// <returns>A set of hexadecimal values where each value represents either an instruction or an instruction operand.</returns>
        public static string[] Assemble(string program)
        {
            var programAssembled = new List<string>();

            string[] lines = program.Split("\r\n");
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Split(';')[0]; // Strips the comments next to the instruction (a comment is started by using semi colon)
                line += '\n'; // Add new line feed

                string[] instructionHex = ParseInstruction(line);
                programAssembled.AddRange(instructionHex);
            }

            return programAssembled.ToArray();
        }

        private static string[] ParseInstruction(string instructionLine)
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

            string[] operandHex = GetInstructionOperandHexValue(operand, addressingMode);
            hexDump.AddRange(operandHex);

            return hexDump.ToArray();
        }

        private static string GetInstructionHexValue(string mnemonic, AddressingMode addressingMode)
        {
            for (int index = 0; index < Cpu.OpCodes.Length; index++)
                if (Cpu.OpCodes[index]?.Mnemonic == mnemonic && Cpu.OpCodes[index].AddressingMode == addressingMode)
                    return index.ToString("x");

            throw new InvalidOperationException("Can not find the instruction based on the mnemonic and addressing mode specified.");
        }

        private static string[] GetInstructionOperandHexValue(string instructionOperand, AddressingMode addrMode)
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
                        hexValues.Add(lowByte.ToString("x"));
                        hexValues.Add(highByte.ToString("x"));
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
                        hexValues.Add(m.Value);
                    }
                    break;
            }
            
            return hexValues.ToArray();
        }

        private static AddressingMode ParseAddressingMode(string mnemonic, string operand)
        {
            if (!MnemonicAddrModes.ContainsKey(mnemonic))
                throw new ArgumentException($"The given mnemonic {mnemonic} does not exist.");

            //operand = StripSpaces(operand);

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
    }
}
