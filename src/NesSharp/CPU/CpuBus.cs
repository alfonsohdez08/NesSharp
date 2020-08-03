using NesSharp.PPU;

namespace NesSharp.CPU
{

    /// <summary>
    /// The CPU's bus.
    /// </summary>
    class CpuBus: IBus
    {
        private readonly Ppu _ppu;
        private readonly Joypad _joypad;
        private readonly byte[] _ram = new byte[2 * 1024];
        private readonly byte[] _programRom;
        private readonly DMA _dma;

        public CpuBus(byte[] programRom, Ppu ppu, Joypad joypad, DMA dma)
        {
            _programRom = programRom;
            _ppu = ppu;
            _joypad = joypad;
            _dma = dma;
        }

        public byte Read(ushort address)
        {
            byte val = 0;
            // 2KB RAM mirrored
            if (address >= 0x0000 && address < 0x2000)
                val = _ram[address & 0x07FF];
            else if (address >= 0x2000 && address < 0x4000) // PPU registers
                val = ReadPpuRegister((ushort)(0x2000 + (address & 7)));
            else if (address == 0x4016)
            {
                val = (byte)_joypad.ReadState();

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
        private byte ReadPpuRegister(ushort address)
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
                    value = _ppu.OamData;
                    break;
                // PPU Scroll register (write only)
                case 0x2005:
                    break;
                // PPU Address register (write only)
                case 0x2006:
                    break;
                // PPU Data register
                case 0x2007:
                    value = _ppu.PpuData;
                    break;
            }

            return value;
        }

        public void Write(ushort address, byte val)
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
                _dma.Invoke(val, _ram);
            }
            else if (address == 0x4016)
            {
                _joypad.Strobe(val == 1);
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
                    _ppu.OamData = value;
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
                    _ppu.PpuData = value; // The value that will be stored in the address set by the PPU address register
                    break;
            }
        }
    }
}
