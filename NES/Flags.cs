using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NES
{
    enum StatusFlag: byte
    {
        Carry = 0,
        Zero = 1,
        DisableInterrupt = 2,
        Decimal = 3,
        Overflow = 6,
        Negative = 7
    }

    class Flags: Register<byte>
    {
        /// <summary>
        /// Initial value of the register (all flags disabled).
        /// </summary>

#if CPU_NES_TEST
        private const byte DefaultFlags = 0x24;
#else
        private const byte DefaultFlags = 0x34;
#endif

        public Flags(): base(DefaultFlags)
        {

        }

        /// <summary>
        /// Either turn "on" or "off" the given CPU status flag.
        /// </summary>
        /// <param name="flag">The CPU status flag.</param>
        /// <param name="val">If true it means it should be "on"; otherwise "off".</param>
        public void SetFlag(StatusFlag flag, bool val)
        {
            byte flagPos = (byte)flag;
            int mask = 1 << flagPos;

            byte flags = GetValue();
            
            int result;
            if (val) // enable/turn on/set the bit
                result = flags | mask;
            else // disable/turn off the bit
                result = ((flags | mask) ^ mask); // Just in case the bit still ON

            SetValue((byte)result);
        }

        /// <summary>
        /// Gets the current value of the given CPU status flag.
        /// </summary>
        /// <param name="flag">The CPU status flag.</param>
        /// <returns>True if it's "on"; otherwise false.</returns>
        public bool GetFlag(StatusFlag flag)
        {
            byte flagPos = (byte)flag;
            int mask = 1 << flagPos;

            byte flags = GetValue();
            int result = flags & mask;

            return result == mask;
        }

        public override void SetValue(byte value)
        {
            /*
             * The bits 4 and 5 are known as B flags. There aren't instructions that affect those bits in the flags register, however they are set.
             * For the regular NES operation, both bits are set, but for the nestest, the bit 4 is not set (it's off).
             */
            int bit4Mask = 1 << 4;

            int bit5Mask = 1 << 5;
            value = (byte)(value | bit5Mask);
#if CPU_NES_TEST
            value = (byte)((value | bit4Mask) ^ bit4Mask); // Prevents setting the bit 4
#else
            value = (byte)(value | bit4Mask);
#endif
            base.SetValue(value);
        }

        /// <summary>
        /// Clears all flags.
        /// </summary>
        public void ClearFlags() => SetValue(DefaultFlags);

#if DEBUG
        /// <summary>
        /// Provides a more human readable form of the CPU flags.
        /// </summary>
        /// <returns>A string representing the current state of the flags.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            byte flags = GetValue();

            var e = Enum.GetValues(typeof(StatusFlag)).Cast<StatusFlag>();
            foreach (StatusFlag f in e)
            {
                int mask = (1 << (byte)f);
                sb.AppendLine($"{f}: {(flags & mask) == mask}");
            }

            return sb.ToString();
        }
#endif

    }
}
