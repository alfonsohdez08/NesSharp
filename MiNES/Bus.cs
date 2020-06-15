using System;

namespace MiNES
{
    /// <summary>
    /// Communicates NES components each other.
    /// </summary>
    public abstract class Bus
    {
        /// <summary>
        /// The memory space reachable/available for the BUS.
        /// </summary>
        protected readonly Memory memory;

        public Bus(Memory memory)
        {
            this.memory = memory;
        }

        public abstract byte Read(ushort address);
        public abstract void Write(ushort address, byte val);
    }

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

        public CpuBus(Memory memory):base(memory)
        {
        }

        public override byte Read(ushort address)
        {
            byte val;
            if (address >= 0x0000 && address < 0x2000)
                val = memory.Fetch((ushort)(address % 0x0800));
            else if (address >= 0x2000 && address < 0x4000)
                val = memory.Fetch((ushort)(0x2000 + address % 8));
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
            if (address >= 0x2000 && address < 0x4000)
                memory.Store((ushort)(0x2000 + address % 8), val); // Writes to the actual slot in memory (I/O registers are mirrored every 8 bytes)
            else
                memory.Store(address, val);
        }
    }

    /// <summary>
    /// PPU's bus.
    /// </summary>
    public class PpuBus : Bus
    {
        public PpuBus(Memory memory): base(memory)
        {
        }

        public override byte Read(ushort address)
        {
            // Nametables and attribute tables (mirrored in the range [0x3000, 0x3EFF])
            if (address >= 0x2000 && address < 0x3F00)
                return memory.Fetch((ushort)(0x2000 + address % 0x1000));
            // Background palette and sprite palletes (mirrored in the range [0x3F20, 0x3FFF])
            else if (address >= 0x3F00 && address < 0x4000)
                return memory.Fetch((ushort)(0x3F00 + address % 0x0020));
            // Mirror of everything allocated from 0x000 until 0x3FFF
            else if (address >= 0x4000)
                return this.Read((ushort)(address % 0x4000));

            return memory.Fetch(address);
        }

        public override void Write(ushort address, byte val)
        {
            // Nametables and attribute tables (mirrored in the range [0x3000, 0x3EFF])
            if (address >= 0x2000 && address < 0x3F00)
                memory.Store((ushort)(0x2000 + address % 0x1000), val);
            // Background palette and sprite palletes (mirrored in the range [0x3F20, 0x3FFF])
            else if (address >= 0x3F00 && address < 0x4000)
                memory.Store((ushort)(0x3F00 + address % 0x0020), val);
            // Mirror of everything allocated from 0x000 until 0x3FFF
            else if (address >= 0x4000)
                this.Write((ushort)(address % 0x4000), val);
            else
                memory.Store(address, val);
        }
    }
}
