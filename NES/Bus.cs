using System;

namespace NES
{
    /// <summary>
    /// Communicates NES components each other.
    /// </summary>
    abstract class Bus
    {
        protected Memory memory;

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

        public CpuBus()
        {
            memory = new Memory(ushort.MaxValue);
        }

        public override byte Read(ushort address) => memory.Fetch(address);

        public override void Write(ushort address, byte val)
        {
            // Hardware RAM (NES)
            if (address >= 0x0000 && address < 0x800)
                WriteRam(address, val);
            else if (address >= 0x2000 && address < 0x2008)
                WriteInputOutputRegisters(address, val);
        }

        private void WriteRam(ushort address, byte val)
        {
            // Writes in the first mirror 0x0800 - 0x0FFF
            memory.Store((ushort)(address + FirstRamMirrorOffset), val);

            // Writes in the second mirror 0x1000 - x017FF
            memory.Store((ushort)(address + SecondRamMirrorOffset), val);

            // Writes in the third mirror 0x1800 - 0x1FFF
            memory.Store((ushort)(address + ThirdRamMirrorOffset), val);
        }

        private void WriteInputOutputRegisters(ushort address, byte val)
        {

        }
    }

    /// <summary>
    /// PPU's bus.
    /// </summary>
    class PpuBus : Bus
    {
        public PpuBus()
        {
            memory = new Memory(0x4000);
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
