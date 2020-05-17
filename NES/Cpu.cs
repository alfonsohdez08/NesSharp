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
        Indirect
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
        /// Instruction's operand (set based on the instruction's addressing mode).
        /// </summary>
        private byte _operand; 

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
                case 0xE9:
                    FetchOperand(AddressingMode.Immediate);
                    SBC();
                    break;
                case 0x38:
                    SEC();
                    break;
                case 0x18:
                    CLC();
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
            byte operand;
            
            IncrementPC();

            switch (mode)
            {
                case AddressingMode.ZeroPage:
                    operand = _memory.Fetch(_memory.Fetch(_pcAddress));
                    break;
                case AddressingMode.ZeroPageX:
                    operand = _memory.Fetch(ByteExtensions.Sum(_memory.Fetch(_pcAddress), _x.GetValue())); //What would happen if by adding the X content cross the zero page 
                    break;
                case AddressingMode.ZeroPageY:
                    operand = _memory.Fetch(ByteExtensions.Sum(_memory.Fetch(_pcAddress), _y.GetValue()));
                    break;
                case AddressingMode.Immediate:
                    operand = _memory.Fetch(_pcAddress);
                    break;
                case AddressingMode.Relative:
                    operand = _memory.Fetch(_pcAddress); // This would be an address offset that would be used for the branch instructions
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
                    if (mode == AddressingMode.Absolute || mode == AddressingMode.Indirect)
                        operand = _memory.Fetch(addressParsed); // For Indirect mode, this would be an address
                    else if (mode == AddressingMode.AbsoluteX)
                        operand = _memory.Fetch((ushort)(addressParsed + _x.GetValue()));
                    else
                        operand = _memory.Fetch((ushort)(addressParsed + _y.GetValue()));
                    break;
                default:
                    throw new NotImplementedException($"The given addressing mode is not supported: {mode.ToString()}.");
            }

            _operand = operand;
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
            byte val = _operand;

            int temp = accValue + val + (_flags.GetFlag(StatusFlag.Carry) ? 1 : 0);

            byte result = (byte)temp;

            // If result is greater than 255, enable the Carry flag
            _flags.SetFlag(StatusFlag.Carry, temp > 255);

            // If the bit no. 7 is set, then enable the Negative flag
            _flags.SetFlag(StatusFlag.Negative, (result & (1 << 7)) == 0x0080);

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
            byte val = _operand;
            byte complement = (byte)(~val);

            int temp = accValue + complement + (_flags.GetFlag(StatusFlag.Carry) ? 1 : 0);
            byte result = (byte)temp;

            // If carry flag is set, it means a borror did not happen, otherwise it did happen
            _flags.SetFlag(StatusFlag.Carry, temp > 255);

            _flags.SetFlag(StatusFlag.Zero, result == 0);

            _flags.SetFlag(StatusFlag.Negative, (temp & 1 << 7) == 0x0080);

            // In the substraction, the sign check is done in the complement
            _flags.SetFlag(StatusFlag.Overflow, ((accValue ^ result) & (complement ^ result) & 0x0080) == 0x0080);

            _a.SetValue(result);
        }

        /// <summary>                                                                                    
        /// Loads a given value into the accumulator (LDA); it uses immediate address (the literal value is passed as argument to the instruction).
        /// </summary>
        private void LDA()
        {
            _flags.SetFlag(StatusFlag.Zero, _operand == 0);
            _flags.SetFlag(StatusFlag.Negative, (_operand & 1 << 7) == 0x0080);

            _a.SetValue(_operand);
        }

        /// <summary>
        /// Stores the accumulator value (STA) in a given address (absolute address).
        /// </summary>
        private void STA()
        {
            IncrementPC();
            byte lowByte = _memory.Fetch(_pcAddress);

            IncrementPC();
            byte highByte = _memory.Fetch(_pcAddress);

            ushort absAddress = ByteExtensions.ParseBytes(lowByte, highByte);//Check if this is the way to merge two bytes (LOW_BYTE HIGH_BYTE)
            byte acValue = _a.GetValue();

            _memory.Store(absAddress, acValue);
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

            return (val & mask) == 128;
        }

        /// <summary>
        /// Checks if the given value is positive (the bit no. 7 is "off").
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>True if it's positive; otherwise false.</returns>
        public static bool IsPositive(this byte val) => !val.IsNegative();

        public static ushort Sum(byte x, byte y) => (ushort)(x + y);
    }
}
