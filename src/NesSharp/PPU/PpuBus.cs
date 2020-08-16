using System;
using System.Diagnostics.Tracing;

namespace NesSharp.PPU
{

    /// <summary>
    /// PPU's bus.
    /// </summary>
    public class PpuBus: IBus
    {        
        private readonly byte[] _vram = new byte[2 * 1024];
        private readonly byte[] _paletteRam = new byte[32];
        private readonly byte[] _chr;

        private readonly INametableAddressParser _ntAddressParser;

        public PpuBus(byte[] chrData, Mirroring mirroring)
        {
            _chr = chrData;
            _ntAddressParser = NametableMirroringResolver.GetAddressParser(mirroring);
        }

        public byte Read(ushort address)
        {
            if (address >= 0x2000 && address < 0x3000)
                return ReadNametable((ushort)(0x2000 + (address & 0x0FFF)));
            else if (address >= 0x3000 && address <= 0x3EFF)
                return ReadNametable((ushort)(0x2000 + (address % 0x0F00)));
            // Background palette and sprite palletes (mirrored in the range [0x3F20, 0x3FFF])
            else if (address >= 0x3F00 && address < 0x4000)
                return ReadPalette((ushort)(0x3F00 + (address & 0x001F)));

            return _chr[address & 0x3FFF];
        }

        public byte ReadCharacterRom(ushort address) => _chr[address & 0x3FFF];

        public byte ReadPalette(ushort address)
        {
            switch (address)
            {
                case 0x3F10:
                    address = 0x3F00;
                    break;
                case 0x3F14:
                    address = 0x3F04;
                    break;
                case 0x3F18:
                    address = 0x3F08;
                    break;
                case 0x3F1C:
                    address = 0x3F0C;
                    break;
            }

            return _paletteRam[address & 0x1F];
        }

        private void WritePalette(ushort address, byte paletteEntry)
        {
            switch (address)
            {
                case 0x3F10:
                    address = 0x3F00;
                    break;
                case 0x3F14:
                    address = 0x3F04;
                    break;
                case 0x3F18:
                    address = 0x3F08;
                    break;
                case 0x3F1C:
                    address = 0x3F0C;
                    break;
            }

            _paletteRam[address & 0x1F] = paletteEntry;
        }

        public void Write(ushort address, byte val)
        {
            // For CHR-RAM
            if (address >= 0x0000 & address < 0x2000)
                _chr[address] = val;
            else if (address >= 0x2000 && address < 0x3000)
                WriteNametable((ushort)(0x2000 + (address & 0x0FFF)), val);
            else if (address >= 0x3000 && address <= 0x3EFF)
                WriteNametable((ushort)(0x2000 + (address % 0x0F00)), val);
            else if (address >= 0x3F00 && address < 0x4000)
                WritePalette((ushort)(0x3F00 + (address & 0x001F)), val);
        }

        /// <summary>
        /// Writes an entry in a nametable (considering the game mirroring setup).
        /// </summary>
        /// <param name="address">The address where the value should be stored.</param>
        /// <param name="val">The value that will be stored.</param>
        private void WriteNametable(ushort address, byte val)
        {
            var addressParsed = _ntAddressParser.Parse(address);

            _vram[addressParsed & 0x07FF] = val;
        }

        /// <summary>
        /// Reads a nametable entry (considering the game mirroring setup).
        /// </summary>
        /// <param name="address">The address where the entry is allocated.</param>
        /// <returns>The value allocated in the given address.</returns>
        public byte ReadNametable(ushort address)
        {
            var addressParsed = _ntAddressParser.Parse(address);

            return _vram[addressParsed & 0x07FF];
        }
    }
}
