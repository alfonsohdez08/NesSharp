namespace NES
{
    /// <summary>
    /// Utilities for manipulate 8-bit values (byte).
    /// </summary>
    public static class ByteExtensions
    {

        /// <summary>
        /// Parse low byte and high byte in order to format it based on little endian format.
        /// </summary>
        /// <param name="lowByte">Low byte (least significant byte).</param>
        /// <param name="highByte">High byte (most significant byte).</param>
        /// <returns>A 16 bit value.</returns>
        public static ushort ParseBytes(byte lowByte, byte highByte)
        {
            //string highByteHex = highByte.ToString("x");
            //string lowByteHex = lowByte.ToString("x");

            // https://stackoverflow.com/questions/6090561/how-to-use-high-and-low-bytes
            return (ushort)(lowByte | highByte << 8);

            //return Convert.ToUInt16(highByteHex + lowByteHex, 16);
        }

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
    }
}
