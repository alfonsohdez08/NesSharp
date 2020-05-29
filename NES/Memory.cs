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
        /// Fetches the value allocated in the given memory address.
        /// </summary>
        /// <param name="address">The memory address.</param>
        /// <returns>The value allocated in the given address</returns>
        public byte Fetch(ushort address) => _memory[address];

        public Memory(byte[] memoryMapped)
        {
            if (memoryMapped == null || memoryMapped.Length > 65536)
                throw new ArgumentException(nameof(memoryMapped));

            _memory = memoryMapped;
        }

        /// <summary>
        /// Stores the given byte into a memory slot specified by the given memory address.
        /// </summary>
        /// <param name="address">The memory address.</param>
        /// <param name="value">The value that would be stored in the memory slot.</param>
        public void Store(ushort address, byte value)
        {
            _memory[address] = value;

            // Writes in the RAM mirrors
            if (address >= 0x0000 && address <= 0x07FF)
            {
                // Writes in the first mirror 0x0800 - 0x0FFF
                _memory[address + FirstRamMirrorOffset] = value;

                // Writes in the second mirror 0x1000 - x017FF
                _memory[address + SecondRamMirrorOffset] = value;

                // Writes in the third mirror 0x1800 - 0x1FFF
                _memory[address + ThirdRamMirrorOffset] = value;
            }
        }

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
