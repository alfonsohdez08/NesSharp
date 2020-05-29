using NES.Rom;
using System;
using System.IO;

namespace NES
{
    class Program
    {
        static void Main(string[] args)
        {
            //byte[] rom = File.Re(@"C:\Users\ward\source\repos\NES\NES\nestest.nes");
            byte[] nesFile = File.ReadAllBytes(@"C:\Users\ward\source\repos\NES\NES\nestest.nes");

            Memory memory = iNESParser.ParseNesFile(nesFile);
            var cpu = new Cpu(memory, 0xC000);

            cpu.Start();

            //var cpu = new Cpu(Memory.LoadRom(nesRom), 0xC000);
            //cpu.Start();

            //var sb = new StringBuilder();
            //sb.AppendLine("lda #$01");
            //sb.AppendLine("sta $10A");

            //ProgramAssembled programAssembled = Assembler.Assemble(sb.ToString());

            Console.ReadLine();
        }
    }
}
