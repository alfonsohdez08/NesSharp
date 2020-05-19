using System;
using System.Collections.Generic;
using System.Text;

namespace NES._6502
{
    class Assembler
    {



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

            for (int index = 1; index < tokens.Length; index++)
            {

            }

            throw new NotImplementedException();
        }
    }
}
