using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{

    enum AddressingMode
    {
        ZeroPage,
        ZeroPageX,
        ZeroPageY,
        Immediate,
        Relative,
        Absolute,
        AbsoluteX,
        AbsoluteY,
        Indirect,
        IndexedIndirect,
        IndirectIndexed
    }

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
        
        private ushort _pcAddress => _programCounter.GetValue();
        #endregion

        /// <summary>
        /// The CPU's memory.
        /// </summary>
        private readonly IMemory _memory;

        /// <summary>
        /// Instruction's operand memory address (the location in memory where resides the instruction's operand).
        /// </summary>
        private ushort _operandAddress; 

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
            byte opCode = _memory.Fetch(_pcAddress);

            // Decodes and executes the OpCode
            switch (opCode)
            {
                case 0xA9:
                    FetchOperand(AddressingMode.Immediate);
                    LDA();
                    break;
                case 0xA5:
                    FetchOperand(AddressingMode.ZeroPage);
                    LDA();
                    break;
                case 0xB5:
                    FetchOperand(AddressingMode.ZeroPageX);
                    LDA();
                    break;
                case 0xAD:
                    FetchOperand(AddressingMode.Absolute);
                    LDA();
                    break;
                case 0xBD:
                    FetchOperand(AddressingMode.AbsoluteX);
                    LDA();
                    break;
                case 0xB9:
                    FetchOperand(AddressingMode.AbsoluteY);
                    LDA();
                    break;
                case 0xA1:
                    FetchOperand(AddressingMode.IndexedIndirect);
                    LDA();
                    break;
                case 0xB1:
                    FetchOperand(AddressingMode.IndirectIndexed);
                    LDA();
                    break;
                case 0x69:
                    FetchOperand(AddressingMode.Immediate);
                    ADC();
                    break;
                case 0x65:
                    FetchOperand(AddressingMode.ZeroPage);
                    ADC();
                    break;
                case 0x75:
                    FetchOperand(AddressingMode.ZeroPageX);
                    ADC();
                    break;
                case 0x6D:
                    FetchOperand(AddressingMode.Absolute);
                    ADC();
                    break;
                case 0x7D:
                    FetchOperand(AddressingMode.AbsoluteX);
                    ADC();
                    break;
                case 0x79:
                    FetchOperand(AddressingMode.AbsoluteY);
                    ADC();
                    break;
                case 0x61:
                    FetchOperand(AddressingMode.IndexedIndirect);
                    ADC();
                    break;
                case 0x71:
                    FetchOperand(AddressingMode.IndirectIndexed);
                    ADC();
                    break;
                case 0xE9:
                    FetchOperand(AddressingMode.Immediate);
                    SBC();
                    break;
                case 0xE5:
                    FetchOperand(AddressingMode.ZeroPage);
                    SBC();
                    break;
                case 0xF5:
                    FetchOperand(AddressingMode.ZeroPageX);
                    SBC();
                    break;
                case 0xED:
                    FetchOperand(AddressingMode.Absolute);
                    SBC();
                    break;
                case 0xFD:
                    FetchOperand(AddressingMode.AbsoluteX);
                    SBC();
                    break;
                case 0xF9:
                    FetchOperand(AddressingMode.AbsoluteY);
                    SBC();
                    break;
                case 0xE1:
                    FetchOperand(AddressingMode.IndexedIndirect);
                    SBC();
                    break;
                case 0xF1:
                    FetchOperand(AddressingMode.IndirectIndexed);
                    SBC();
                    break;
                case 0x38:
                    SEC();
                    break;
                case 0x18:
                    CLC();
                    break;
                case 0x85:
                    FetchOperand(AddressingMode.ZeroPage);
                    STA();
                    break;
                case 0x00:
                    return false;
                default:
                    throw new NotSupportedException($"OpCode not supported: {opCode.ToString("X")}");
            }

            return true;
        }

        #region Addressing modes

        /// <summary>
        /// Fetchs the instruction operand based on the instruction addressing mode.
        /// </summary>
        /// <param name="mode">The instruction's addressing mode.</param>
        private void FetchOperand(AddressingMode mode)
        {
            ushort operandAddress;
            
            IncrementPC();

            switch (mode)
            {
                case AddressingMode.ZeroPage:
                    operandAddress = _memory.Fetch(_pcAddress);
                    break;
                case AddressingMode.ZeroPageX:
                    operandAddress = (byte)(_memory.Fetch(_pcAddress) + _x.GetValue()); // If carry in the high byte (result greater than 255), requires an additiona cycle
                    break;
                case AddressingMode.ZeroPageY:
                    operandAddress = (byte)(_memory.Fetch(_pcAddress) + _y.GetValue()); // If carry in the high byte (result greater than 255), requires an additiona cycle
                    break;
                case AddressingMode.Immediate:
                case AddressingMode.Relative:
                    operandAddress = _pcAddress;
                    break;
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.Indirect:
                    ushort addressParsed;
                    {
                        byte lowByte = _memory.Fetch(_pcAddress);

                        IncrementPC();

                        byte highByte = _memory.Fetch(_pcAddress);

                        addressParsed = ByteExtensions.ParseBytes(lowByte, highByte);
                    }
                    if (mode == AddressingMode.Indirect)
                    {
                        // The content located in the address parsed is the LSB (Least Significant Byte) of the target address
                        byte lowByte = _memory.Fetch(addressParsed++);
                        byte highByte = _memory.Fetch(addressParsed);

                        operandAddress = ByteExtensions.ParseBytes(lowByte, highByte);
                    }
                    else if (mode == AddressingMode.Absolute)
                        operandAddress = addressParsed;
                    else if (mode == AddressingMode.AbsoluteX)
                        operandAddress = (ushort)(addressParsed + _x.GetValue());
                    else
                        operandAddress = (ushort)(addressParsed + _y.GetValue());
                    break;
                case AddressingMode.IndexedIndirect:
                    {
                        byte address = (byte)(_memory.Fetch(_pcAddress) + _x.GetValue()); // Wraps around the zero page

                        byte lowByte = _memory.Fetch(address++);
                        byte highByte = _memory.Fetch(address);

                        operandAddress = ByteExtensions.ParseBytes(lowByte, highByte);
                    }
                    break;
                case AddressingMode.IndirectIndexed:
                    {
                        byte zeroPageAddress = _memory.Fetch(_pcAddress);

                        byte lowByte = _memory.Fetch(zeroPageAddress++);
                        byte highByte = _memory.Fetch(zeroPageAddress);

                        operandAddress = (ushort)(ByteExtensions.ParseBytes(lowByte, highByte) + _y.GetValue());
                    }
                    break;
                default:
                    throw new NotImplementedException($"The given addressing mode is not supported: {mode.ToString()}.");
            }

            _operandAddress = operandAddress;
        }

        #endregion

        /// <summary>
        /// Increments the address allocated in the Program Counter.
        /// </summary>
        private void IncrementPC()
        {
            _programCounter.SetValue((ushort)(_pcAddress + 1));
        }

        /// <summary>
        /// Adds a value to the accumulator's value.
        /// </summary>
        private void ADC()
        {            
            /*
             * Overflow happens when the result can't fit into a signed byte: RESULT < -128 OR RESULT > 127
             * In order to this happen, both operands should hava the same sign: same sign operands
             * produces a bigger magnitude (the magnitude of a signed number is the actual number next to the sign: +5 magnitude equals to 5).
             */

            byte accValue = _a.GetValue();
            byte val = _memory.Fetch(_operandAddress);

            int temp = accValue + val + (_flags.GetFlag(StatusFlag.Carry) ? 1 : 0);

            byte result = (byte)temp;

            // If result is greater than 255, enable the Carry flag
            _flags.SetFlag(StatusFlag.Carry, temp > 255);

            // If the bit no. 7 is set, then enable the Negative flag
            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            // If result equals 0, enable the Zero flag
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            // If two numbers of the same sign produce a number whose sign is different, then there's an overflow
            _flags.SetFlag(StatusFlag.Overflow, ((accValue ^ result) & (val ^ result) & 0x0080) == 0x0080);

            _a.SetValue(result);
        }

        /// <summary>
        /// Substracts a value from the accumulator's value.
        /// </summary>

        private void SBC()
        {
            /*
                The substraction M - N can be represented as: M + (-N) = M + (256 - N) = M + (2 complement of N)
                Because there's not a borrow flag, we use the carry flag as our borrow flag by using its complement: B = 1 - C
                The substraction of N from M is expressed as:
                    M - N - B = M + (-N) - (1 - C) = M + (2 complement of N) - 1 + C
                    M + (256 - N) - 1 + C = M + (255 - N) + C = M + (1 complement of N) + C
             */

            /* Same story as ADC: overflow happens when the result can't fit into a signed byte: RESULT < -128 OR RESULT > 127
             * However, in substraction, the overflow would happen when both operands initially have different signs. The sign of substracting number
             * will change when taking its one complement (from (-) to (+), and (+) to (-)). For example, M = (+), N = (-), so when making the
             * substraction, we end with M + (-N) = M + (+N); it gets converted to (+) because the one complement of N. Another example would be, M = (-),
             * N = (+), so the substraction is -M + (+N) = -M + (-N); it gets converted to (-) because the one complement of N.
             */

            byte accValue = _a.GetValue();
            byte val = _memory.Fetch(_operandAddress);
            byte complement = (byte)(~val);

            int temp = accValue + complement + (_flags.GetFlag(StatusFlag.Carry) ? 1 : 0);
            byte result = (byte)temp;

            // If carry flag is set, it means a borror did not happen, otherwise it did happen
            _flags.SetFlag(StatusFlag.Carry, temp > 255);

            _flags.SetFlag(StatusFlag.Zero, result == 0);

            _flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            // In the substraction, the sign check is done in the complement
            _flags.SetFlag(StatusFlag.Overflow, ((accValue ^ result) & (complement ^ result) & 0x0080) == 0x0080);

            _a.SetValue(result);
        }

        /// <summary>                                                                                    
        /// Loads a value located in an address into the accumulator.
        /// </summary>
        private void LDA()
        {
            _a.SetValue(_memory.Fetch(_operandAddress));

            _flags.SetFlag(StatusFlag.Zero, _a.GetValue() == 0);
            _flags.SetFlag(StatusFlag.Negative, _a.GetValue().IsNegative());
        }

        /// <summary>
        /// Stores the accumulator value into memory.
        /// </summary>
        private void STA()
        {
            byte accValue = _a.GetValue();

            _memory.Store(_operandAddress, accValue);
        }

        /// <summary>
        /// Turn on the Carry Flag by setting 1.
        /// </summary>
        private void SEC()
        {
            _flags.SetFlag(StatusFlag.Carry, true);
        }

        /// <summary>
        /// Clears the Carry Flag by setting 0.
        /// </summary>
        private void CLC()
        {
            _flags.SetFlag(StatusFlag.Carry, false);
        }
    }


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
            string highByteHex = highByte.ToString("x");
            string lowByteHex = lowByte.ToString("x");

            return Convert.ToUInt16(highByteHex + lowByteHex, 16);
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
    }
}
