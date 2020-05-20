using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NES._6502
{
    class Assembler
    {
        private static readonly Dictionary<string, HashSet<AddressingMode>> MnemonicAddrModes;

        private static readonly RegexOptions RegexOpts = RegexOptions.IgnoreCase | RegexOptions.ECMAScript;
        private static readonly Dictionary<AddressingMode, Regex> AddrModeRegexPatterns = new Dictionary<AddressingMode, Regex>()
        {
            {AddressingMode.Accumulator, new Regex(@"(a| ){1}\n", RegexOpts)},
            {AddressingMode.Absolute, new Regex(@"\$([0-9]|[a-f]){1,4}\n", RegexOpts)},
            {AddressingMode.AbsoluteX, new Regex(@"\$([0-9]|[a-f]){1,4},x\n", RegexOpts)},
            {AddressingMode.AbsoluteY, new Regex(@"\$([0-9]|[a-f]){1,4},y\n", RegexOpts)},
            {AddressingMode.Immediate, new Regex(@"#\$([0-9]|[a-f]){1,2}\n", RegexOpts)},
            {AddressingMode.Implied, null},
            {AddressingMode.Indirect, new Regex(@"\(\$([0-9]|[a-f]){1,4}\)\n", RegexOpts)},
            {AddressingMode.IndirectX, new Regex(@"\(\$([0-9]|[a-f]){1,2},x\)\n", RegexOpts)},
            {AddressingMode.IndirectY, new Regex(@"\(\$([0-9]|[a-f]){1,2}\),y\n", RegexOpts)},
            {AddressingMode.Relative, new Regex(@"\$([0-9]|[a-f]){1,2}\n", RegexOpts)},
            {AddressingMode.ZeroPage, new Regex(@"\$([0-9]|[a-f]){1,2}\n", RegexOpts)},
            {AddressingMode.ZeroPageX, new Regex(@"\$([0-9]|[a-f]){1,2},x\n", RegexOpts)},
            {AddressingMode.ZeroPageY, new Regex(@"\$([0-9]|[a-f]){1,2},y\n", RegexOpts)},
        };

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

        //private static Dictionary<Regex, AddressingMode> _regexAddr = new Dictionary<Regex, AddressingMode>()
        //{
        //    {new Regex("A"), AddressingMode.Accumulator},
        //    { new Regex(""), AddressingMode.Absolute }
        //};

        /// <summary>
        /// Assembles a program.
        /// </summary>
        /// <param name="program">The program.</param>
        /// <returns>A set of hexadecimal values where each value represents either an instruction or an instruction operand.</returns>
        public string[] Assemble(string program)
        {
            var programAssembled = new List<string>();

            string[] lines = program.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                line = line.Split(';')[0]; // Strips the comments next to the instruction (a comment is started by using semi colon)


            }

            return programAssembled.ToArray();
        }

        private static Instruction Parse(string fullInstruction)
        {
            string[] tokens = fullInstruction.Split(' ');
            if (tokens.Length == 0)
                throw new ArgumentException($"Can not recoginze the given instruction: {fullInstruction}");

            string mnemonic = tokens[0];
            string operand = tokens[1];

            //AddressingMode addrMode = ParseAddressingMode(operand);

            throw new NotImplementedException();

            //return FindInstruction(mnemonic, addrMode);
        }


        /// <summary>
        /// Finds the instruction based on its mnemonic code and addressing mode.
        /// </summary>
        /// <param name="mnemonic">The instruction's mnemonic.</param>
        /// <param name="addrMode">The instruction's addressing mode.</param>
        /// <returns>The 6502 CPU instruction.</returns>
        private static Instruction FindInstruction(string mnemonic, AddressingMode addrMode)
        {
            //for (int index = 0; index < Cpu.OpCodes.Length; index++)
            //    if (Cpu.OpCodes[index]?.Mnemonic == mnemonic && Cpu.OpCodes[index].AddressingMode == addrMode)
            //        return Cpu.OpCodes[index];



            throw new InvalidOperationException($"Did not find an instruction identified by the mnemonic {mnemonic} and addressing mode {addrMode}.");
        }

        private static AddressingMode ParseAddressingMode(string mnemonic, string operand)
        {
            if (!MnemonicAddrModes.ContainsKey(mnemonic))
                throw new ArgumentException($"The given mnemonic {mnemonic} does not exist.");

            HashSet<AddressingMode> addrModes = MnemonicAddrModes[mnemonic];


            throw new NotImplementedException();
        }
    }
}
