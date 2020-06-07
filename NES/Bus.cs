using System;

namespace NES
{
    /// <summary>
    /// Communicates NES components each other.
    /// </summary>
    abstract class Bus
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
    class CpuBus : Bus
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

        public override byte Read(ushort address) => memory.Fetch(address);

        public override void Write(ushort address, byte val)
        {
            // Hardware RAM (NES)
            if (address >= 0x0000 && address < 0x800)
                WriteRam(address, val);
            else if ((address >= 0x2000 && address < 0x2008) || (address >= 0x4000 && address < 0x4020))
                WriteInputOutputRegisters(address, val);
        }

        /// <summary>
        /// Writes into the NES hardware RAM.
        /// </summary>
        /// <param name="address">The address where it should be written in the RAM.</param>
        /// <param name="val">The value that would be stored in the slot specified by the address within the RAM.</param>
        private void WriteRam(ushort address, byte val)
        {
            memory.Store(address, val);

            // Writes in the first mirror 0x0800 - 0x0FFF
            memory.Store((ushort)(address + FirstRamMirrorOffset), val);

            // Writes in the second mirror 0x1000 - x017FF
            memory.Store((ushort)(address + SecondRamMirrorOffset), val);

            // Writes in the third mirror 0x1800 - 0x1FFF
            memory.Store((ushort)(address + ThirdRamMirrorOffset), val);
        }

        private void WriteInputOutputRegisters(ushort address, byte val)
        {
            if (address >= 0x2000 && address < 0x2008)
                // Mirror the byte written every 8 bytes until 3FFF (inclusive)
                for (ushort i = address; i < 0x4000; i += 0x0008)
                    memory.Store(i, val);
            else
                memory.Store(address, val);
        }
    }

    /// <summary>
    /// PPU's bus.
    /// </summary>
    class PpuBus : Bus
    {
        public PpuBus(Memory memory): base(memory)
        {
        }

        public override byte Read(ushort address)
        {
            throw new NotImplementedException();
        }

        public override void Write(ushort address, byte val)
        {
            throw new NotImplementedException();
        }
    }
}
