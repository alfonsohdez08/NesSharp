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
        /// <param name="content">The content in byte of the NES file.</param>
        /// <returns>A ready to use NES memory (program rom and character rom already mapped).</returns>
        public static void ParseNesCartridge(byte[] content, out byte[] programRom, out byte[] characterRom, out Mirroring mirroring)
        {
            if (content == null || content.Length == 0)
                throw new ArgumentException(nameof(content));

            programRom = new byte[32 * 1024]; // from 0x0000 up to 0xFFFF (in decimal: 0 up to 65,535)
            //characterRom = new byte[0x4000];

            byte numberOfPrgBanks = content[4];
            byte[] prgRomLowerBank = new ArraySegment<byte>(content, HeaderOffset, 0x4000).ToArray();
            byte[] prgRomUpperBank = numberOfPrgBanks > 1 ? new ArraySegment<byte>(content, 0x4000 + HeaderOffset, 0x4000).ToArray() : prgRomLowerBank;

            // Only supports mapper 0
            byte flags6 = content[6];
            mirroring = (Mirroring)(byte)(flags6 & 0x01); // Bit 0 from flags 6 determine which kind of mirroring the game supports

            // Map PRG lower bank
            for (int i = 0; i < prgRomLowerBank.Length; i++)
                programRom[i] = prgRomLowerBank[i];

            // Map PRG upper bank
            for (int i = 0x4000, j = 0; i < 0x8000; j++, i++)
                programRom[i] = prgRomUpperBank[j];

            // Map CHR bank
            characterRom = new ArraySegment<byte>(content, HeaderOffset + (numberOfPrgBanks * 0x4000), 0x2000).ToArray();
        }
    }
}
