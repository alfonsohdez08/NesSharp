using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NES.Rom
{
    /// <summary>
    /// Parser for the .NES files.
    /// </summary>
    static class iNESParser
    {


        /// <summary>
        /// Reads a NES file (.nes file extension) for dump its content into the NES memory.
        /// </summary>
        /// <param name="nesFilePath">The absolute path of the NES file.</param>
        /// <returns>A ready to use NES memory (program rom and character rom already mapped).</returns>
        public static Memory ParseNesFile(string nesFilePath)
        {
            if (string.IsNullOrEmpty(nesFilePath))
                throw new ArgumentNullException(nameof(nesFilePath));

            return ParseNesFile(File.ReadAllBytes(nesFilePath));
        }

        /// <summary>
        /// Reads a NES file (.nes file extension) for dump its content into the NES memory.
        /// </summary>
        /// <param name="content">The content in byte of the NES file.</param>
        /// <returns>A ready to use NES memory (program rom and character rom already mapped).</returns>
        public static Memory ParseNesFile(byte[] content)
        {
            if (content == null || content.Length == 0)
                throw new ArgumentException(nameof(content));

            byte[] nesMemory = Memory.CreateEmptyMemory();

            byte numberOfPrgBanks = content[5];
            byte[] prgLowerBank = new ArraySegment<byte>(content, 16, 16384).ToArray();
            byte[] prgUpperBank = numberOfPrgBanks > 1 ? new ArraySegment<byte>(content, 16385, 16384).ToArray() : prgLowerBank;

            // Map PRG lower bank
            ushort address = 0x8000;
            for (int i = 0; address < 0xC000 && i < prgLowerBank.Length; address++, i++)
                nesMemory[address] = prgLowerBank[i];
            
            // Map PRG upper bank
            address = 0xC000;
            for (int i = 0; address < 0xFFFF && i < prgUpperBank.Length; address++, i++)
                nesMemory[address] = prgUpperBank[i];

            throw new NotImplementedException();

            //return new Memory(nesMemory);
        }
    }
}
