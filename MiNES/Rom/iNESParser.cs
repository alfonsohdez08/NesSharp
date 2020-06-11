using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiNES.Rom
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
        public static void ParseNesRom(string nesFilePath, out Memory cpuMemoryMapped, out Memory ppuMemoryMapped)
        {
            if (string.IsNullOrEmpty(nesFilePath))
                throw new ArgumentNullException(nameof(nesFilePath));

            ParseNesRom(File.ReadAllBytes(nesFilePath), out cpuMemoryMapped, out ppuMemoryMapped);
        }

        /// <summary>
        /// Reads a NES file (.nes file extension) for dump its content into the NES memory.
        /// </summary>
        /// <param name="content">The content in byte of the NES file.</param>
        /// <returns>A ready to use NES memory (program rom and character rom already mapped).</returns>
        public static void ParseNesRom(byte[] content, out Memory cpuMemoryMapped, out Memory ppuMemoryMapped)
        {
            if (content == null || content.Length == 0)
                throw new ArgumentException(nameof(content));

            //byte[] nesMemory = Memory.CreateEmptyMemory();
            cpuMemoryMapped = new Memory(0x10000); // from 0x0000 up to 0xFFFF (in decimal: 0 up to 65,535)
            ppuMemoryMapped = new Memory(0x4000);

            byte numberOfPrgBanks = content[5];
            byte[] prgRomLowerBank = new ArraySegment<byte>(content, 0x0010, 0x4000).ToArray();
            byte[] prgRomUpperBank = numberOfPrgBanks > 1 ? new ArraySegment<byte>(content, 0x4001, 0x4000).ToArray() : prgRomLowerBank;

            // Map PRG lower bank
            ushort address = 0x8000;
            for (int i = 0; address < 0xC000 && i < prgRomLowerBank.Length; address++, i++)
                cpuMemoryMapped.Store(address, prgRomLowerBank[i]);
            
            // Map PRG upper bank
            address = 0xC000;
            for (int i = 0; address <= 0xFFFF && i < prgRomUpperBank.Length; address++, i++)
                cpuMemoryMapped.Store(address, prgRomUpperBank[i]);

            //TODO: Map ppu memory space
        }
    }
}
