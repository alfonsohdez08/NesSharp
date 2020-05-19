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
        IndirectX,
        IndirectY,
        Implied,
        Accumulator
    }

    /// <summary>
    /// The 6502 CPU.
    /// </summary>
    class Cpu
    {

        #region Mnemonics
        public const string ADC_INSTRUCTION = "ADC";
        public const string AND_INSTRUCTION = "AND";
        public const string ASL_INSTRUCTION = "ASL";
        public const string BCC_INSTRUCTION = "BCC";
        public const string BCS_INSTRUCTION = "BCS";
        public const string BEQ_INSTRUCTION = "BEQ";
        public const string BIT_INSTRUCTION = "BIT";
        public const string BMI_INSTRUCTION = "BMI";
        public const string BNE_INSTRUCTION = "BNE";
        public const string BPL_INSTRUCTION = "BPL";
        public const string BRK_INSTRUCTION = "BRK";
        public const string BVC_INSTRUCTION = "BVC";
        public const string BVS_INSTRUCTION = "BVS";
        public const string CLC_INSTRUCTION = "CLC";
        public const string CLD_INSTRUCTION = "CLD";
        public const string CLI_INSTRUCTION = "CLI";
        public const string CLV_INSTRUCTION = "CLV";
        public const string CMP_INSTRUCTION = "CMP";
        public const string CPX_INSTRUCTION = "CPX";
        public const string CPY_INSTRUCTION = "CPY";
        public const string DEC_INSTRUCTION = "DEC";
        public const string DEX_INSTRUCTION = "DEX";
        public const string DEY_INSTRUCTION = "DEY";
        public const string EOR_INSTRUCTION = "EOR";
        public const string INC_INSTRUCTION = "INC";
        public const string INX_INSTRUCTION = "INX";
        public const string INY_INSTRUCTION = "INY";
        public const string JMP_INSTRUCTION = "JMP";
        public const string JSR_INSTRUCTION = "JSR";
        public const string LDA_INSTRUCTION = "LDA";
        public const string LDX_INSTRUCTION = "LDX";
        public const string LDY_INSTRUCTION = "LDY";
        public const string LSR_INSTRUCTION = "LSR";
        public const string NOP_INSTRUCTION = "NOP";
        public const string ORA_INSTRUCTION = "ORA";
        public const string PHA_INSTRUCTION = "PHA";
        public const string PHP_INSTRUCTION = "PHP";
        public const string PLA_INSTRUCTION = "PLA";
        public const string PLP_INSTRUCTION = "PLP";
        public const string ROL_INSTRUCTION = "ROL";
        public const string ROR_INSTRUCTION = "ROR";
        public const string RTI_INSTRUCTION = "RTI";
        public const string RTS_INSTRUCTION = "RTS";
        public const string SBC_INSTRUCTION = "SBC";
        public const string SEC_INSTRUCTION = "SEC";
        public const string SED_INSTRUCTION = "SED";
        public const string SEI_INSTRUCTION = "SEI";
        public const string STA_INSTRUCTION = "STA";
        public const string STX_INSTRUCTION = "STX";
        public const string STY_INSTRUCTION = "STY";
        public const string TAX_INSTRUCTION = "TAX";
        public const string TAY_INSTRUCTION = "TAY";
        public const string TSX_INSTRUCTION = "TSX";
        public const string TXA_INSTRUCTION = "TXA";
        public const string TXS_INSTRUCTION = "TXS";
        public const string TYA_INSTRUCTION = "TYA";
        #endregion

        #region Instruction Set Operation Codes Matrix
        public static readonly Instruction[] OpCodes = new Instruction[256]
             {
            new Instruction(BRK_INSTRUCTION, AddressingMode.Implied, 7), new Instruction(ORA_INSTRUCTION, AddressingMode.IndirectX, 6), null, null, null, new Instruction(ORA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ASL_INSTRUCTION, AddressingMode.ZeroPage, 5), null, new Instruction(PHP_INSTRUCTION, AddressingMode.Implied, 3), new Instruction(ORA_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ASL_INSTRUCTION, AddressingMode.Accumulator, 2), null, null, new Instruction(ORA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ASL_INSTRUCTION, AddressingMode.Absolute, 6), null,
            new Instruction(BPL_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(ORA_INSTRUCTION, AddressingMode.IndirectY, 5), null, null, null, new Instruction(ORA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ASL_INSTRUCTION, AddressingMode.ZeroPageX, 6), null, new Instruction(CLC_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(ORA_INSTRUCTION, AddressingMode.AbsoluteY, 4), null, null, null, new Instruction(ORA_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(ASL_INSTRUCTION, AddressingMode.AbsoluteX, 7), null,
            new Instruction(JSR_INSTRUCTION, AddressingMode.Absolute, 6), new Instruction(AND_INSTRUCTION, AddressingMode.IndirectX, 6), null, null, new Instruction(BIT_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(AND_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ROL_INSTRUCTION, AddressingMode.ZeroPage, 5), null, new Instruction(PLP_INSTRUCTION, AddressingMode.Implied, 4), new Instruction(AND_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ROL_INSTRUCTION, AddressingMode.Accumulator, 2), null, new Instruction(BIT_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(AND_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ROL_INSTRUCTION, AddressingMode.Absolute, 6), null,
            new Instruction(BMI_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(AND_INSTRUCTION, AddressingMode.IndirectY, 5), null, null, null, new Instruction(AND_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ROL_INSTRUCTION, AddressingMode.ZeroPageX, 6), null, new Instruction(SEC_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(AND_INSTRUCTION, AddressingMode.AbsoluteY, 4), null, null, null, new Instruction(AND_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(ROL_INSTRUCTION, AddressingMode.AbsoluteX, 7), null,
            new Instruction(RTI_INSTRUCTION, AddressingMode.Implied, 6), new Instruction(EOR_INSTRUCTION, AddressingMode.IndirectX, 6), null, null, null, new Instruction(EOR_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LSR_INSTRUCTION, AddressingMode.ZeroPage, 5), null, new Instruction(PHA_INSTRUCTION, AddressingMode.Implied, 3), new Instruction(EOR_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LSR_INSTRUCTION, AddressingMode.Accumulator, 2), null, new Instruction(JMP_INSTRUCTION, AddressingMode.Absolute, 3), new Instruction(EOR_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LSR_INSTRUCTION, AddressingMode.Absolute, 6), null,
            new Instruction(BVC_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(EOR_INSTRUCTION, AddressingMode.IndirectY, 5), null, null, null, new Instruction(EOR_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LSR_INSTRUCTION, AddressingMode.ZeroPageX, 6), null, new Instruction(CLI_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(EOR_INSTRUCTION, AddressingMode.AbsoluteY, 4), null, null, null, new Instruction(EOR_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(LSR_INSTRUCTION, AddressingMode.AbsoluteX, 7), null,
            new Instruction(RTS_INSTRUCTION, AddressingMode.Implied, 6), new Instruction(ADC_INSTRUCTION, AddressingMode.IndirectX, 6), null, null, null, new Instruction(ADC_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(ROR_INSTRUCTION, AddressingMode.ZeroPage, 5), null, new Instruction(PLA_INSTRUCTION, AddressingMode.Implied, 4), new Instruction(ADC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(ROR_INSTRUCTION, AddressingMode.Accumulator, 2), null, new Instruction(JMP_INSTRUCTION, AddressingMode.Indirect, 5), new Instruction(ADC_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(ROR_INSTRUCTION, AddressingMode.Absolute, 6), null,
            new Instruction(BVS_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(ADC_INSTRUCTION, AddressingMode.IndirectY, 5), null, null, null, new Instruction(ADC_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(ROR_INSTRUCTION, AddressingMode.ZeroPageX, 6), null, new Instruction(SEI_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(ADC_INSTRUCTION, AddressingMode.AbsoluteY, 4), null, null, null, new Instruction(ADC_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(ROR_INSTRUCTION, AddressingMode.AbsoluteX, 7), null,
            null, new Instruction(STA_INSTRUCTION, AddressingMode.IndirectX, 6), null, null, new Instruction(STY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(STA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(STX_INSTRUCTION, AddressingMode.ZeroPage, 3), null, new Instruction(DEY_INSTRUCTION, AddressingMode.Implied, 2), null, new Instruction(TXA_INSTRUCTION, AddressingMode.Implied, 2), null, new Instruction(STY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(STA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(STX_INSTRUCTION, AddressingMode.Absolute, 4), null,
            new Instruction(BCC_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(STA_INSTRUCTION, AddressingMode.IndirectY, 6), null, null, new Instruction(STY_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(STA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(STX_INSTRUCTION, AddressingMode.ZeroPageY, 4), null, new Instruction(TYA_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(STA_INSTRUCTION, AddressingMode.AbsoluteY, 5), new Instruction(TXS_INSTRUCTION, AddressingMode.Implied, 2), null, null, new Instruction(STA_INSTRUCTION, AddressingMode.AbsoluteX, 5), null, null,
            new Instruction(LDY_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.IndirectX, 6), new Instruction(LDX_INSTRUCTION, AddressingMode.Immediate, 2), null, new Instruction(LDY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LDA_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(LDX_INSTRUCTION, AddressingMode.ZeroPage, 3), null, new Instruction(TAY_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(TAX_INSTRUCTION, AddressingMode.Implied, 2), null, new Instruction(LDY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LDA_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(LDX_INSTRUCTION, AddressingMode.Absolute, 4), null,
            new Instruction(BCS_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.IndirectY, 5), null, null, new Instruction(LDY_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LDA_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(LDX_INSTRUCTION, AddressingMode.ZeroPageY, 4), null, new Instruction(CLV_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(LDA_INSTRUCTION, AddressingMode.AbsoluteY, 4), new Instruction(TSX_INSTRUCTION, AddressingMode.Implied, 2), null, new Instruction(LDY_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(LDA_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(LDX_INSTRUCTION, AddressingMode.AbsoluteY, 4), null,
            new Instruction(CPY_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.IndirectX, 6), null, null, new Instruction(CPY_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(CMP_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(DEC_INSTRUCTION, AddressingMode.ZeroPage, 5), null, new Instruction(INY_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(DEX_INSTRUCTION, AddressingMode.Implied, 2), null, new Instruction(CPY_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(CMP_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(DEC_INSTRUCTION, AddressingMode.Absolute, 6), null,
            new Instruction(BNE_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.IndirectY, 5), null, null, null, new Instruction(CMP_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(DEC_INSTRUCTION, AddressingMode.ZeroPageX, 6), null, new Instruction(CLD_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(CMP_INSTRUCTION, AddressingMode.AbsoluteY, 4), null, null, null, new Instruction(CMP_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(DEC_INSTRUCTION, AddressingMode.AbsoluteX, 7), null,
            new Instruction(CPX_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.IndirectX, 6), null, null, new Instruction(CPX_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(SBC_INSTRUCTION, AddressingMode.ZeroPage, 3), new Instruction(INC_INSTRUCTION, AddressingMode.ZeroPage, 5), null, new Instruction(INX_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.Immediate, 2), new Instruction(NOP_INSTRUCTION, AddressingMode.Implied, 2), null, new Instruction(CPX_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(SBC_INSTRUCTION, AddressingMode.Absolute, 4), new Instruction(INC_INSTRUCTION, AddressingMode.Absolute, 6), null,
            new Instruction(BEQ_INSTRUCTION, AddressingMode.Relative, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.IndirectY, 5), null, null, null, new Instruction(SBC_INSTRUCTION, AddressingMode.ZeroPageX, 4), new Instruction(INC_INSTRUCTION, AddressingMode.ZeroPageX, 6), null, new Instruction(SED_INSTRUCTION, AddressingMode.Implied, 2), new Instruction(SBC_INSTRUCTION, AddressingMode.AbsoluteY, 4), null, null, null, new Instruction(SBC_INSTRUCTION, AddressingMode.AbsoluteX, 4), new Instruction(INC_INSTRUCTION, AddressingMode.AbsoluteX, 7), null
        };
        #endregion

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

        /// <summary>
        /// The Program Counter register (holds the memory address of the next instruction).
        /// </summary>
        private readonly Register<ushort> _programCounter;

        private ushort _pcAddress => _programCounter.GetValue();

        /// <summary>
        /// The CPU's memory.
        /// </summary>
        private readonly IMemory _memory;

        /// <summary>
        /// Instruction's operand memory address (the location in memory where resides the instruction's operand).
        /// </summary>
        private ushort _operandAddress;

        public byte Accumulator => _a.GetValue();
        public byte X => _x.GetValue();
        public byte Y => _y.GetValue();
        public byte Flags => _flags.GetValue();
        public byte StackPointer => _stackPointer.GetValue();
        public ushort ProgramCounter => _programCounter.GetValue();

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
            // Each time a machine cycle is elapsed, the program counter would be incremented
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
            Instruction instruction = OpCodes[opCode];
            if (instruction == null) // Illegal opcode
                return true;

            FetchOperand(instruction.AddressingMode);

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
                default:
                    throw new NotImplementedException();
            }

            return true;
        }

        private void TYA()
        {
            throw new NotImplementedException();
        }

        private void TXS()
        {
            throw new NotImplementedException();
        }

        private void TXA()
        {
            throw new NotImplementedException();
        }

        private void TSX()
        {
            throw new NotImplementedException();
        }

        private void TAY()
        {
            throw new NotImplementedException();
        }

        private void TAX()
        {
            throw new NotImplementedException();
        }

        private void STY()
        {
            throw new NotImplementedException();
        }

        private void STX()
        {
            throw new NotImplementedException();
        }

        private void SEI()
        {
            throw new NotImplementedException();
        }

        private void SED()
        {
            throw new NotImplementedException();
        }

        private void RTI()
        {
            throw new NotImplementedException();
        }

        private void RTS()
        {
            throw new NotImplementedException();
        }

        private void PLP()
        {
            throw new NotImplementedException();
        }

        private void PLA()
        {
            throw new NotImplementedException();
        }

        private void PHP()
        {
            throw new NotImplementedException();
        }

        private void PHA()
        {
            throw new NotImplementedException();
        }

        private void NOP()
        {
            throw new NotImplementedException();
        }

        private void LDY()
        {
            throw new NotImplementedException();
        }

        private void LDX()
        {
            throw new NotImplementedException();
        }

        private void JSR()
        {
            throw new NotImplementedException();
        }

        private void JMP()
        {
            throw new NotImplementedException();
        }

        private void INY()
        {
            throw new NotImplementedException();
        }

        private void INX()
        {
            throw new NotImplementedException();
        }

        private void INC()
        {
            throw new NotImplementedException();
        }

        private void DEY()
        {
            throw new NotImplementedException();
        }

        private void DEX()
        {
            throw new NotImplementedException();
        }

        private void DEC()
        {
            throw new NotImplementedException();
        }

        private void CPY()
        {
            throw new NotImplementedException();
        }

        private void CPX()
        {
            throw new NotImplementedException();
        }

        private void CMP()
        {
            throw new NotImplementedException();
        }

        private void CLV()
        {
            throw new NotImplementedException();
        }

        private void CLI()
        {
            throw new NotImplementedException();
        }

        private void CLD()
        {
            throw new NotImplementedException();
        }

        private void BVS()
        {
            throw new NotImplementedException();
        }

        private void BVC()
        {
            throw new NotImplementedException();
        }

        private void BPL()
        {
            throw new NotImplementedException();
        }

        private void BRK()
        {
            throw new NotImplementedException();
        }

        private void BNE()
        {
            throw new NotImplementedException();
        }

        private void BMI()
        {
            throw new NotImplementedException();
        }

        private void BIT()
        {
            throw new NotImplementedException();
        }

        private void BEQ()
        {
            throw new NotImplementedException();
        }

        private void BCS()
        {
            throw new NotImplementedException();
        }

        private void BCC()
        {
            throw new NotImplementedException();
        }

        #region Addressing modes

        /// <summary>
        /// Fetchs the instruction operand based on the instruction addressing mode.
        /// </summary>
        /// <param name="mode">The instruction's addressing mode.</param>
        private void FetchOperand(AddressingMode mode)
        {
            if (mode == AddressingMode.Accumulator)
                return;

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
                case AddressingMode.IndirectX:
                    {
                        byte address = (byte)(_memory.Fetch(_pcAddress) + _x.GetValue()); // Wraps around the zero page

                        byte lowByte = _memory.Fetch(address++);
                        byte highByte = _memory.Fetch(address);

                        operandAddress = ByteExtensions.ParseBytes(lowByte, highByte);
                    }
                    break;
                case AddressingMode.IndirectY:
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

        /// <summary>
        /// Shifts each bit of a memory content one place to the left.
        /// </summary>
        private void ASL()
        {
            byte val = _memory.Fetch(_operandAddress);
            int result = val << 1;

            _flags.SetFlag(StatusFlag.Carry, (val & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            _memory.Store(_operandAddress, (byte)result);
        }

        /// <summary>
        /// Shifts each bit of current accumulator value one place to the left.
        /// </summary>
        private void ASL_ACC()
        {
            byte val = _a.GetValue();
            int result = val << 1;

            _flags.SetFlag(StatusFlag.Carry, (val & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            _a.SetValue((byte)result);
        }

        /// <summary>
        /// Shifts each bit of the memory content one place to the right.
        /// </summary>
        private void LSR()
        {
            byte val = _memory.Fetch(_operandAddress);
            int result = val >> 1;

            _flags.SetFlag(StatusFlag.Carry, (val & 0x01) == 0x01);
            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);

            _memory.Store(_operandAddress, (byte)result);
        }

        /// <summary>
        /// Shifts each bit of current accumulator value one place to the right.
        /// </summary>
        private void LSR_ACC()
        {
            byte val = _a.GetValue();
            int result = val >> 1;

            _flags.SetFlag(StatusFlag.Carry, (val & 0x01) == 0x01);
            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);

            _a.SetValue((byte)result);
        }

        /// <summary>
        /// Moves each of the bits of an memory content one place to the left.
        /// </summary>
        private void ROL()
        {
            byte val = _memory.Fetch(_operandAddress);
            int result = val << 1;
            result |= (_flags.GetFlag(StatusFlag.Carry) ? 0x01 : 0); // places the carry flag into the bit 0

            _flags.SetFlag(StatusFlag.Carry, (val & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            _memory.Store(_operandAddress, (byte)result);
        }

        /// <summary>
        /// Moves each of the bits of the accumulator value one place to the left.
        /// </summary>
        private void ROL_ACC()
        {
            byte val = _a.GetValue();
            int result = val << 1;
            result |= (_flags.GetFlag(StatusFlag.Carry) ? 0x01 : 0); // places the carry flag into the bit 0

            _flags.SetFlag(StatusFlag.Carry, (val & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            _a.SetValue((byte)result);
        }

        /// <summary>
        /// Moves each of the bits of an memory content one place to the right.
        /// </summary>
        private void ROR()
        {
            byte val = _memory.Fetch(_operandAddress);
            int result = val >> 1;
            result |= (_flags.GetFlag(StatusFlag.Carry) ? 0x0080 : 0); // places the carry flag into bit no.7

            _flags.SetFlag(StatusFlag.Carry, (val & 0x01) == 0x01);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);
            _flags.SetFlag(StatusFlag.Zero, result == 0);

            _memory.Store(_operandAddress, (byte)result);
        }

        /// <summary>
        /// Moves each of the bits of the accumulator value one place to the right.
        /// </summary>
        private void ROR_ACC()
        {
            byte val = _a.GetValue();
            int result = val >> 1;
            result |= (_flags.GetFlag(StatusFlag.Carry) ? 0x0080 : 0); // places the carry flag into bit no.7

            _flags.SetFlag(StatusFlag.Carry, (val & 0x01) == 0x01);
            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);

            _a.SetValue((byte)result);
        }

        /// <summary>
        /// Performs a logical AND operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void AND()
        {
            byte val = _memory.Fetch(_operandAddress);
            int result = val & _a.GetValue();

            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);

            _a.SetValue((byte)result);
        }

        /// <summary>
        /// Performs a logical Exclusive OR (NOR) operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void EOR()
        {
            byte val = _memory.Fetch(_operandAddress);
            int result = val ^ _a.GetValue();

            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);

            _a.SetValue((byte)result);
        }

        /// <summary>
        /// Performs a logical Inclusive OR operation between the accumulator value and a value fetched from memory.
        /// </summary>
        private void ORA()
        {
            byte val = _memory.Fetch(_operandAddress);
            int result = val | _a.GetValue();

            _flags.SetFlag(StatusFlag.Zero, result == 0);
            _flags.SetFlag(StatusFlag.Negative, (result & 0x0080) == 0x0080);

            _a.SetValue((byte)result);
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

    /// <summary>
    /// Represents a CPU instruction (including its mnemonic, addressing mode and machine cycles).
    /// </summary>
    class Instruction
    {
        /// <summary>
        /// The mnemonic code that represents the underlying instruction.
        /// </summary>
        public string Mnemonic { get; private set; }

        /// <summary>
        /// The addressing mode of the instruction.
        /// </summary>
        public AddressingMode AddressingMode { get; private set; }

        /// <summary>
        /// The amount of machine cycles required in order to execute the instruction.
        /// </summary>
        public byte MachineCycles { get; private set; }

        public Instruction(string mnemonic, AddressingMode addressingMode, byte machineCycles)
        {
            Mnemonic = mnemonic;
            AddressingMode = addressingMode;
            MachineCycles = machineCycles;
        }
    }
}
