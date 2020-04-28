using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    /// <summary>
    /// The 6502 CPU.
    /// </summary>
    class Cpu
    {
        #region 8-bits registers
        /// <summary>
        /// Accumulators.
        /// </summary>
        private Register<byte> _a;

        /// <summary>
        /// X register (general purpose).
        /// </summary>
        private Register<byte> _x;

        /// <summary>
        /// Y register (general purpose).
        /// </summary>
        private Register<byte> _y;

        /// <summary>
        /// Status register (each bit represents a flag).
        /// </summary>
        private Register<byte> _status;

        /// <summary>
        /// 
        /// </summary>
        private Register<byte> _stackPointer;
        #endregion

        #region 16-bits register
        /// <summary>
        /// The Program Counter register (holds the memory address of the next instruction).
        /// </summary>
        private Register<short> _programCounter;
        #endregion

        /// <summary>
        /// Represents the CPU's memory whose size is 2KiB (2,048 bytes).
        /// </summary>
        private readonly byte[] _memory = new byte[2048]; //Memory are organized in ranges (mappers)


    }
}
