using NesSharp.PPU;
using System;
using System.IO;

namespace NesSharp
{
    /// <summary>
    /// A NES game cartridge.
    /// </summary>
    public class Cartridge
    {
        private const byte HeaderOffset = 16;

        /// <summary>
        /// Gets the mirroring configured for the game stored in the cartridge.
        /// </summary>
        public Mirroring GameMirroring { get; private set; }

        /// <summary>
        /// Gets the program rom.
        /// </summary>
        public byte[] ProgramRom { get; private set; } = new byte[0x8000];

        /// <summary>
        /// Gets the character rom.
        /// </summary>
        public byte[] CharacterRom { get; private set; } = new byte[0x2000];

        private int _prgBanks;
        private int _chrBanks;

        private Cartridge(byte[] rom)
        {
            ParseHeader(rom);
            ParseProgramRomBanks(rom);
            ParseCharacterRomBanks(rom);
        }

        private void ParseHeader(byte[] nesFile)
        {
            _prgBanks = nesFile[4];
            if (_prgBanks > 2)
                throw new NotSupportedException("Can not support more than 2 program rom bank.");

            _chrBanks = nesFile[5];

            byte flags6 = nesFile[6];
            GameMirroring = (Mirroring)(flags6 & 1);
        }

        private void ParseProgramRomBanks(byte[] nesFile)
        {
            byte[] lowerBank = new ArraySegment<byte>(nesFile, HeaderOffset, 0x4000).ToArray();
            byte[] upperBank;

            if (_prgBanks > 1)
                upperBank = new ArraySegment<byte>(nesFile, HeaderOffset + 0x4000, 0x4000).ToArray();
            else
                upperBank = lowerBank;

            Array.Copy(lowerBank, ProgramRom, lowerBank.Length);
            Array.Copy(upperBank, 0, ProgramRom, 0x4000, upperBank.Length);
        }

        private void ParseCharacterRomBanks(byte[] nesFile)
        {
            if (_chrBanks == 0)
                return;

            CharacterRom = new ArraySegment<byte>(nesFile, HeaderOffset + _prgBanks * 0x4000, 0x2000).ToArray();
        }

        /// <summary>
        /// Builds a NES game cartridge.
        /// </summary>
        /// <param name="path">The path where the cartridge is located.</param>
        /// <returns>A cartridge ready to be played.</returns>
        public static Cartridge LoadCartridge(string path) => new Cartridge(File.ReadAllBytes(path));
    }
}
