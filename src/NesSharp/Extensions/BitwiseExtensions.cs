using System;
using System.ComponentModel;

namespace NesSharp.Extensions
{
    /// <summary>
    /// Utilities for manipulate values bits.
    /// </summary>
    public static class BitwiseExtensions
    {

        /// <summary>
        /// Parse low byte and high byte in order to format it based on little endian format.
        /// </summary>
        /// <param name="lowByte">Low byte (least significant byte).</param>
        /// <param name="highByte">High byte (most significant byte).</param>
        /// <returns>A 16 bit value.</returns>
        public static ushort ParseBytes(byte lowByte, byte highByte) => (ushort)(lowByte | highByte << 8); // https://stackoverflow.com/questions/6090561/how-to-use-high-and-low-bytes

        /// <summary>
        /// Checks if the given value is negative (the bit no. 7 is "on").
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>True if it's negative; otherwise false.</returns>
        public static bool IsNegative(this byte val)
        {
            byte mask = 1 << 7;

            return (val & mask) == 0x0080;
        }

        /// <summary>
        /// Fetches the high byte of a 16 bit value.
        /// </summary>
        /// <param name="val">The 16 bit value.</param>
        /// <returns>The high byte (most significant byte).</returns>
        public static byte GetHighByte(this ushort val) => (byte)(val >> 8);

        /// <summary>
        /// Fetches the low byte of a 16 bit value.
        /// </summary>
        /// <param name="val">The 16 bit value.</param>
        /// <returns>The low byte (least significant byte).</returns>
        public static byte GetLowByte(this ushort val) => (byte)(val);

        /// <summary>
        /// Turn on/off the bit allocated in the position specified.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="bitPosition">Position of the bit (from 0 up to 7; otherwise an exception will be raised).</param>
        /// <param name="bitValue">The value that will be set to position of the source value: true = 1 , false = 0.</param>
        public static void SetBit(this ref byte value, byte bitPosition, bool bitValue)
        {
            int mask = 1 << bitPosition;

            int result;
            if (bitValue) // Turn on the bit
                result = value | mask;
            else // Turn off the bit
                result = ((value | mask) ^ mask); // Just in case the bit still ON

            value = (byte)result;
        }

        /// <summary>
        /// Retrieves the bit (as a boolean) stored in a specific position from the source value specified.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="bitPosition">The position of the bit within the source value.</param>
        /// <returns>True if bit is set to "1"; false if bit is set to "0".</returns>
        public static bool GetBit(this byte value, byte bitPosition)
        {
            int mask = 1 << bitPosition;
            int result = value & mask;

            return result == mask;
        }

        public static bool GetBit(this int value, int position)
        {
            int mask = 1 << position;
            int result = value & mask;

            return result == mask;
        }

        /// <summary>
        /// Sets a specified value into the high byte area of a 16-bit value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="val">The value that will be set in the high byte area.</param>
        public static void SetHighByte(this ref ushort value, byte val)
        {
            value = (ushort)((value & 0x00FF) | (val << 8));
        }

        /// <summary>
        /// Sets a specified value into the low byte area of a 16-bit value.
        /// </summary>
        /// <param name="value">The source value.</param>
        /// <param name="val">The value that will be set in the low byte area.</param>
        public static void SetLowByte(this ref ushort value, byte val)
        {
            value = (ushort)((value & 0xFF00) | val);
        }

        public static int Byte(this int value) => (byte)value;

        public static int Word(this int value) => (ushort)value;

        public static int MirrorBits(this int v)
        {
            int m = v;

            m = (m & 0xF0) >> 4 | (m & 0x0F) << 4;
            m = (m & 0xCC) >> 2 | (m & 0x33) << 2;
            m = (m & 0xAA) >> 1 | (m & 0x55) << 1;

            return m;
        }
    }
}
