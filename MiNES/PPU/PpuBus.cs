using System;
using System.Diagnostics.Tracing;

namespace MiNES.PPU
{

    /// <summary>
    /// PPU's bus.
    /// </summary>
    public class PpuBus : Bus
    {
        private readonly Mirroring _mirroring;

        public PpuBus(Memory memory, Mirroring mirroring): base(memory)
        {
            _mirroring = mirroring;
        }

        public override byte Read(ushort address)
        {
            // Nametables and attribute tables (mirrored in the range [0x3000, 0x3EFF])
            if (address >= 0x2000 && address < 0x3F00)
                return ReadNametable((ushort)(0x2000 + address % 0x1000));
            // Background palette and sprite palletes (mirrored in the range [0x3F20, 0x3FFF])
            else if (address >= 0x3F00 && address < 0x4000)
                return ReadPalette((ushort)(0x3F00 + address % 0x0020));
            // Mirror of everything allocated from 0x000 until 0x3FFF
            else if (address >= 0x4000)
                return this.Read((ushort)(address % 0x4000));

            return memory.Fetch(address);
        }

        private byte ReadPalette(ushort address)
        {
            //if (address == 0x3F04 || address == 0x3F08 || address == 0x3F0C || address == 0X3F10)
            //    address = 0x3F00;

            return memory.Fetch(address);
        }

        private void WritePalette(ushort address, byte paletteEntry)
        {
            //if (address == 0x3F04 || address == 0x3F08 || address == 0x3F0C || address == 0X3F10)
            //    address = 0x3F00;

            memory.Store(address, paletteEntry);
        }

        public override void Write(ushort address, byte val)
        {
            // Nametables and attribute tables (mirrored in the range [0x3000, 0x3EFF])
            if (address >= 0x2000 && address < 0x3F00)
                WriteNametable((ushort)(0x2000 + address % 0x1000), val);
            // Background palette and sprite palletes (mirrored in the range [0x3F20, 0x3FFF])
            else if (address >= 0x3F00 && address < 0x4000) //TODO: check the behavior when writing into this address from the CPU
                WritePalette((ushort)(0x3F00 + address % 0x0020), val);
            // Mirror of everything allocated from 0x000 until 0x3FFF
            else if (address >= 0x4000)
                this.Write((ushort)(address % 0x4000), val);
            else
                memory.Store(address, val);
        }

        /// <summary>
        /// Writes an entry in a nametable (considering the game mirroring setup).
        /// </summary>
        /// <param name="address">The address where the value should be stored.</param>
        /// <param name="val">The value that will be stored.</param>
        private void WriteNametable(ushort address, byte val)
        {
            //TODO: This logic can be done much better!
            ushort baseAddress = address;
            ushort offset = (ushort)(address % 0x0400);
            if (_mirroring == Mirroring.Vertical)
            {
                if (address >= 0x2800 && address < 0x2C00)
                    baseAddress = 0x2000;
                else if (address >= 0x2C00 && address < 0x3000)
                    baseAddress = 0x2400;
                else
                    offset = 0x0000;
            }
            else // Horizontal
            {
                if (address >= 0x2400 && address < 0x2800)                
                    baseAddress = 0x2000;
                else if (address >= 0x2C00 && address < 0x3000)
                    baseAddress = 0x2800;
                else
                    offset = 0x0000;
            }

            memory.Store((ushort)(baseAddress + offset), val);
        }

        /// <summary>
        /// Reads a nametable entry (considering the game mirroring setup).
        /// </summary>
        /// <param name="address">The address where the entry is allocated.</param>
        /// <returns>The value allocated in the given address.</returns>
        private byte ReadNametable(ushort address)
        {
            ushort baseAddress = address;
            ushort offset = (ushort)(address % 0x0400);
            if (_mirroring == Mirroring.Vertical)
            {
                if (address >= 0x2800 && address < 0x2C00)
                    baseAddress = 0x2000;
                else if (address >= 0x2C00 && address < 0x3000)
                    baseAddress = 0x2400;
                else
                    offset = 0x0000;
            }
            else // Horizontal
            {
                if (address >= 0x2400 && address < 0x2800)
                    baseAddress = 0x2000;
                else if (address >= 0x2C00 && address < 0x3000)
                    baseAddress = 0x2800;
                else
                    offset = 0x0000;
            }

            return memory.Fetch((ushort)(baseAddress + offset));
        }
    }
}
