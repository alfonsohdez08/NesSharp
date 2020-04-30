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
        private Register<byte> _status = new Register<byte>(0b00000000);

        /// <summary>
        /// Holds the address of the outer 
        /// </summary>
        private Register<byte> _stackPointer;
        #endregion

        #region 16-bits register
        /// <summary>
        /// Starting address for the Program Counter when the CPU is started.
        /// </summary>
        private const ushort PcStartingAddress = 0x0200;

        /// <summary>
        /// The Program Counter register (holds the memory address of the next instruction).
        /// </summary>
        private readonly Register<ushort> _programCounter = new Register<ushort>(PcStartingAddress);
        
        private ushort _currentPcAddress => _programCounter.GetValue();
        #endregion

        /// <summary>
        /// The CPU's memory.
        /// </summary>
        private readonly IMemory _memory;

        public Cpu(IMemory memory)
        {
            _memory = memory;
        }

        /// <summary>
        /// Executes a machine cycle (fetch, decode, execute instruction).
        /// </summary>
        public void Cycle()
        {
            byte opCode;

            do
            {
                // Fetchs the OpCode from the memory
                opCode = _memory.Fetch(_currentPcAddress);

                // Decodes and executes the OpCode
                switch(opCode)
                {
                    case 0xA9: //LDA (immediate addressing)
                        LDA_Immediate();
                        break;
                    case 0x8D:
                        STA_Absolute(); //STA (absolute addressing)
                        break;
                    default:
                        throw new NotSupportedException($"OpCode not supported: {opCode.ToString("X")}");
                }

                IncrementPC();
            } while (opCode != 0x00);
        }

        /// <summary>
        /// Increments the address allocated in the Program Counter.
        /// </summary>
        private void IncrementPC()
        {
            _programCounter.SetValue((ushort)(_currentPcAddress + 0x1));
        }

        /// <summary>
        /// Loads a given value into the accumulator (LDA); it uses immediate address (the literal value is passed as argument to the instruction).
        /// </summary>
        private void LDA_Immediate()
        {
            IncrementPC();
            
            byte value = _memory.Fetch(_currentPcAddress);
            _a.SetValue(value);
        }


        /// <summary>
        /// Stores the accumulator value (STA) in a given address (absolute address).
        /// </summary>
        private void STA_Absolute()
        {
            IncrementPC();
            byte lowByte = _memory.Fetch(_currentPcAddress);

            IncrementPC();
            byte highByte = _memory.Fetch(_currentPcAddress);

            ushort absAddress = (ushort)(lowByte + highByte); //Check if this is the way to merge two bytes (LOW_BYTE HIGH_BYTE)            
            byte acValue = _a.GetValue();

            _memory.Store(absAddress, acValue); // I think when a value is set to the accumulator, it should apply a math operation (either add or substraction based on the sign)
        }
    }
}
