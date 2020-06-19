using System;

namespace MiNES
{
    /// <summary>
    /// CPU's bus.
    /// </summary>
    public class CpuBus : Bus
    {
        /// <summary>
        /// The offset for first mirror of the CPU RAM.
        /// </summary>
        private const ushort FirstRamMirrorOffset = 2048;

        /// <summary>
        /// The offset for second mirror of the CPU RAM.
        /// </summary>
        private const ushort SecondRamMirrorOffset = 4096;

        /// <summary>
        /// The offset for third mirror of the CPU RAM.
        /// </summary>
        private const ushort ThirdRamMirrorOffset = 6144;

        /// <summary>
        /// Flag for determine whether writing either the low byte or high byte of the address into the PPUADDR register.
        /// </summary>
        /// <remarks>False: high byte; True: low byte.</remarks>
        private bool _addressLatch = false;


        public CpuBus(Memory memory):base(memory)
        {
        }

        public override byte Read(ushort address)
        {
            byte val;
            if (address >= 0x0000 && address < 0x2000)
                val = memory.Fetch((ushort)(address % 0x0800));
            else if (address >= 0x2000 && address < 0x4000)
            {
                ushort addr = (ushort)(0x2000 + address % 8);

                switch (addr)
                {
                    case 0x2000:
                        val = GetPpuControlRegister();
                        break;
                    case 0x2001:
                        val = GetPpuMaskRegister();
                        break;
                    case 0x2002:
                        val = GetPpuStatusRegister();
                        // Clears bit 7 after reading the PPU Status Register
                        memory.Store(addr, (byte)((val | 0x0080) ^ 0x0080));
                        break;
                    case 0x2003:
                        val = GetOamAddress();
                        break;
                    case 0x2004:
                        val = GetOamData();
                        break;
                    case 0x2005:
                        val = GetPpuScroll();
                        break;
                    case 0x2006:
                        val = GetPpuRamAddress();
                        break;
                    case 0x2007:
                        val = GetPpuRamValue();
                        break;
                    default:
                        throw new InvalidOperationException();
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

            //// Writes in the first mirror 0x0800 - 0x0FFF
            //memory.Store((ushort)(address + FirstRamMirrorOffset), val);

            //// Writes in the second mirror 0x1000 - x017FF
            //memory.Store((ushort)(address + SecondRamMirrorOffset), val);

            //// Writes in the third mirror 0x1800 - 0x1FFF
            //memory.Store((ushort)(address + ThirdRamMirrorOffset), val);
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
                //if (addr == 0x2002)
                //    throw new InvalidOperationException("The CPU can't write to the PPU status register.");

                memory.Store(addr, val); // Writes to the actual slot in memory (I/O registers are mirrored every 8 bytes)

                switch (addr)
                {
                    case 0x2000:
                        break;
                    case 0x2001:
                        break;
                    case 0x2003:
                        break;
                    case 0x2004:
                        break;
                    case 0x2005:
                        break;
                    case 0x2006:
                        {
                            
                        }
                        break;
                    case 0x2007:
                        break;
                }
            }
            else
                memory.Store(address, val);
        }

        public byte GetPpuControlRegister() => memory.Fetch(0x2000);

        public byte GetPpuMaskRegister() => memory.Fetch(0x2001);

        public byte GetPpuStatusRegister() => memory.Fetch(0x2002);

        public byte GetOamAddress() => memory.Fetch(0x2003);

        public byte GetOamData() => memory.Fetch(0x2004);

        public byte GetPpuScroll() => memory.Fetch(0x2005);

        public byte GetPpuRamAddress() => memory.Fetch(0x2006);

        public byte GetPpuRamValue() => memory.Fetch(0x2007);

        public byte GetOamDma() => memory.Fetch(0x4014);
    }
}
