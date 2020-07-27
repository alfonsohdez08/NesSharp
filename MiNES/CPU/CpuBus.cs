using MiNES.PPU;
using System;

namespace MiNES.CPU
{
    /// <summary>
    /// CPU's bus.
    /// </summary>
    public class CpuBus
    {
        private readonly Ppu _ppu;

        /// <summary>
        /// Acknowledges the CPU for start transfering the OAM data into the PPU OAM.
        /// </summary>
        public bool DmaTransferTriggered { get; set; } = false;

        /// <summary>
        /// The page within memory RAM where the OAM data resides.
        /// </summary>
        public byte OamMemoryPage { get; private set; }

        private readonly byte[] _ram = new byte[2 * 1024];
        private readonly byte[] _programRom;
        private readonly Joypad _joypad;

        /// <summary>
        /// Creates an instance of the bus used by the CPU.
        /// </summary>
        /// <param name="memory">The space for allocate RAM and other "stuffs".</param>
        /// <param name="ppu">The PPU (the CPU reads/writes to the PPU registers by using any of the addresses in this range $2000-$2007).</param>
        public CpuBus(byte[] programRom, Ppu ppu, Joypad joypad)
        {
            _programRom = programRom;
            _ppu = ppu;
            _joypad = joypad;
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
                int bit = (_joypad.Register & 0x80) == 0x80 ? 1 : 0;
                val = (byte)bit;

                if (!_joypad.Poll)
                {
                    _joypad.Register <<= 1;
                    _joypad.Register &= 0xFF;
                }
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
                    value = (byte)_ppu.Status.Status;
                    
                    // Side effects of reading the status register
                    _ppu.Status.VerticalBlank = false; // Clears bit 7 (V-BLANK) flag after CPU read the status register
                    _ppu.ResetAddressLatch();
                    break;

                // PPU OAM address register (write only)
                case 0x2003:
                    break;

                // PPU OAM data register
                case 0x2004:
                    //value = _ppu.OamData;
                    value = (byte)_ppu.GetOamData();
                    break;

                // PPU Scroll register (write only)
                case 0x2005:
                    break;

                // PPU Address register (write only)
                case 0x2006:
                    break;

                // PPU Data register
                case 0x2007:
                    value = (byte)_ppu.GetPpuData();
                    break;
                //default:
                //    throw new InvalidOperationException($"The address {address.ToString("X")} is not mapped to any PPU register.");
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
                // The value written to this port is the page within the CPU RAM where a copy of the OAM resides
                DmaTransferTriggered = true;
                OamMemoryPage = val;
            }
            else if (address == 0x4016)
            {
                _joypad.Poll = val == 1;
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
                    _ppu.Control.Control = value;
                    _ppu.T.Nametable = value & 3;

                    break;

                // PPU Mask register (write only)
                case 0x2001:
                    _ppu.Mask.Mask = value;
                    break;

                // PPU Status register (read only)
                case 0x2002:
                    break;

                // PPU OAM address register (write only)
                case 0x2003:
                    _ppu.OamAddress = value;
                    break;

                // PPU OAM data register
                case 0x2004:
                    //_ppu.OamData = value;
                    _ppu.SetOamData(value);
                    break;
                // PPU Scroll register (write only)
                case 0x2005:
                    _ppu.SetScroll(value); // First write: sets fine X and coarse X; Second write: sets fine Y and coarse Y
                    break;
                // PPU Address register (write only)
                case 0x2006:
                    _ppu.SetAddress(value); // First write: high byte of the address; Second write: low byte of the address
                    break;
                // PPU Data register
                case 0x2007:
                    _ppu.SetPpuData(value); // The value that will be stored in the address set by the PPU address register
                    break;
            }
        }
    }
}
