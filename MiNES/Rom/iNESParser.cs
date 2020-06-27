using MiNES.PPU;
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
        /// The header offset (16 bytes).
        /// </summary>
        private const byte HeaderOffset = 0x10;

        /// <summary>
        /// Reads a NES file (.nes file extension) for dump its content into the NES memory.
        /// </summary>
        /// <param name="nesFilePath">The absolute path of the NES file.</param>
        /// <returns>A ready to use NES memory (program rom and character rom already mapped).</returns>
        public static void ParseNesCartridge(string nesFilePath, out Memory cpuMemoryMapped, out Memory ppuMemoryMapped, out Mirroring mirroring)
        {
            if (string.IsNullOrEmpty(nesFilePath))
                throw new ArgumentNullException(nameof(nesFilePath));

            ParseNesCartridge(File.ReadAllBytes(nesFilePath), out cpuMemoryMapped, out ppuMemoryMapped, out mirroring);
        }

        /// <summary>
        /// Reads a NES file (.nes file extension) for dump its content into the NES memory.
        /// </summary>
        /// <param name="content">The content in byte of the NES file.</param>
        /// <returns>A ready to use NES memory (program rom and character rom already mapped).</returns>
        public static void ParseNesCartridge(byte[] content, out Memory cpuMemoryMapped, out Memory ppuMemoryMapped, out Mirroring mirroring)
        {
            if (content == null || content.Length == 0)
                throw new ArgumentException(nameof(content));

            cpuMemoryMapped = new Memory(0x10000); // from 0x0000 up to 0xFFFF (in decimal: 0 up to 65,535)
            ppuMemoryMapped = new Memory(0x4000);

            byte numberOfPrgBanks = content[4];
            byte[] prgRomLowerBank = new ArraySegment<byte>(content, HeaderOffset, 0x4000).ToArray();
            byte[] prgRomUpperBank = numberOfPrgBanks > 1 ? new ArraySegment<byte>(content, 0x4000 + HeaderOffset, 0x4000).ToArray() : prgRomLowerBank;

            // Only supports mapper 0
            byte flags6 = content[6];
            mirroring = (Mirroring)(byte)(flags6 & 0x01); // Bit 0 from flags 6 determine which kind of mirroring the game supports

            // Map PRG lower bank
            ushort address = 0x8000;
            for (int i = 0; address < 0xC000 && i < prgRomLowerBank.Length; address++, i++)
                cpuMemoryMapped.Store(address, prgRomLowerBank[i]);
            
            // Map PRG upper bank
            address = 0xC000;
            for (int i = 0; address <= 0xFFFF && i < prgRomUpperBank.Length; address++, i++)
                cpuMemoryMapped.Store(address, prgRomUpperBank[i]);

            // Map CHR bank
            byte[] chrRom = new ArraySegment<byte>(content, HeaderOffset + (numberOfPrgBanks * 0x4000), 0x2000).ToArray();

            address = 0x0000;
            for (int i = 0; address < 0x2000 && i < chrRom.Length; i++, address++)
                ppuMemoryMapped.Store(address, chrRom[i]);
        }
    }
}
