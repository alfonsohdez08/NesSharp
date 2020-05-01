using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Memory: IMemory
    {
        /// <summary>
        /// The memory size of the NES is 64KB and it's compound by a set of 256 pages, where each page is 256 bytes (each slot within a page is a byte).
        /// </summary>
        private readonly byte[] _memory = new byte[65535];

        public byte Fetch(ushort address) => _memory[address];

        public void Store(ushort address, byte value) => _memory[address] = value;
    }

    interface IMemory
    {
        /// <summary>
        /// Fetchs the value allocated in the given address in the memory.
        /// </summary>
        /// <param name="address">The 16 bit address (in hexadecimal).</param>
        /// <returns>The value (8 bit) allocated in the given address.</returns>
        byte Fetch(ushort address);
        
        /// <summary>
        /// Stores the given value (8-bit) in the memory identified by the given address.
        /// </summary>
        /// <param name="address">The 16 bit address (in hexadecimal).</param>
        /// <param name="value">The value that would be stored in the memory slot (8 bit value).</param>
        void Store(ushort address, byte value);
    }

    //public static class HexadecimalExtensions
    //{
    //    public static ushort ToDecimal(this ushort hexValue) => Convert.ToUInt16(hexValue.ToString(), 16);
    //}
}
