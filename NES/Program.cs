using NES._6502;
using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Program
    {
        static void Main(string[] args)
        {
            //string[] hexDump = new string[] { "a9", "01" ,"8d", "00", "02", "a9" ,"05" ,"8d" ,"01" ,"02" ,"a9" ,"08", "8d" ,"02" ,"02" };
            //string program = "a9 88 38 e9 09";
            //string program = "a9 88 38 e9 f7";
            //string[] hexDump = program.Split(' ');
            ////string[] hexDump = new string[] { "a9", "01", "e9","78" };
            //var memory = new Memory();
            //const ushort startingAddress = 0x0001;
            //ushort address = startingAddress; //initial address

            //for (int i = 0; i < hexDump.Length; i++)
            //{
            //    byte b = Convert.ToByte(hexDump[i], 16);

            //    memory.Store(address, b);
            //    address++;
            //}

            //var cpu = new Cpu(memory, startingAddress);

            //cpu.Start();

            var sb = new StringBuilder();
            sb.AppendLine("lda #$01");
            sb.AppendLine("sta $10A");

            ProgramAssembled programAssembled = Assembler.Assemble(sb.ToString());

            Console.ReadLine();
        }
    }
}
