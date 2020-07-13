﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static MiNES.Extensions.BitwiseExtensions;

namespace MiNES.CPU
{

    /// <summary>
    /// The 6502 CPU.
    /// </summary>
    public partial class Cpu
    {
        /// <summary>
        /// Accumulator.
        /// </summary>
        private byte _a;

        /// <summary>
        /// X register (general purpose).
        /// </summary>
        private byte _x;

        /// <summary>
        /// Y register (general purpose).
        /// </summary>
        private byte _y;

        /// <summary>
        /// Status register (each bit represents a flag).
        /// </summary>
        private readonly Flags _flags = new Flags();

        /// <summary>
        /// Holds the address of the outer 
        /// </summary>
        private byte _stackPointer;

        /// <summary>
        /// The Program Counter register (holds the memory address of the next instruction or instruction's operand).
        /// </summary>
        private ushort _programCounter;

        /// <summary>
        /// Instruction's operand memory address (the location in memory where resides the instruction's operand).
        /// <remarks>
        /// For instance, for immediate addressing mode, it stores the adddres where it should pickup the value.
        /// </remarks>
        /// </summary>
        private ushort _operandAddress;

        /// <summary>
        /// The number of cycles spent for execute an instruction (for execute the fetch decode execute cycle).
        /// </summary>
        private byte _cycles;

        /// <summary>
        /// A flag for denote whether the underlying instruction requires an additional cycle e.g. crosses a page bounday.
        /// </summary>
        private bool _additionalCycle = false;

        /// <summary>
        /// The CPU bus (interacts with other components within the NES).
        /// </summary>
        private readonly CpuBus _bus;

        /// <summary>
        /// Counter of how many cycles has been elapsed along instructions executed.
        /// </summary>
        public int CyclesElapsed { get; private set; }

#if CPU_NES_TEST
        private readonly List<byte> _instructionHex = new List<byte>();
        private int _cyclesElapsed = 7; // Initially 7 cycles has elapsed 

        public string TestLineResult { get; private set; }
        public bool CpuTestDone { get; private set; } = false;

        private string GetRegistersSnapshot()
        {
            return $"A:{FormatByte(_a)} X:{FormatByte(_x)} Y:{FormatByte(_y)} P:{FormatByte(_flags.GetFlags())} SP:{FormatByte(_stackPointer)}";
        }

        private string ParseInstruction(Instruction instruction)
        {
            var instructionSb = new StringBuilder(instruction.Mnemonic);

            if (!(instruction.AddressingMode == AddressingMode.Accumulator || instruction.AddressingMode == AddressingMode.Implied))
            {
                var operandParsed = new StringBuilder($"${FetchOperand()}");
                if (instruction.AddressingMode == AddressingMode.Immediate)
                    operandParsed.Insert(0, '#');
                else if (instruction.AddressingMode == AddressingMode.AbsoluteX || instruction.AddressingMode == AddressingMode.ZeroPageX)
                    operandParsed.Append(",X");
                else if (instruction.AddressingMode == AddressingMode.AbsoluteY || instruction.AddressingMode == AddressingMode.ZeroPageY)
                    operandParsed.Append(",Y");
                else if (instruction.AddressingMode == AddressingMode.Indirect)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append(')');
                }
                else if (instruction.AddressingMode == AddressingMode.IndirectX)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append(",X)");
                }
                else if (instruction.AddressingMode == AddressingMode.IndirectY)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append("),Y");
                }

                instructionSb.Append($" {operandParsed.ToString()}");
            }

            string FetchOperand()
            {
                ushort val = _operandAddress;
                if (instruction.AddressingMode == AddressingMode.Immediate)
                    val = _bus.Read(_operandAddress);
                else if (instruction.AddressingMode == AddressingMode.Relative)
                    val = (ushort)(_programCounter + (sbyte)(_bus.Read(_operandAddress))); // Perform the addition no matter what the condition result

                return ParseOperand(val, instruction.AddressingMode);
            }

            return instructionSb.ToString();
        }

        private static string ParseOperand(ushort operand, AddressingMode addressingMode)
        {
            string op = operand.ToString("X");

            // This representation is not accurate because it attempts to follow the format of the nes_cpu_test.log
            switch (addressingMode)
            {
                // Addressing modes whose represetantion must be 2 bytes
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.Indirect:
                case AddressingMode.Relative:
                    op = op.PadLeft(4, '0');
                    break;
                // Addressing modes whose represetantion must be 1 byte
                case AddressingMode.Immediate:
                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageX:
                case AddressingMode.ZeroPageY:
                case AddressingMode.IndirectX:
                case AddressingMode.IndirectY:
                    op = op.PadLeft(2, '0');
                    break;
            }

            return op;
        }

        private static string FormatByte(byte b) => $"{b.ToString("X").PadLeft(2, '0')}";
#endif

        /// <summary>
        /// Creates an instance of the 6502 CPU.
        /// </summary>
        /// <param name="bus">The CPU bus (used for interact with other components within the NES).</param>
        public Cpu(CpuBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException(nameof(bus));

            _bus = bus;
            Initialize();
        }

        /// <summary>
        /// Initializes the CPU based on the commercial NES.
        /// </summary>
        private void Initialize()
        {
#if CPU_NES_TEST
            _flags.SetFlags(0x0024); // ‭0010 0100‬
            _stackPointer = 0xFD;
            _programCounter = 0xC000;
#else
            _flags.SetFlags(0x0034); // 00‭11 0100‬
            Interrupt(InterruptionType.RESET);
#endif
        }

