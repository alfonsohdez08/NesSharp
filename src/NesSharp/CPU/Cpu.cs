using System;
using System.Text;
using static NesSharp.Extensions.BitwiseExtensions;

namespace NesSharp.CPU
{

    /// <summary>
    /// The 6502 CPU.
    /// </summary>
    partial class Cpu: Clockeable
    {
        /// <summary>
        /// The NTSC CPU master clock factor.
        /// </summary>
        private const int NtscMasterClockFactor = 15;

        /// <summary>
        /// The CPU bus (interacts with other components within the NES).
        /// </summary>
        private readonly CpuBus _bus;

        /// <summary>
        /// The Status register.
        /// </summary>
        private readonly Flags _flags = new Flags();

        /// <summary>
        /// The Accumulator register.
        /// </summary>
        private byte _a;

        /// <summary>
        /// The X register.
        /// </summary>
        private byte _x;

        /// <summary>
        /// The Y register.
        /// </summary>
        private byte _y;

        /// <summary>
        /// The pointer address of the next available position in the stack.
        /// </summary>
        private byte _stackPointer;

        /// <summary>
        /// The register that holds the address of either the current's instruction operand or next instruction.
        /// </summary>
        private ushort _programCounter;

        /// <summary>
        /// The address where the instruction operand should be looked up.
        /// </summary>
        private ushort _operandAddress;

        /// <summary>
        /// The number of CPU cycles spent for execute the instruction looked up.
        /// </summary>
        private int _instructionCycles;

        /// <summary>
        /// A flag for denote whether the instruction requires an additional cycle e.g. crosses a page bounday.
        /// </summary>
        private bool _additionalCycle = false;

        /// <summary>
        /// Counter of how many cycles has been elapsed along the instructions executed.
        /// </summary>
        public uint CyclesElapsed { get; private set; }

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
        /// Executes a NMI interruption.
        /// </summary>
        public void NMI()
        {
            const int nmiCycles = 7;

            Interrupt(InterruptionType.NMI);
            AddCycles(nmiCycles);
        }

        /// <summary>
        /// Add an amount of cycles to the CPU cycles counter for acknowledge/log an operation that has happened.
        /// </summary>
        /// <param name="cpuCycles">The number of CPU cycles.</param>
        public void AddCycles(int cpuCycles)
        {
            CyclesElapsed += (uint)cpuCycles;
            MasterClockCycles += (cpuCycles * NtscMasterClockFactor);
        }

        /// <summary>
        /// Executes the next instruction denoted by the program counter.
        /// </summary>
        private int StepInstruction()
        {
            ExecuteInstruction();
            AddCycles(_instructionCycles);

            return _instructionCycles;
        }

