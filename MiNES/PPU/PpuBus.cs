using System;
using System.Diagnostics.Tracing;

namespace MiNES.PPU
{

    /// <summary>
    /// PPU's bus.
    /// </summary>
    public class PpuBus
    {
        private readonly Mirroring _mirroring;
        
        private readonly byte[] _vram = new byte[2 * 1024];
        private readonly byte[] _paletteRam = new byte[32];
        private readonly byte[] _chrRom;

        public PpuBus(byte[] chrRom, Mirroring mirroring)
        {
            _mirroring = mirroring;
            _chrRom = chrRom;
        }

        public byte Read(uint address)
        {
            // Nametables and attribute tables (mirrored in the range [0x3000, 0x3EFF])
            if (address >= 0x2000 && address < 0x3F00)
                return ReadNametable(0x2000 + (address & 0x0FFF));
            // Background palette and sprite palletes (mirrored in the range [0x3F20, 0x3FFF])
            else if (address >= 0x3F00 && address < 0x4000)
                return ReadPalette(0x3F00 + (address & 0x001F));

            return _chrRom[address & 0x3FFF];
        }

        private byte ReadPalette(uint address)
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

        private void WritePalette(uint address, byte paletteEntry)
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

        public void Write(uint address, byte val)
        {
            if (address >= 0x2000 && address < 0x3F00)
                WriteNametable(0x2000 + (address & 0x0FFF), val);
            else if (address >= 0x3F00 && address < 0x4000)
                WritePalette(0x3F00 + (address & 0x001F), val);
        }

        /// <summary>
        /// Writes an entry in a nametable (considering the game mirroring setup).
        /// </summary>
        /// <param name="address">The address where the value should be stored.</param>
        /// <param name="val">The value that will be stored.</param>
        private void WriteNametable(uint address, byte val)
        {
            if (_mirroring == Mirroring.Vertical)
            {
                if ((address >= 0x2800 && address < 0x2C00) || (address >= 0x2C00 && address < 0x3000))
                    address -= 0x0400;
            }
            else // Horizontal
            {
                // NT 1 mirrors NT 0 and NT 3 mirrors NT 2
                if ((address >= 0x2400 && address < 0x2800) || (address >= 0x2C00 && address < 0x3000))
                    address -= 0x0400;

            }

            _vram[address & 0x07FF] = val;
        }

        /// <summary>
        /// Reads a nametable entry (considering the game mirroring setup).
        /// </summary>
        /// <param name="address">The address where the entry is allocated.</param>
        /// <returns>The value allocated in the given address.</returns>
        private byte ReadNametable(uint address)
        {
            if (_mirroring == Mirroring.Vertical)
            {
                if ((address >= 0x2800 && address < 0x2C00) || (address >= 0x2C00 && address < 0x3000))
                    address -= 0x0400;
            }
            else // Horizontal
            {
                // NT 1 mirrors NT 0 and NT 3 mirrors NT 2
                if ((address >= 0x2400 && address < 0x2800) || (address >= 0x2C00 && address < 0x3000))
                    address -= 0x0400;
            }

            return _vram[address & 0x07FF];
        }
    }
}
