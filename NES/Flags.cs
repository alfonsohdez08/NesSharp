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
        Interrupt = 2,
        B4 = 4,
        B5 = 5,
        Overflow = 6,
        Negative = 7
    }

    class Flags: Register<byte>
    {
        /// <summary>
        /// Initial value of the register (all flags disabled).
        /// </summary>
        private const byte FlagsDisabled = 0b00000000;

        public Flags(): base(FlagsDisabled)
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
            if (val)
                result = flags | mask;
            else
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

        /// <summary>
        /// Clears all flags.
        /// </summary>
        public void ClearFlags() => SetValue(FlagsDisabled);

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