#if !CPU_NES_TEST
        private string ParseInstruction(Instruction instruction)
        {
            var instructionSb = new StringBuilder(instruction.Mnemonic);

            if (!(instruction.AddressingMode == AddressingMode.Accumulator || instruction.AddressingMode == AddressingMode.Implied))
            {
                var operandParsed = new StringBuilder($"${FetchOperand()}");
                if (instruction.AddressingMode == AddressingMode.Immediate)
                    operandParsed.Insert(0, '#');
                else if (instruction.AddressingMode == AddressingMode.AbsoluteX || instruction.AddressingMode == AddressingMode.ZeroPageX)
                    operandParsed.Append(",X");
                else if (instruction.AddressingMode == AddressingMode.AbsoluteY || instruction.AddressingMode == AddressingMode.ZeroPageY)
                    operandParsed.Append(",Y");
                else if (instruction.AddressingMode == AddressingMode.Indirect)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append(')');
                }
                else if (instruction.AddressingMode == AddressingMode.IndirectX)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append(",X)");
                }
                else if (instruction.AddressingMode == AddressingMode.IndirectY)
                {
                    operandParsed.Insert(0, '(');
                    operandParsed.Append("),Y");
                }

                instructionSb.Append($" {operandParsed.ToString()}");
            }

            string FetchOperand()
            {
                ushort val = _operandAddress;
                if (instruction.AddressingMode == AddressingMode.Immediate)
                    val = _bus.Read(_operandAddress);
                else if (instruction.AddressingMode == AddressingMode.Relative)
                    val = _bus.Read(_operandAddress);
                    //val = (ushort)(_pcAddress + (sbyte)(_bus.Read(_operandAddress))); // Perform the addition no matter what the condition result

                return ParseOperand(val, instruction.AddressingMode);
            }

            return instructionSb.ToString();
        }

        private static string ParseOperand(ushort operand, AddressingMode addressingMode)
        {
            string op = operand.ToString("X");

            // This representation is not accurate because it attempts to follow the format of the nes_cpu_test.log
            switch (addressingMode)
            {
                // Addressing modes whose represetantion must be 2 bytes
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.Indirect:
                    op = op.PadLeft(4, '0');
                    break;
                // Addressing modes whose represetantion must be 1 byte
                case AddressingMode.Immediate:
                case AddressingMode.ZeroPage:
                case AddressingMode.ZeroPageX:
                case AddressingMode.ZeroPageY:
                case AddressingMode.IndirectX:
                case AddressingMode.IndirectY:
                case AddressingMode.Relative:
                    op = op.PadLeft(2, '0');
                    break;
            }

            return op;
        }
