using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Flags: Register<byte>
    {
        private const string FlagsLegend = "NVssDIZC";

        /// <summary>
        /// Initial value of the register (all flags disabled).
        /// </summary>
        private const byte FlagsDisabled = 0b00000000;

        public Flags(): base(FlagsDisabled)
        {

        }

        /// <summary>
        /// Controls the Carry flag.
        /// </summary>
        /// <param name="enable">If true, turn on the bit related to the flag; otherwise turn off the bit related to the flag.</param>
        public void Carry(bool enable = true) => Flag(0, enable);

        /// <summary>
        /// Controls the Zero flag.
        /// </summary>
        /// <param name="enable">If true, turn on the bit related to the flag; otherwise turn off the bit related to the flag.</param>
        public void Zero(bool enable = true) => Flag(1, enable);

        /// <summary>
        /// Controls the Interrupt flag.
        /// </summary>
        /// <param name="enable">If true, turn on the bit related to the flag; otherwise turn off the bit related to the flag.</param>
        public void InterruptDisable(bool enable = true) => Flag(2, enable);

        /// <summary>
        /// Controls the Break flag.
        /// </summary>
        /// <param name="enable">If true, turn on the bit related to the flag; otherwise turn off the bit related to the flag.</param>
        public void Break(bool enable = true) => Flag(4, enable);

        /// <summary>
        /// Controls the Overflow flag.
        /// </summary>
        /// <param name="enable">If true, turn on the bit related to the flag; otherwise turn off the bit related to the flag.</param>
        public void Overflow(bool enable = true) => Flag(6, enable);

        /// <summary>
        /// Controls the Negative flag.
        /// </summary>
        /// <param name="enable">If true, turn on the bit related to the flag; otherwise turn off the bit related to the flag.</param>
        public void Negative(bool enable = true) => Flag(7, enable);

        /// <summary>
        /// Clears all flags.
        /// </summary>
        public void ClearFlags() => SetValue(FlagsDisabled);

        /// <summary>
        /// Turn on/off a flag identified by the given bit position.
        /// </summary>
        /// <param name="bitPosition">The bit position (0-7).</param>
        /// <param name="enable">If true, turn on the bit; otherwise turn off.</param>
        private void Flag(byte bitPosition, bool enable)
        {
            if (bitPosition < 0 || bitPosition > 7)
                throw new ArgumentOutOfRangeException("Bit position out of range. Ensure position is in the range [0-7].");

            byte flags = GetValue();
            byte mask = (byte)(1 << bitPosition);

            if (enable)
                flags = (byte)(flags | mask);
            else
                flags = (byte)((byte)(flags | mask) ^ mask);

            SetValue(flags);
        }

#if DEBUG
        /// <summary>
        /// Provides a more human readable form of the CPU flags.
        /// </summary>
        /// <returns>A string representing the current state of the flags.</returns>
        public override string ToString()
        {
            string flags = Convert.ToString(GetValue(), 2);
            if (flags.Length < 8)
                flags = flags.PadLeft(8 - flags.Length, '0');

            var sb = new StringBuilder();
            sb.AppendLine(flags);
            sb.AppendLine(FlagsLegend);

            return sb.ToString();
        }
#endif

    }
}
