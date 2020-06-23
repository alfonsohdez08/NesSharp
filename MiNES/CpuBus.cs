using System;

namespace MiNES
{
    /// <summary>
    /// CPU's bus.
    /// </summary>
    public class CpuBus : Bus
    {
        private readonly Ppu _ppu;

        /// <summary>
        /// Creates an instance of the bus used by the CPU.
        /// </summary>
        /// <param name="memory">The space for allocate RAM and other "stuffs".</param>
        /// <param name="ppu">The PPU (the CPU reads/writes to the PPU registers by using any of the addresses in this range $2000-$2007).</param>
        public CpuBus(Memory memory, Ppu ppu):base(memory)
        {
            _ppu = ppu;
        }

        public override byte Read(ushort address)
        {
            byte val;
            if (address >= 0x0000 && address < 0x2000)
                val = memory.Fetch((ushort)(address % 0x0800));
            else if (address >= 0x2000 && address < 0x4000) // PPU registers
            {
                ushort addr = (ushort)(0x2000 + address % 8);
                switch (addr)
                {
                    // PPU Status register
                    case 0x2002:
                        val = _ppu.Status.GetValue();
                        // Clears bit 7 after reading the PPU Status Register
                        //_ppu.Status = (byte)((val | 0x0080) ^ 0x0080);
                        _ppu.Status.SetVerticalBlank(false);
                        _ppu.ResetAddressLatch();
                        break;
                    // PPU OAM data register
                    case 0x2004:
                        val = _ppu.OamData;
                        break;
                    // PPU data register
                    case 0x2007:
                        val = _ppu.GetPpuData();
                        break;
                    default:
                        return 1; // dummy value
                }
            }
            else
                val = memory.Fetch(address);

            return val;
        }

        public override void Write(ushort address, byte val)
        {
            // Hardware RAM (NES)
            if (address >= 0x0000 && address < 0x2000)
                WriteRam(address, val);
            else if (address >= 0x2000 && address < 0x4020)
                WriteInputOutputRegisters(address, val);
        }

        /// <summary>
        /// Writes into the NES hardware RAM.
        /// </summary>
        /// <param name="address">The address where it should be written in the RAM.</param>
        /// <param name="val">The value that would be stored in the slot specified by the address within the RAM.</param>
        private void WriteRam(ushort address, byte val)
        {
            memory.Store((ushort)(address % 0x0800), val);
        }

        private void WriteInputOutputRegisters(ushort address, byte val)
        {
            /*
                TODO: BIG NOTE FOR YOU:

                The address range [0x2000, 0x2007] from addressable range for the CPU are the locations for the PPU registers. This means
                that by writing/reading those locations, YOU ARE ACTUALLY READING A REGISTER (as you are doing with the CPU's Accumulator register for instance).
                So each read/write, should affect the memory object allocated for the PPU. That was my mistake!!! I was writing to the CPU memory object, instead
                of writting to the actual PPU memory object.

                I can relate this CpuBus class with the PPU, meaning that the CpuBus has reference to the PPU: they are associated (elaborate more this sentence btw). I
                can also static fields in case I don't want a relationship between these two components.
                
                Remember that if something is in the address range, it doesn't mean it should belong to Memory object. I'm using memory object for represent RAM basically,
                but it shouldn't be like this! Think more about this! There's some notes that I made, check them. This logic could apply to more components either from the
                CPU or PPU (for instance, the PPU has 2kB, and it fits into its address range; however, there are more components that are in his address range and are registers...
                not slot in memory ram).
            */

            if (address >= 0x2000 && address < 0x4000)
            {
                ushort addr = (ushort)(0x2000 + address % 8);
                switch (addr)
                {
                    case 0x2000:
                        //_ppu.Control = val;
                        _ppu.Control.SetValue(val);
                        break;
                    case 0x2001:
                        _ppu.Mask = val;
                        break;
                    case 0x2003:
                        _ppu.OamAddress = val;
                        break;
                    case 0x2004:
                        _ppu.OamData = val;
                        break;
                    case 0x2005:
                        // TODO: check address latch here
                        _ppu.Scroll = val;
                        break;
                    case 0x2006:
                        _ppu.Address = val;
                        break;
                    case 0x2007:
                        _ppu.SetPpuData(val);
                        break;
                }
            }
            else
                memory.Store(address, val);
        }
    }
}
