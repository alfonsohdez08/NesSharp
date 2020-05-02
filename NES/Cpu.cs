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
        private readonly Register<byte> _a = new Register<byte>();

        /// <summary>
        /// X register (general purpose).
        /// </summary>
        private readonly Register<byte> _x = new Register<byte>();

        /// <summary>
        /// Y register (general purpose).
        /// </summary>
        private readonly Register<byte> _y = new Register<byte>();

        /// <summary>
        /// Status register (each bit represents a flag).
        /// </summary>
        private readonly Flags _flags = new Flags();

        /// <summary>
        /// Holds the address of the outer 
        /// </summary>
        private readonly Register<byte> _stackPointer = new Register<byte>();
        #endregion

        #region 16-bits register
        /// <summary>
        /// The Program Counter register (holds the memory address of the next instruction).
        /// </summary>
        private readonly Register<ushort> _programCounter;
        
        private ushort _currentPcAddress => _programCounter.GetValue();
        #endregion

        /// <summary>
        /// The CPU's memory.
        /// </summary>
        private readonly IMemory _memory;

        /// <summary>
        /// Creates an instace of a 6502 CPU.
        /// </summary>
        /// <param name="memory">Memory with program already loaded.</param>
        /// <param name="startingAddress">Address where the program starts.</param>
        public Cpu(IMemory memory, ushort startingAddress = 0x0200)
        {
            _memory = memory;
            _programCounter = new Register<ushort>(startingAddress);
        }

        /// <summary>
        /// Executes the program loaded in the memory.
        /// </summary>
        public void Start()
        {
            // Each time a machine cycle is elapsed, the program counter would be incremeted
            while (Cycle())
                IncrementPC();
        }

        /// <summary>
        /// Executes a machine cycle (fetch, decode, execute instruction).
        /// </summary>
        private bool Cycle()
        {
            /*
                Remarks: The whole instruction is executed in a cycle; however, in reality, this is not true. An instruction could long multiple cycles (take more than
                once cycle). Learn more about this.
             */

            // Fetchs the OpCode from the memory
            byte opCode = _memory.Fetch(_currentPcAddress);

            // Decodes and executes the OpCode
            switch (opCode)
            {
                case 0xA9: //LDA (immediate addressing)
                    LDA_Immediate();
                    break;
                case 0x8D:
                    STA_Absolute(); //STA (absolute addressing)
                    break;
                case 0x69:
                    ADC_Immediate(); //ADC (immediate addressing)
                    break;
                case 0x00:
                    return false;
                default:
                    throw new NotSupportedException($"OpCode not supported: {opCode.ToString("X")}");
            }

            return true;
        }

        /// <summary>
        /// Increments the address allocated in the Program Counter.
        /// </summary>
        private void IncrementPC()
        {
            _programCounter.SetValue((ushort)(_currentPcAddress + 1));
        }

        private void ADC_Immediate()
        {
            IncrementPC();

            byte value = _memory.Fetch(_currentPcAddress);

            int result = _a.GetValue() + value;
            if (result >= 256) // Value is greater than a byte
                _flags.Carry();

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

            ushort absAddress = ByteManipulation.ParseBytes(lowByte, highByte);//Check if this is the way to merge two bytes (LOW_BYTE HIGH_BYTE)
            byte acValue = _a.GetValue();

            _memory.Store(absAddress, acValue);
        }
    }


    public static class ByteManipulation
    {

        /// <summary>
        /// Parse low byte and high byte in order to format it based on little endian format.
        /// </summary>
        /// <param name="lowByte">Low byte (least significant byte).</param>
        /// <param name="highByte">High byte (most significant byte).</param>
        /// <returns>A 16 bit value parsed in the little endian format.</returns>
        public static ushort ParseBytes(byte lowByte, byte highByte)
        {
            string highByteHex = highByte.ToString("x");
            string lowByteHex = lowByte.ToString("x");

            // 6502 CPU is little endian (LOW_BYTE HIGH_BYTE)
            return Convert.ToUInt16(lowByteHex + highByteHex, 16);
        }
    }
}