        public override void RunUpTo(int masterClockCycles)
        {
            while (MasterClockCycles <= masterClockCycles)
                StepInstruction();
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

        /// <summary>
        /// Executes a CPU instruction (droven by the fetch decode execute cycle).
        /// </summary>
        private void ExecuteInstruction()
        {
            // Fetch the op code from the memory
            byte opCode = _bus.Read(_programCounter);

#if CPU_NES_TEST
            ushort instructionAddress = _programCounter;

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

            _instructionCycles = instruction.Cycles;
            _additionalCycle = instruction.AdditionalCycleWhenCrossPage;

            SetOperand(instruction.AddressingMode);

#if CPU_NES_TEST
            string instructionDisassembled = ParseInstruction(instruction);
            string instructionHexDump = string.Join(" ", _instructionHex.Select(i => i.ToString("X").PadLeft(2, '0')));

            //TestLineResult = $"{opCodeAddress.ToString("X")}  {instructionHexDump.PadRight(10, ' ')}{instructionDisassembled.PadRight(32, ' ')}{registersSnapshot}";
            TestLineResult = $"{instructionAddress.ToString("X").PadLeft(4, '0')} {instructionHexDump.PadRight(10, ' ')}{registersSnapshot}";
            _instructionHex.Clear();
#endif

            // Execute the instruction based on its mnemonic code
            switch (instruction.Mnemonic)
            {
                case Mnemonic.ADC:
                    ADC();
                    break;
                case Mnemonic.AND:
                    AND();
                    break;
                case Mnemonic.ASL:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ASL_ACC();
                    else
                        ASL();
                    break;
                case Mnemonic.BCC:
                    BCC();
                    break;
                case Mnemonic.BCS:
                    BCS();
                    break;
                case Mnemonic.BEQ:
                    BEQ();
                    break;
                case Mnemonic.BIT:
                    BIT();
                    break;
                case Mnemonic.BMI:
                    BMI();
                    break;
                case Mnemonic.BNE:
                    BNE();
                    break;
                case Mnemonic.BPL:
                    BPL();
                    break;
                case Mnemonic.BRK:
                    BRK();
                    break;
                case Mnemonic.BVC:
                    BVC();
                    break;
                case Mnemonic.BVS:
                    BVS();
                    break;
                case Mnemonic.CLC:
                    CLC();
                    break;
                case Mnemonic.CLD:
                    CLD();
                    break;
                case Mnemonic.CLI:
                    CLI();
                    break;
                case Mnemonic.CLV:
                    CLV();
                    break;
                case Mnemonic.CMP:
                    CMP();
                    break;
                case Mnemonic.CPX:
                    CPX();
                    break;
                case Mnemonic.CPY:
                    CPY();
                    break;
                case Mnemonic.DEC:
                    DEC();
                    break;
                case Mnemonic.DEX:
                    DEX();
                    break;
                case Mnemonic.DEY:
                    DEY();
                    break;
                case Mnemonic.EOR:
                    EOR();
                    break;
                case Mnemonic.INC:
                    INC();
                    break;
                case Mnemonic.INX:
                    INX();
                    break;
                case Mnemonic.INY:
                    INY();
                    break;
                case Mnemonic.JMP:
                    JMP();
                    break;
                case Mnemonic.JSR:
                    JSR();
                    break;
                case Mnemonic.LDA:
                    LDA();
                    break;
                case Mnemonic.LDX:
                    LDX();
                    break;
                case Mnemonic.LDY:
                    LDY();
                    break;
                case Mnemonic.LSR:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        LSR_ACC();
                    else
                        LSR();
                    break;
                case Mnemonic.NOP:
                    NOP();
                    break;
                case Mnemonic.ORA:
                    ORA();
                    break;
                case Mnemonic.PHA:
                    PHA();
                    break;
                case Mnemonic.PHP:
                    PHP();
                    break;
                case Mnemonic.PLA:
                    PLA();
                    break;
                case Mnemonic.PLP:
                    PLP();
                    break;
                case Mnemonic.ROL:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ROL_ACC();
                    else
                        ROL();
                    break;
                case Mnemonic.ROR:
                    if (instruction.AddressingMode == AddressingMode.Accumulator)
                        ROR_ACC();
                    else
                        ROR();
                    break;
                case Mnemonic.RTI:
                    RTI();
                    break;
                case Mnemonic.RTS:
                    RTS();
                    break;
                case Mnemonic.SBC:
                    SBC();
                    break;
                case Mnemonic.SEC:
                    SEC();
                    break;
                case Mnemonic.SED:
                    SED();
                    break;
                case Mnemonic.SEI:
                    SEI();
                    break;
                case Mnemonic.STA:
                    STA();
                    break;
                case Mnemonic.STX:
                    STX();
                    break;
                case Mnemonic.STY:
                    STY();
                    break;
                case Mnemonic.TAX:
                    TAX();
                    break;
                case Mnemonic.TAY:
                    TAY();
                    break;
                case Mnemonic.TSX:
                    TSX();
                    break;
                case Mnemonic.TXA:
                    TXA();
                    break;
                case Mnemonic.TXS:
                    TXS();
                    break;
                case Mnemonic.TYA:
                    TYA();
                    break;
                case Mnemonic.LAX:
                    LAX();
                    break;
                case Mnemonic.SAX:
                    SAX();
                    break;
                case Mnemonic.DCP:
                    DCP();
                    break;
                case Mnemonic.ISB:
                    ISB();
                    break;
                case Mnemonic.SLO:
                    SLO();
                    break;
                case Mnemonic.RLA:
                    RLA();
                    break;
                case Mnemonic.SRE:
                    SRE();
                    break;
                case Mnemonic.RRA:
                    RRA();
                    break;
            }

#if CPU_NES_TEST
            TestLineResult += $" CYC:{_cyclesElapsed}";
            _cyclesElapsed += _cycles;
#endif
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
            _a = _y;

            UpdateZeroNegativeFlags(_a);
        }

        /// <summary>
        /// Transfers the content of the register X to Stack Pointer register.
        /// </summary>
        private void TXS() => _stackPointer = _x;

        /// <summary>
        /// Transfers the content of the register X to the Accumulator.
        /// </summary>
        private void TXA()
        {
            _a = _x;

            UpdateZeroNegativeFlags(_a);
        }

        /// <summary>
        /// Transfers the content of the Stack Pointer register to the X register.
        /// </summary>
        private void TSX()
        {
            _x = _stackPointer;

            UpdateZeroNegativeFlags(_x);
        }

        /// <summary>
        /// Transfers the content of Acummulator to the register Y.
        /// </summary>
        private void TAY()
        {
            _y = _a;

            UpdateZeroNegativeFlags(_y);
        }

        /// <summary>
        /// Transfers the content of Acummulator to the register X.
        /// </summary>
        private void TAX()
        {
            _x = _a;

            UpdateZeroNegativeFlags(_x);
        }

        /// <summary>
        /// Stores the content of the register Y to a memory slot.
        /// </summary>
        private void STY() => _bus.Write(_operandAddress, _y);


        /// <summary>
        /// Stores the content of the register Y to a memory slot.
        /// </summary>
        private void STX() => _bus.Write(_operandAddress, _x);

        /// <summary>
        /// Sets the disable interrupt flag.
        /// </summary>
        private void SEI() => _flags.SetFlag(StatusFlag.DisableInterrupt, true);

        /// <summary>
        /// Sets the decimal flag.
        /// </summary>
        private void SED() => _flags.SetFlag(StatusFlag.Decimal, true);

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
        private void PLP() => _flags.SetFlags(Pop());

        /// <summary>
        /// Pops/pulls a byte from the stack and store into the Accumulator.
        /// </summary>
        private void PLA()
        {
            _a = Pop();

            UpdateZeroNegativeFlags(_a);
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
        private void PHA() => Push(_a);

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
            _y = _bus.Read(_operandAddress);

            UpdateZeroNegativeFlags(_y);
        }

        /// <summary>
        /// Loads a value into the register X.
        /// </summary>
        private void LDX()
        {
            _x = _bus.Read(_operandAddress);

            UpdateZeroNegativeFlags(_x);
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
        private void JMP() => _programCounter = _operandAddress;

        /// <summary>
        /// Increments the register Y by one.
        /// </summary>
        private void INY() => UpdateZeroNegativeFlags(++_y);

        /// <summary>
        /// Increments the register X by one.
        /// </summary>
        private void INX() => UpdateZeroNegativeFlags(++_x);

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
        private void DEY() => UpdateZeroNegativeFlags(--_y);


        /// <summary>
        /// Decrements the register X by one.
        /// </summary>
        private void DEX() => UpdateZeroNegativeFlags(--_x);


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
        private void CPY() => Compare(_y, _bus.Read(_operandAddress));

        /// <summary>
        /// Compares the content of the register X against a value held in memory.
        /// </summary>
        private void CPX() => Compare(_x, _bus.Read(_operandAddress));

        /// <summary>
        /// Compares the content of the accumulator against a value held in memory.
        /// </summary>
        private void CMP() => Compare(_a, _bus.Read(_operandAddress));

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
        private void CLV() => _flags.SetFlag(StatusFlag.Overflow, false);

        /// <summary>
        /// Clears the interrupt flag (by setting to false).
        /// </summary>
        private void CLI() => _flags.SetFlag(StatusFlag.DisableInterrupt, false);

        /// <summary>
        /// Clears the decimal flag.
        /// </summary>
        private void CLD() => _flags.SetFlag(StatusFlag.Decimal, false);

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if overflow flag is set.
        /// </summary>
        private void BVS() => GotoBranchIf(_flags.GetFlag(StatusFlag.Overflow));

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if overflow flag is not set.
        /// </summary>
        private void BVC() => GotoBranchIf(!_flags.GetFlag(StatusFlag.Overflow));

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if negative flag is not set.
        /// </summary>
        private void BPL() => GotoBranchIf(!_flags.GetFlag(StatusFlag.Negative));

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if zero flag is not set.
        /// </summary>
        private void BNE() => GotoBranchIf(!_flags.GetFlag(StatusFlag.Zero));

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if negative flag is set.
        /// </summary>
        private void BMI() => GotoBranchIf(_flags.GetFlag(StatusFlag.Negative));

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if zero flag is set.
        /// </summary>
        private void BEQ() => GotoBranchIf(_flags.GetFlag(StatusFlag.Zero));

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if carry flag is set.
        /// </summary>
        private void BCS() => GotoBranchIf(_flags.GetFlag(StatusFlag.Carry));

        /// <summary>
        /// Adds the address (a signed number) in scope to the program counter if carry flag is not set.
        /// </summary>
        private void BCC() => GotoBranchIf(!_flags.GetFlag(StatusFlag.Carry));

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
        private void STA() => _bus.Write(_operandAddress, _a);

        /// <summary>
        /// Turn on the Carry Flag by setting 1.
        /// </summary>
        private void SEC() => _flags.SetFlag(StatusFlag.Carry, true);

        /// <summary>
        /// Clears the Carry Flag by setting 0.
        /// </summary>
        private void CLC() => _flags.SetFlag(StatusFlag.Carry, false);

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
        private void ASL_ACC() => _a = ShiftLeft(_a);

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
        private void LSR_ACC() => _a = ShiftRight(_a);

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
        private void ROL_ACC() => _a = RotateLeft(_a);

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
        private void ROR_ACC() => _a = RotateRight(_a);

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
            _a = (byte)(_bus.Read(_operandAddress) & _a);

            UpdateZeroNegativeFlags(_a);
        }

        /// <summary>
        /// Performs a logical Exclusive OR (NOR) operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void EOR()
        {
            _a = (byte)(_bus.Read(_operandAddress) ^ _a);

            UpdateZeroNegativeFlags(_a);
        }

        /// <summary>
        /// Performs a logical Inclusive OR operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void ORA()
        {
            _a = (byte)(_bus.Read(_operandAddress) | _a);

            UpdateZeroNegativeFlags(_a);
        }

        /// <summary>
        /// Forces CPU interruption.
        /// </summary>
        private void BRK() => Interrupt(InterruptionType.BRK);

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
        /// Go to a branch if condition is true.
        /// </summary>
        /// <param name="conditionResult">The result of the condition evaluated.</param>
        private void GotoBranchIf(bool conditionResult)
        {
            if (conditionResult)
                AddOffsetToPC();
        }

        /// <summary>
        /// Adds an offset to the current program counter.
        /// </summary>
        private void AddOffsetToPC()
        {
            // Add additional cycle when branch condition is true
            _instructionCycles++;

            ushort targetAddress = (ushort)(_programCounter + (sbyte)_bus.Read(_operandAddress));
            CheckIfCrossedPageBoundary(_programCounter, targetAddress); // Add another cycle if new branch is in another page

            _programCounter = targetAddress;
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
            ushort operandAddress = 0;
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
                    switch (mode)
                    {
                        case AddressingMode.Indirect:
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
                            break;
                        case AddressingMode.Absolute:
                            operandAddress = addressParsed;
                            break;
                        case AddressingMode.AbsoluteX:
                            operandAddress = (ushort)(addressParsed + _x);
                            CheckIfCrossedPageBoundary(addressParsed, operandAddress);
                            break;
                        case AddressingMode.AbsoluteY:
                            operandAddress = (ushort)(addressParsed + _y);
                            CheckIfCrossedPageBoundary(addressParsed, operandAddress);
                            break;
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
                _instructionCycles++;
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
    }
}
