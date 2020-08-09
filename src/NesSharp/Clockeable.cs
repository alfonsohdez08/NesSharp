using System;
using System.Collections.Generic;
using System.Text;

namespace NesSharp
{
    abstract class Clockeable
    {
        public int MasterClockCycles { get; set; }

        /// <summary>
        /// Runs a NES component for a certain amount of master clock cycles.
        /// </summary>
        /// <param name="masterClockCycles">The amount of master clock cycles.</param>
        public abstract void RunUpTo(int masterClockCycles);
    }
}
