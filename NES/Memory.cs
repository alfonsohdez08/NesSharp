using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Memory
    {
        /// <summary>
        /// The NES memory.
        /// </summary>
        private readonly byte[] _memory;

        /// <summary>
        /// Fetches the value allocated in the given memory address.
        /// </summary>
        /// <param name="address">The memory address.</param>
        /// <returns>The value allocated in the given address</returns>
        public byte Fetch(ushort address) => _memory[address];

        public Memory(ushort length)
        {
            _memory = new byte[length];
        }

        /// <summary>
        /// Stores the given byte into a memory slot specified by the given memory address.
        /// </summary>
        /// <param name="address">The memory address.</param>
        /// <param name="value">The value that would be stored in the memory slot.</param>
        public void Store(ushort address, byte value) => _memory[address] = value;

        /// <summary>
        /// Creates an empty array of bytes that represents an empty memory for the NES.
        /// </summary>
        /// <returns>An empty array of bytes that represents an empty memory for the NES.</returns>
        public static byte[] CreateEmptyMemory() => new byte[65536];

        //public static Memory LoadRom(byte[] cartridge)
        //{
        //    byte[] rom = new ArraySegment<byte>(cartridge, 0x0010, 0x4000).ToArray();

        //    var memory = new Memory();
        //    ushort bank1Address = 0xC000;
        //    ushort bank2Address = 0x8000;

        //    for (int i = 0; i < rom.Length; i++, bank1Address++, bank2Address++)
        //    {
        //        memory.Store(bank1Address, rom[i]);
        //        memory.Store(bank2Address, rom[i]);
        //    }

        //    return memory;
        //}
    }
}
