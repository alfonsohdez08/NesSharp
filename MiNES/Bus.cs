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
}