#endif

        /// <summary>
        /// Executes a CPU instruction (droven by the fetch decode execute cycle).
        /// </summary>
        /// <returns>The number of cycle spent in order to execute the instruction.</returns>
        private byte ExecuteInstruction()
        {
            // Fetches the op code from the memory
            ushort instructionAddress = _programCounter;
            byte opCode = _bus.Read(_programCounter);

#if CPU_NES_TEST
            if (_programCounter == 1)
            {
                CpuTestDone = true;
                return 0;
            }

            _instructionHex.Add(opCode);
            string registersSnapshot = GetRegistersSnapshot();
#endif

            // Advances either to the current instruction operand or next instruction (depending on the addressing mode of the instruction)
            IncrementPC();

            Instruction instruction = OpCodes[opCode];
            if (instruction == null) // Illegal opcode (kills the CPU)
                throw new Exception("The CPU got killed by an illegal opcode.");

            _cycles = instruction.Cycles;
            _additionalCycle = instruction.AdditionalCycleWhenCrossPage;

            SetOperand(instruction.AddressingMode);

#if CPU_NES_TEST
            string instructionDisassembled = ParseInstruction(instruction);
            string instructionHexDump = string.Join(" ", _instructionHex.Select(i => i.ToString("X").PadLeft(2, '0')));

            //TestLineResult = $"{opCodeAddress.ToString("X")}  {instructionHexDump.PadRight(10, ' ')}{instructionDisassembled.PadRight(32, ' ')}{registersSnapshot}";
            TestLineResult = $"{instructionAddress.ToString("X").PadLeft(4, '0')} {instructionHexDump.PadRight(10, ' ')}{registersSnapshot}";
            _instructionHex.Clear();
#else
            //string instructionDissasembled = ParseInstruction(instruction);
            //Console.WriteLine($"{instructionAddress.ToString("X").PadLeft(4, '0')}: {instructionDissasembled}");
#endif

            // Executes the instruction based on its mnemonic code
            switch (instruction.Mnemonic)
            {
                case ADC_INSTRUCTION:
                    ADC();
                    break;
                case AND_INSTRUCTION:
                    AND();
                    break;
                case ASL_INSTRUCTION:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ASL_ACC();
                    else
                        ASL();
                    break;
                case BCC_INSTRUCTION:
                    BCC();
                    break;
                case BCS_INSTRUCTION:
                    BCS();
                    break;
                case BEQ_INSTRUCTION:
                    BEQ();
                    break;
                case BIT_INSTRUCTION:
                    BIT();
                    break;
                case BMI_INSTRUCTION:
                    BMI();
                    break;
                case BNE_INSTRUCTION:
                    BNE();
                    break;
                case BPL_INSTRUCTION:
                    BPL();
                    break;
                case BRK_INSTRUCTION:
                    BRK();
                    break;
                case BVC_INSTRUCTION:
                    BVC();
                    break;
                case BVS_INSTRUCTION:
                    BVS();
                    break;
                case CLC_INSTRUCTION:
                    CLC();
                    break;
                case CLD_INSTRUCTION:
                    CLD();
                    break;
                case CLI_INSTRUCTION:
                    CLI();
                    break;
                case CLV_INSTRUCTION:
                    CLV();
                    break;
                case CMP_INSTRUCTION:
                    CMP();
                    break;
                case CPX_INSTRUCTION:
                    CPX();
                    break;
                case CPY_INSTRUCTION:
                    CPY();
                    break;
                case DEC_INSTRUCTION:
                    DEC();
                    break;
                case DEX_INSTRUCTION:
                    DEX();
                    break;
                case DEY_INSTRUCTION:
                    DEY();
                    break;
                case EOR_INSTRUCTION:
                    EOR();
                    break;
                case INC_INSTRUCTION:
                    INC();
                    break;
                case INX_INSTRUCTION:
                    INX();
                    break;
                case INY_INSTRUCTION:
                    INY();
                    break;
                case JMP_INSTRUCTION:
                    JMP();
                    break;
                case JSR_INSTRUCTION:
                    JSR();
                    break;
                case LDA_INSTRUCTION:
                    LDA();
                    break;
                case LDX_INSTRUCTION:
                    LDX();
                    break;
                case LDY_INSTRUCTION:
                    LDY();
                    break;
                case LSR_INSTRUCTION:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        LSR_ACC();
                    else
                        LSR();
                    break;
                case NOP_INSTRUCTION:
                    NOP();
                    break;
                case ORA_INSTRUCTION:
                    ORA();
                    break;
                case PHA_INSTRUCTION:
                    PHA();
                    break;
                case PHP_INSTRUCTION:
                    PHP();
                    break;
                case PLA_INSTRUCTION:
                    PLA();
                    break;
                case PLP_INSTRUCTION:
                    PLP();
                    break;
                case ROL_INSTRUCTION:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ROL_ACC();
                    else
                        ROL();
                    break;
                case ROR_INSTRUCTION:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ROR_ACC();
                    else
                        ROR();
                    break;
                case RTI_INSTRUCTION:
                    RTI();
                    break;
                case RTS_INSTRUCTION:
                    RTS();
                    break;
                case SBC_INSTRUCTION:
                    SBC();
                    break;
                case SEC_INSTRUCTION:
                    SEC();
                    break;
                case SED_INSTRUCTION:
                    SED();
                    break;
                case SEI_INSTRUCTION:
                    SEI();
                    break;
                case STA_INSTRUCTION:
                    STA();
                    break;
                case STX_INSTRUCTION:
                    STX();
                    break;
                case STY_INSTRUCTION:
                    STY();
                    break;
                case TAX_INSTRUCTION:
                    TAX();
                    break;
                case TAY_INSTRUCTION:
                    TAY();
                    break;
                case TSX_INSTRUCTION:
                    TSX();
                    break;
                case TXA_INSTRUCTION:
                    TXA();
                    break;
                case TXS_INSTRUCTION:
                    TXS();
                    break;
                case TYA_INSTRUCTION:
                    TYA();
                    break;
                case LAX_INSTRUCTION:
                    LAX();
                    break;
                case SAX_INSTRUCTION:
                    SAX();
                    break;
                case DCP_INSTRUCTION:
                    DCP();
                    break;
                case ISB_INSTRUCTION:
                    ISB();
                    break;
                case SLO_INSTRUCTION:
                    SLO();
                    break;
                case RLA_INSTRUCTION:
                    RLA();
                    break;
                case SRE_INSTRUCTION:
                    SRE();
                    break;
                case RRA_INSTRUCTION:
                    RRA();
                    break;
                default:
                    throw new NotImplementedException($"The instruction {instruction.Mnemonic} has not been implemented yet; Op Code: {opCode.ToString("X").PadLeft(2, '0')} Addressing Mode: {instruction.AddressingMode}.");
            }

#if CPU_NES_TEST
            TestLineResult += $" CYC:{_cyclesElapsed}";
            _cyclesElapsed += _cycles;
#endif

            CyclesElapsed += _cycles;

            return _cycles;
        }

        /// <summary>
        /// Evaluates the conditions for set/unset the flags Zero and Negative.
        /// </summary>
        /// <param name="value">The value that would be used for the conditions.</param>
        private void UpdateZeroNegativeFlags(byte value)
        {
            _flags.SetFlag(StatusFlag.Zero, value == 0);
            _flags.SetFlag(StatusFlag.Negative, value.IsNegative());
        }

        /// <summary>
        /// Perform a ROR to a value from a memory location and then perform an ADC with the value from the Accumulator.
        /// </summary>
        private void RRA()
        {
            // Equivalent instructions:
            ROR();
            ADC();
        }

        /// <summary>
        /// Perform a LSR to a value from a memory location and then perform an EOR against the value from the Accumulator.
        /// </summary>
        private void SRE()
        {
            // Equivalent instructions:
            LSR();
            EOR();
        }

        /// <summary>
        /// Perform a ROL to a value from a memory location and then perform an AND against the value from the Accumulator.
        /// </summary>
        private void RLA()
        {
            // Equivalent instructions:
            ROL();
            AND();
        }

        /// <summary>
        /// Perform an ASL to a value from a memory location and then perform an OR against the value from the Accumulator.
        /// </summary>
        private void SLO()
        {
            // Equivalent instructions:
            ASL();
            ORA();
        }

        /// <summary>
        /// Perform an INC to a memory location and then perform a SBC with the accumulator.
        /// </summary>
        private void ISB()
        {
            /* Equivalent instructions:
                INC $FF
                SBC $FF             
             */

            INC();
            SBC();
        }

        /// <summary>
        /// Decrements by one the value specified by the memory address and the compare it against the Accumulator.
        /// </summary>
        private void DCP()
        {
            // Equivalent instructions
            DEC();
            CMP();
        }

        /// <summary>
        /// Performs an AND operation between the registers X and Accumulator and store its result into memory.
        /// </summary>
        private void SAX()
        {
            /* Equivalent instructions:
                STX $FE
                PHA
                AND $FE
                STA $FE
                PLA
             */

            // This instruction does not affect the CPU flags register
            byte flags = _flags.GetFlags();

            STX();
            PHA();
            AND();
            STA();
            PLA();

            // Pulling back the original value of the flags register
            _flags.SetFlags(flags);
        }

        /// <summary>
        /// Loads the content of a memory location into the Accumulator and the register X.
        /// </summary>
        private void LAX()
        {
            /*
                LDA $8400,Y
                LDX $8400,Y             
             */

            LDA();
            LDX();
        }

        /// <summary>
        /// Transfers the content of the register Y to the Accumulator.
        /// </summary>
        private void TYA()
        {
            byte val = _y;

            UpdateZeroNegativeFlags(val);

            _a = val;
        }

        /// <summary>
        /// Transfers the content of the register X to Stack Pointer register.
        /// </summary>
        private void TXS()
        {
            _stackPointer = _x;
        }

        /// <summary>
        /// Transfers the content of the register X to the Accumulator.
        /// </summary>
        private void TXA()
        {
            byte val = _x;

            UpdateZeroNegativeFlags(val);

            _a = _x;
        }

        /// <summary>
        /// Transfers the content of the Stack Pointer register to the X register.
        /// </summary>
        private void TSX()
        {
            byte val = _stackPointer;

            UpdateZeroNegativeFlags(val);

            _x = val;
        }

        /// <summary>
        /// Transfers the content of Acummulator to the register Y.
        /// </summary>
        private void TAY()
        {
            byte val = _a;

            UpdateZeroNegativeFlags(val);

            _y = val;
        }

        /// <summary>
        /// Transfers the content of Acummulator to the register X.
        /// </summary>
        private void TAX()
        {
            byte val = _a;

            UpdateZeroNegativeFlags(val);

            _x = val;
        }

        /// <summary>
        /// Stores the content of the register Y to a memory slot.
        /// </summary>
        private void STY()
        {
            _bus.Write(_operandAddress, _y);
        }

        /// <summary>
        /// Stores the content of the register Y to a memory slot.
        /// </summary>
        private void STX()
        {
            _bus.Write(_operandAddress, _x);
        }

        /// <summary>
        /// Sets the disable interrupt flag.
        /// </summary>
        private void SEI()
        {
            _flags.SetFlag(StatusFlag.DisableInterrupt, true);
        }

        /// <summary>
        /// Sets the decimal flag.
        /// </summary>
        private void SED()
        {
            _flags.SetFlag(StatusFlag.Decimal, true);
        }

        /// <summary>
        /// Pops/pulls the processor flags and the program counter from the stack (first pop is the processor flags, then the next 2 pops operations 
        /// are for fetch the low byte and high byte of the program counter).
        /// </summary>
        private void RTI()
        {
            byte flags = Pop();
            _flags.SetFlags(flags);

            byte pcLowByte = Pop();
            byte pcHighByte = Pop();
            _programCounter = ParseBytes(pcLowByte, pcHighByte);
        }

        /// <summary>
        /// Pops the low byte and high byte of the program counter (set when jumping into a routine).
        /// </summary>
        private void RTS()
        {
            byte pcLowByte = Pop();
            byte pcHighByte = Pop();

            ushort pcAddress = ParseBytes(pcLowByte, pcHighByte);
            _programCounter = (ushort)(pcAddress + 1);
        }

        /// <summary>
        /// Pops/pulls the processor status flags from the stack.
        /// </summary>
        private void PLP()
        {
            _flags.SetFlags(Pop());
        }

        /// <summary>
        /// Pops/pulls a byte from the stack and store into the Accumulator.
        /// </summary>
        private void PLA()
        {
            byte val = Pop();

            UpdateZeroNegativeFlags(val);

            _a = val;
        }

        /// <summary>
        /// Pushes a copy of the processor status flag into the stack.
        /// </summary>
        private void PHP()
        {
            /*
             * The copy of the CPU flags that would be pushed onto the stack will have set the bits 4 and 5
             * (this setting it's only to value that would be pushed onto the stack, not in the actual CPU flags).
             * Source: https://stackoverflow.com/questions/52017657/6502-emulator-testing-nestest 
             */
            byte flags = _flags.GetFlags();
            flags |= 0x30; // Sets the bit 4 and 5 to the copy of the CPU flags

            Push(flags);
        }

        /// <summary>
        /// Pushes a copy of the accumulator into the stack.
        /// </summary>
        private void PHA()
        {
            Push(_a);
        }

        /// <summary>
        /// Doesn't do anything.
        /// </summary>
        private void NOP()
        {
        }

        /// <summary>
        /// Loads a value into the register Y.
        /// </summary>
        private void LDY()
        {
            byte val = _bus.Read(_operandAddress);

            UpdateZeroNegativeFlags(val);

            _y = val;
        }

        /// <summary>
        /// Loads a value into the register X.
        /// </summary>
        private void LDX()
        {
            byte val = _bus.Read(_operandAddress);

            UpdateZeroNegativeFlags(val);

            _x = val;
        }

        /// <summary>
        /// Pushes the program counter address minus one into the stack and sets the program counter to the target memory address.
        /// </summary>
        private void JSR()
        {
            // The PC at this point points to the next instruction
            //ushort returnAddress = _programCounter.GetValue();
            ushort returnAddress = (ushort)(_programCounter - 1);

            // Pushes the high byte
            Push(returnAddress.GetHighByte());

            // Pushes the low byte
            Push(returnAddress.GetLowByte());

            _programCounter = _operandAddress;
        }

        /// <summary>
        /// Sets the program counter to the address specified in the JMP instruction.
        /// </summary>
        private void JMP()
        {
            _programCounter = _operandAddress;
        }

        /// <summary>
        /// Increments the register Y by one.
        /// </summary>
        private void INY()
        {
            _y++;

            UpdateZeroNegativeFlags(_y);
        }

        /// <summary>
        /// Increments the register X by one.
        /// </summary>
        private void INX()
        {
            _x++;

            UpdateZeroNegativeFlags(_x);
        }

        /// <summary>
        /// Increments by one the value allocated in the memory adddress specified.
        /// </summary>
        private void INC()
        {
            byte val = (byte)(_bus.Read(_operandAddress) + 1);

            UpdateZeroNegativeFlags(val);

            _bus.Write(_operandAddress, val);
        }

        /// <summary>
        /// Decrements the register Y by one.
        /// </summary>
        private void DEY()
        {
            _y--;

            UpdateZeroNegativeFlags(_y);
        }

        /// <summary>
        /// Decrements the register X by one.
        /// </summary>
        private void DEX()
        {
            _x--;

            UpdateZeroNegativeFlags(_x);
        }

        /// <summary>
        /// Decrements by one the value allocated in the memory address specified.
        /// </summary>
        private void DEC()
        {
            byte val = (byte)(_bus.Read(_operandAddress) - 1);

            UpdateZeroNegativeFlags(val);

            _bus.Write(_operandAddress, val);
        }

        /// <summary>
        /// Compares the content of the register Y against a value held in memory.
        /// </summary>
        private void CPY()
        {
            Compare(_y, _bus.Read(_operandAddress));
        }

        /// <summary>
        /// Compares the content of the register X against a value held in memory.
        /// </summary>
        private void CPX()
        {
            Compare(_x, _bus.Read(_operandAddress));
        }

        /// <summary>
        /// Compares the content of the accumulator against a value held in memory.
        /// </summary>
        private void CMP()
        {
            Compare(_a, _bus.Read(_operandAddress));
        }

        /// <summary>
        /// Compares two bytes and updates the CPU flags based on the result.
        /// </summary>
        /// <param name="register">A value coming from a register (Accumulator, X, Y).</param>
        /// <param name="valueFromMemory">A value held in memory.</param>
        private void Compare(byte register, byte valueFromMemory)
        {
            _flags.SetFlag(StatusFlag.Carry, register >= valueFromMemory);
            _flags.SetFlag(StatusFlag.Zero, register == valueFromMemory);
            _flags.SetFlag(StatusFlag.Negative, ((byte)(register - valueFromMemory)).IsNegative());
        }

        /// <summary>
        /// Clears the overflow flag (by setting to false).
        /// </summary>
        private void CLV()
        {
            _flags.SetFlag(StatusFlag.Overflow, false);
        }

        /// <summary>
        /// Clears the interrupt flag (by setting to false).
        /// </summary>
        private void CLI()
        {
            _flags.SetFlag(StatusFlag.DisableInterrupt, false);
        }

        /// <summary>
        /// Clears the decimal flag.
        /// </summary>
        private void CLD()
        {
            _flags.SetFlag(StatusFlag.Decimal, false);
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if overflow flag is set.
        /// </summary>
        private void BVS()
        {
            if (_flags.GetFlag(StatusFlag.Overflow))
                AddOffsetToPC();
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if overflow flag is not set.
        /// </summary>
        private void BVC()
        {
            if (!_flags.GetFlag(StatusFlag.Overflow))
                AddOffsetToPC();
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if negative flag is not set.
        /// </summary>
        private void BPL()
        {
            if (!_flags.GetFlag(StatusFlag.Negative))
                AddOffsetToPC();
        }

        /// <summary>
        /// Forces CPU interruption.
        /// </summary>
        private void BRK()
        {
            Interrupt(InterruptionType.BRK);
        }

        /// <summary>
        /// Interrupts the CPU.
        /// </summary>
        /// <param name="interruptionType">The kind of interruption to the CPU.</param>
        private void Interrupt(InterruptionType interruptionType)
        {
            if (interruptionType != InterruptionType.RESET)
            {
                byte lowByte = _programCounter.GetLowByte();
                byte highByte = _programCounter.GetHighByte();

                // Pushes the program counter
                Push(highByte);
                Push(lowByte);

                // Pushes the CPU flags
                byte flags = (byte)(_flags.GetFlags() | 0x30); // Sets bit 5 and 4
                if (interruptionType != InterruptionType.BRK)
                    flags = (byte)((flags | 0x10) ^ 0x10); // Disable the bit 4 to the copy of the CPU flags

                Push(flags);

                // Side effects after performing an interruption
                _flags.SetFlag(StatusFlag.DisableInterrupt, true);
            }

            byte jumpAddressLowByte, jumpAddressHighByte;
            switch (interruptionType)
            {
                case InterruptionType.NMI:
                    jumpAddressLowByte = _bus.Read(0xFFFA);
                    jumpAddressHighByte = _bus.Read(0xFFFB);
                    break;
                case InterruptionType.RESET:
                    jumpAddressLowByte = _bus.Read(0xFFFC);
                    jumpAddressHighByte = _bus.Read(0xFFFD);
                    break;
                case InterruptionType.IRQ:
                case InterruptionType.BRK:
                    jumpAddressLowByte = _bus.Read(0xFFFE);
                    jumpAddressHighByte = _bus.Read(0xFFFF);
                    break;
                default:
                    throw new InvalidOperationException($"The interruption {interruptionType} does not exist.");
            }

            ushort address = ParseBytes(jumpAddressLowByte, jumpAddressHighByte);
            _programCounter = address;
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if zero flag is not set.
        /// </summary>
        private void BNE()
        {
            if (!_flags.GetFlag(StatusFlag.Zero))
                AddOffsetToPC();
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if negative flag is set.
        /// </summary>
        private void BMI()
        {
            if (_flags.GetFlag(StatusFlag.Negative))
                AddOffsetToPC();
        }

        /// <summary>
        /// This instructions is used to test if one or more bits are set in a target memory location. The mask pattern in A is ANDed with the value 
        /// in memory to set or clear the zero flag, but the result is not kept. Bits 7 and 6 of the value from memory are copied into the N and V flags.
        /// (source: http://www.obelisk.me.uk/6502/reference.html#BIT)
        /// </summary>
        private void BIT()
        {
            byte memory = _bus.Read(_operandAddress);

            _flags.SetFlag(StatusFlag.Zero, (_a & memory) == 0);
            _flags.SetFlag(StatusFlag.Overflow, (memory & 0x0040) == 0x0040); // bit no. 6 from the memory value
            _flags.SetFlag(StatusFlag.Negative, memory.IsNegative());
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if zero flag is set.
        /// </summary>
        private void BEQ()
        {
            if (_flags.GetFlag(StatusFlag.Zero))
                AddOffsetToPC();
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if carry flag is set.
        /// </summary>
        private void BCS()
        {
            if (_flags.GetFlag(StatusFlag.Carry))
                AddOffsetToPC();
        }

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if carry flag is not set.
        /// </summary>
        private void BCC()
        {
            if (!_flags.GetFlag(StatusFlag.Carry))
                AddOffsetToPC();
        }

        private void AddOffsetToPC()
        {
            // Add additional cycle when branch condition is true
            _cycles++;

            ushort targetAddress = (ushort)(_programCounter + (sbyte)_bus.Read(_operandAddress));
            CheckIfCrossedPageBoundary(_programCounter, targetAddress); // Add another cycle if new branch is in another page

            _programCounter = targetAddress;
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

            byte accValue = _a;
            byte val = _bus.Read(_operandAddress);

            int temp = accValue + val + (_flags.GetFlag(StatusFlag.Carry) ? 1 : 0);

            byte result = (byte)temp;

            // If result is greater than 255, enable the Carry flag
            _flags.SetFlag(StatusFlag.Carry, temp > 255);

            //// If the bit no. 7 is set, then enable the Negative flag
            //_flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            //// If result equals 0, enable the Zero flag
            //_flags.SetFlag(StatusFlag.Zero, result == 0);

            UpdateZeroNegativeFlags(result);

            // If two numbers of the same sign produce a number whose sign is different, then there's an overflow
            _flags.SetFlag(StatusFlag.Overflow, ((accValue ^ result) & (val ^ result) & 0x0080) == 0x0080);

            _a = result;
        }

        /// <summary>
        /// Substracts a value from the accumulator's value.
        /// </summary>
        private void SBC()
        {
            /* The one complement of a number N is defined as N - 255. In binary, this is achieved by
             * flipping each bit of the binary representation of N. The two complement is just the one complement
             * plus one: 2 complement = 1 complement + 1 = (255 - N) + 1 = 256 - N. The two complement is used
             * for represent negative numbers.
             * 
             * The substraction M - N can be represented as: M + (-N) = M + (256 - N) = M + (2 complement of N)
             * due to there's not a borrow flag, we use the carry flag as our borrow flag by using its complement: B = 1 - C
             * The substraction of N from M is expressed as:
             *  M - N - B = M + (-N) - (1 - C) = M + (2 complement of N) - 1 + C
             *  M + (256 - N) - 1 + C = M + (255 - N) + C = M + (1 complement of N) + C
             */

            /* Same story as ADC: overflow happens when the result can't fit into a signed byte: RESULT < -128 OR RESULT > 127
             * However, in substraction, the overflow would happen when both operands initially have different signs. The sign of substracting number
             * will change when taking its one complement (from (-) to (+), and (+) to (-)). For example, M = (+), N = (-), so when making the
             * substraction, we end with M + (-N) = M + (+N); it gets converted to (+) because the one complement of N. Another example would be, M = (-),
             * N = (+), so the substraction is -M + (+N) = -M + (-N); it gets converted to (-) because the one complement of N.
             */

            byte accValue = _a;
            byte val = _bus.Read(_operandAddress);
            byte complement = (byte)(~val);

            int temp = accValue + complement + (_flags.GetFlag(StatusFlag.Carry) ? 1 : 0);
            byte result = (byte)temp;

            // If carry flag is set, it means a borror did not happen, otherwise it did happen
            _flags.SetFlag(StatusFlag.Carry, temp > 255);

            UpdateZeroNegativeFlags(result);

            //_flags.SetFlag(StatusFlag.Zero, result == 0);

            //_flags.SetFlag(StatusFlag.Negative, result.IsNegative());

            // In the substraction, the sign check is done in the complement
            _flags.SetFlag(StatusFlag.Overflow, ((accValue ^ result) & (complement ^ result) & 0x0080) == 0x0080);

            _a = result;
        }

        /// <summary>                                                                                    
        /// Loads a value located in an address into the accumulator.
        /// </summary>
        private void LDA()
        {
            _a = _bus.Read(_operandAddress);

            UpdateZeroNegativeFlags(_a);
        }

        /// <summary>
        /// Stores the accumulator value into memory.
        /// </summary>
        private void STA()
        {
            _bus.Write(_operandAddress, _a);
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

        /// <summary>
        /// Shifts each bit of a memory content one place to the left.
        /// </summary>
        private void ASL()
        {
            byte val = _bus.Read(_operandAddress);
            
            byte result = ShiftLeft(val);

            _bus.Write(_operandAddress, result);
        }

        /// <summary>
        /// Shifts each bit of current accumulator value one place to the left.
        /// </summary>
        private void ASL_ACC()
        {            
            byte result = ShiftLeft(_a);
            _a = result;
        }

        /// <summary>
        /// Performs a shift operation (left direction) in the given value (updates the CPU flags).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The value after performing the left shift.</returns>
        private byte ShiftLeft(byte val)
        {
            byte result = (byte)(val << 1);

            _flags.SetFlag(StatusFlag.Carry, (val & 0x0080) == 0x0080);
            UpdateZeroNegativeFlags(result);

            return result;
        }

        /// <summary>
        /// Shifts each bit of the memory content one place to the right.
        /// </summary>
        private void LSR()
        {
            byte val = _bus.Read(_operandAddress);
            
            byte result = ShiftRight(val);

            _bus.Write(_operandAddress, result);
        }

        /// <summary>
        /// Shifts each bit of current accumulator value one place to the right.
        /// </summary>
        private void LSR_ACC()
        {
            byte result = ShiftRight(_a);
            _a = result;
        }

        /// <summary>
        /// Performs a shift operation (right direction) in the given value (updates the CPU flags).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The value after performing the right shift.</returns>
        private byte ShiftRight(byte val)
        {
            byte result = (byte)(val >> 1);

            _flags.SetFlag(StatusFlag.Carry, (val & 0x01) == 0x01);
            UpdateZeroNegativeFlags(result);

            return result;
        }

        /// <summary>
        /// Moves each of the bits of an memory content one place to the left.
        /// </summary>
        private void ROL()
        {
            byte val = _bus.Read(_operandAddress);
            
            byte result = RotateLeft(val);

            _bus.Write(_operandAddress, result);
        }

        /// <summary>
        /// Moves each of the bits of the accumulator value one place to the left.
        /// </summary>
        private void ROL_ACC()
        {
            byte result = RotateLeft(_a);
            _a = result;
        }

        /// <summary>
        /// "Rotates" (shift) to the left side the given value (updates the CPU flags).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The value after performing the left side rotation (shift).</returns>
        private byte RotateLeft(byte val)
        {
            int r = val << 1;
            r |= (_flags.GetFlag(StatusFlag.Carry) ? 0x01 : 0); // places the carry flag into the bit 0
            byte result = (byte)r;

            _flags.SetFlag(StatusFlag.Carry, (val & 0x0080) == 0x0080);
            UpdateZeroNegativeFlags(result);

            return result;
        }

        /// <summary>
        /// Moves each of the bits of an memory content one place to the right.
        /// </summary>
        private void ROR()
        {
            byte val = _bus.Read(_operandAddress);
            
            byte result = RotateRight(val);

            _bus.Write(_operandAddress, result);
        }

        /// <summary>
        /// Moves each of the bits of the accumulator value one place to the right.
        /// </summary>
        private void ROR_ACC()
        {
            byte result = RotateRight(_a);
            _a = result;
        }

        /// <summary>
        /// "Rotates" (shift) to the right side the given value (updates the CPU flags).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The value after performing the right side rotation (shift).</returns>
        private byte RotateRight(byte val)
        {
            int r = val >> 1;
            r |= (_flags.GetFlag(StatusFlag.Carry) ? 0x0080 : 0); // places the carry flag into bit no.7
            byte result = (byte)r;

            _flags.SetFlag(StatusFlag.Carry, (val & 0x01) == 0x01);
            UpdateZeroNegativeFlags(result);

            return result;
        }

        /// <summary>
        /// Performs a logical AND operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void AND()
        {
            byte val = _bus.Read(_operandAddress);
            byte result = (byte)(val & _a);

            UpdateZeroNegativeFlags(result);

            _a = result;
        }

        /// <summary>
        /// Performs a logical Exclusive OR (NOR) operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void EOR()
        {
            byte val = _bus.Read(_operandAddress);
            byte result = (byte)(val ^ _a);

            UpdateZeroNegativeFlags(result);

            _a = result;
        }

        /// <summary>
        /// Performs a logical Inclusive OR operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void ORA()
        {
            byte val = _bus.Read(_operandAddress);
            byte result = (byte)(val | _a);

            UpdateZeroNegativeFlags(result);

            _a = result;
        }

#region Addressing modes

        /// <summary>
        /// Sets the operand address for the instruction.
        /// </summary>
        /// <param name="mode">The instruction's addressing mode.</param>
        private void SetOperand(AddressingMode mode)
        {
            if (mode == AddressingMode.Accumulator || mode == AddressingMode.Implied)
                return;

#if CPU_NES_TEST
            _instructionHex.Add(_bus.Read(_programCounter));
#endif
            ushort operandAddress;
            switch (mode)
            {
                case AddressingMode.ZeroPage:
                    operandAddress = _bus.Read(_programCounter);
                    break;
                case AddressingMode.ZeroPageX:
                    operandAddress = (byte)(_bus.Read(_programCounter) + _x);
                    break;
                case AddressingMode.ZeroPageY:
                    operandAddress = (byte)(_bus.Read(_programCounter) + _y);
                    break;
                case AddressingMode.Immediate:
                case AddressingMode.Relative:
                    operandAddress = _programCounter;
                    break;
                case AddressingMode.Absolute:
                case AddressingMode.AbsoluteX:
                case AddressingMode.AbsoluteY:
                case AddressingMode.Indirect:
                    ushort addressParsed;
                    {
                        byte lowByte = _bus.Read(_programCounter);

                        IncrementPC();

                        byte highByte = _bus.Read(_programCounter);
#if CPU_NES_TEST
                        _instructionHex.Add(highByte);
#endif

                        addressParsed = ParseBytes(lowByte, highByte);
                    }
                    if (mode == AddressingMode.Indirect)
                    {
                        // The content located in the address parsed is the LSB (Least Significant Byte) of the target address
                        byte lowByte = _bus.Read(addressParsed);

                        /*
                         * There's a bug in the hardware when parsing the effective address in the Indirect
                         * addressing mode: if the LSB (least significant byte) of the absolute address is 0xFF, then incrementing
                         * by one the absolute address (incrementing by one is required for get the MSB of the effective address) 
                         * would produce a wrap around the page; example below:
                         * 
                         * Absolute address: 0x02FF.
                         * LSB from the effetive address is at 0x02FF.
                         * MSB from the effective address should be at 0x02FF + 0x0001 = 0x0300; but because the bug explained above, it's
                         * at 0x0200 (we stayed in the same page 0x02)
                         */
                        if (addressParsed.GetLowByte() == 0xFF)
                            addressParsed ^= (0x00FF);
                        else
                            addressParsed++;

                        byte highByte = _bus.Read(addressParsed);

                        operandAddress = ParseBytes(lowByte, highByte);
                    }
                    else if (mode == AddressingMode.Absolute)
                    {
                        operandAddress = addressParsed;
                    }
                    else if (mode == AddressingMode.AbsoluteX)
                    {
                        operandAddress = (ushort)(addressParsed + _x);
                        CheckIfCrossedPageBoundary(addressParsed, operandAddress);
                    }
                    else
                    {
                        operandAddress = (ushort)(addressParsed + _y);
                        CheckIfCrossedPageBoundary(addressParsed, operandAddress);
                    }
                    break;
                case AddressingMode.IndirectX:
                    {
                        byte address = (byte)(_bus.Read(_programCounter) + _x); // Wraps around the zero page

                        byte lowByte = _bus.Read(address++);
                        byte highByte = _bus.Read(address);

                        operandAddress = ParseBytes(lowByte, highByte);
                    }
                    break;
                case AddressingMode.IndirectY:
                    {
                        byte zeroPageAddress = _bus.Read(_programCounter);

                        byte lowByte = _bus.Read(zeroPageAddress++);
                        byte highByte = _bus.Read(zeroPageAddress);

                        ushort address = ParseBytes(lowByte, highByte);
                        operandAddress = (ushort)(address + _y);

                        CheckIfCrossedPageBoundary(address, operandAddress);
                    }
                    break;
                default:
                    throw new NotImplementedException($"The given addressing mode is not supported: {mode}.");
            }

            _operandAddress = operandAddress;
            IncrementPC(); // Points to the next opcode (instruction)
        }

#endregion

        /// <summary>
        /// Checks if both address are in the same page or not. If they do not, the cycle counter will be incremented by one.
        /// </summary>
        /// <param name="initialAddress">The address before the addition.</param>
        /// <param name="addressAdded">The result of the addition between the initial address and another address.</param>
        private void CheckIfCrossedPageBoundary(ushort initialAddress, ushort addressAdded)
        {
            if (_additionalCycle && !AreAddressOnSamePage(initialAddress, addressAdded))
                _cycles++;
        }

        /// <summary>
        /// Checks if the two given addresses are on the same page (an address page is the high byte).
        /// </summary>
        /// <param name="address1">First address.</param>
        /// <param name="address2">Second address.</param>
        /// <returns>True if both are on the same page; otherwise false.</returns>
        private bool AreAddressOnSamePage(ushort address1, ushort address2) => address1 >> 8 == address2 >> 8;

        /// <summary>
        /// Increments the address allocated in the Program Counter.
        /// </summary>
        private void IncrementPC()
        {
            _programCounter++;
        }

        /// <summary>
        /// Parses the address for a stack operation (the stack uses the second page (page 1) from the CPU memory).
        /// </summary>
        /// <param name="stackPointerAddress">The address where the stack points.</param>
        /// <returns>An address somewhere in the page 1.</returns>
        private static ushort ParseStackAddress(byte stackPointerAddress)
        {
            return (ushort)(0x0100 | stackPointerAddress);
        }

        /// <summary>
        /// Pushes a value onto the stack (it updates the stack pointer after pushing the value).
        /// </summary>
        /// <param name="b">The value that would be pushed into the stack.</param>
        private void Push(byte b)
        {
            byte stackPointer = _stackPointer;

            /* When pushing a byte onto the stack, the stack pointer is decremented by one (the stack pointer groes down). Also the stack pointer
             * points to the next available slot in the stack's memory page.
             */
            ushort address = ParseStackAddress(stackPointer--);

            _bus.Write(address, b);

            _stackPointer = stackPointer;
        }

        /// <summary>
        /// Pops a value from the stack (it updates the stack pointer after popping the value).
        /// </summary>
        /// <returns>The top value in the stack.</returns>
        private byte Pop()
        {
            byte stackPointer = _stackPointer;

            // When popping a byte from the stack, the stack pointer is incremented by one
            ushort address = ParseStackAddress(++stackPointer);

            byte val = _bus.Read(address);

            _stackPointer = stackPointer;

            return val;
        }

        /// <summary>
        /// Executes a NMI interruption.
        /// </summary>
        public void NMI()
        {
            Interrupt(InterruptionType.NMI);
        }

        /// <summary>
        /// Executes the next instruction denoted by the program counter.
        /// </summary>
        /// <returns>The number of cycles spent for execute the instruction.</returns>
        public int Step()
        {
            if (_bus.DmaTransferTriggered)
                return TransferOam();
            else
                return ExecuteInstruction();
        }

        /// <summary>
        /// Transfers the OAM dataset into the PPU OAM.
        /// </summary>
        /// <returns>The number of cycles spent while transfering the OAM.</returns>
        private int TransferOam()
        {
            int cyclesSpent = CyclesElapsed % 2 != 0 ? 514 : 513;
            try
            {
                ushort cpuAddress = (ushort)(_bus.OamMemoryPage << 8);
                for (int i = 0; i < 256; i++)
                {
                    byte oamByte = _bus.Read(cpuAddress++);
                    _bus.Write(0x2004, oamByte);
                }
            }
            finally
            {
                _bus.DmaTransferTriggered = false;
                CyclesElapsed += cyclesSpent;
            }

            return cyclesSpent;
        }
    }
}
