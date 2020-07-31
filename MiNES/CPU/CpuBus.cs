using MiNES.PPU;
using System;

namespace MiNES.CPU
{
    /// <summary>
    /// CPU's bus.
    /// </summary>
    public class CpuBus
    {
        private readonly NES _nes;
        private readonly byte[] _ram = new byte[2 * 1024];
        private readonly byte[] _programRom;

        public CpuBus(byte[] programRom, NES nes)
        {
            _programRom = programRom;
            _nes = nes;
        }

        public byte Read(uint address)
        {
            byte val = 0;
            // 2KB RAM mirrored
            if (address >= 0x0000 && address < 0x2000)
                val = _ram[address & 0x07FF];
            else if (address >= 0x2000 && address < 0x4000) // PPU registers
                val = ReadPpuRegister((ushort)(0x2000 + (address & 7)));
            else if (address == 0x4016)
            {
                val = (byte)_nes.Joypad.ReadState();

                //int bit = (_joypad._incomingData & 0x80) == 0x80 ? 1 : 0;
                //val = (byte)bit;

                //if (!_joypad.Strobe)
                //{
                //    _joypad._incomingData <<= 1;
                //    _joypad._incomingData &= 0xFF;
                //}
            }
            else if (address >= 0x8000 & address <= 0xFFFF)
                val = _programRom[address & 0x7FFF];

            return val;
        }

        /// <summary>
        /// Reads the PPU registers available in the addresses $2000-$2007 from the CPU memory map.
        /// </summary>
        /// <param name="address">The PPU register address.</param>
        /// <returns>The value allocated in the register identified by the given address.</returns>
        private byte ReadPpuRegister(uint address)
        {
            byte value = 0;
            switch(address)
            {
                // PPU Control register (write only)
                case 0x2000:
                    break;

                // PPU Mask register (write only)
                case 0x2001:
                    break;

                // PPU Status register
                case 0x2002:
                    value = (byte)_nes.Ppu.Status.Status;
                    
                    // Side effects of reading the status register
                    _nes.Ppu.Status.VerticalBlank = false; // Clears bit 7 (V-BLANK) flag after CPU read the status register
                    _nes.Ppu.ResetAddressLatch();
                    break;
                // PPU OAM address register (write only)
                case 0x2003:
                    break;
                // PPU OAM data register
                case 0x2004:
                    //value = _ppu.OamData;
                    value = _nes.Ppu.GetOamData();
                    break;
                // PPU Scroll register (write only)
                case 0x2005:
                    break;
                // PPU Address register (write only)
                case 0x2006:
                    break;
                // PPU Data register
                case 0x2007:
                    value = (byte)_nes.Ppu.GetPpuData();
                    break;
            }

            return value;
        }

        public void Write(uint address, byte val)
        {
            // Hardware RAM (NES)
            if (address >= 0x0000 && address < 0x2000)
                _ram[address & 0x07FF] = val;
            else if (address >= 0x2000 && address < 0x4000)
                //WritePpuRegister((ushort)(0x2000 + address % 8), val);
                WritePpuRegister((ushort)(0x2000 + (address & 7)), val);
            // DMA port
            else if (address == 0x4014)
            {
                WriteToOamBuffer(val);
            }
            else if (address == 0x4016)
            {
                _nes.Joypad.Strobe(val == 1);
                //_joypad.Strobe = val == 1;
            }
        }

        /// <summary>
        /// Writes a value into one of the 8 PPU registers mapped to the CPU addresses.
        /// </summary>
        /// <param name="address">the address of the PPU register.</param>
        /// <param name="value">The value that will be stored.</param>
        private void WritePpuRegister(ushort address, byte value)
        {
            switch (address)
            {
                // PPU Control register (write only)
                case 0x2000:
                    _nes.Ppu.Control.Control = value;
                    _nes.Ppu.T.Nametable = value & 3;
                    break;
                // PPU Mask register (write only)
                case 0x2001:
                    _nes.Ppu.Mask.Mask = value;
                    break;
                // PPU Status register (read only)
                case 0x2002:
                    break;
                // PPU OAM address register (write only)
                case 0x2003:
                    _nes.Ppu.OamAddress = value;
                    break;
                // PPU OAM data register
                case 0x2004:
                    _nes.Ppu.SetOamData(value);
                    break;
                // PPU Scroll register (write only)
                case 0x2005:
                    _nes.Ppu.SetScroll(value); // First write: sets fine X and coarse X; Second write: sets fine Y and coarse Y
                    break;
                // PPU Address register (write only)
                case 0x2006:
                    _nes.Ppu.SetAddress(value); // First write: high byte of the address; Second write: low byte of the address
                    break;
                // PPU Data register
                case 0x2007:
                    _nes.Ppu.SetPpuData(value); // The value that will be stored in the address set by the PPU address register
                    break;
            }
        }

        public void WriteToOamBuffer(byte page)
        {
            byte[] buffer = new ArraySegment<byte>(_ram, page << 8, 256).ToArray();
            _nes.Ppu.OamBuffer = buffer;
            
            int oamAddress = _nes.Ppu.OamAddress;
            oamAddress += 0x100;
            _nes.Ppu.OamAddress = (byte)oamAddress;

            _nes.Cpu.TicksAccumulated = _nes.Cpu.TicksElapsed % 2 != 0 ? 514 : 513;
        }

    }
}
