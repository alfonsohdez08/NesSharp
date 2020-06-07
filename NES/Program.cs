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

            iNESParser.ParseNesRom(nesFile, out Memory cpuMemoryMapped, out Memory ppuMemoryMapped);
            
            var cpuBus = new CpuBus(cpuMemoryMapped);
            var cpu = new Cpu(cpuBus, 0xC000);

            cpu.Run();

            //while (cpu.StepInstruction())
            //{
            //    Console.WriteLine($"{cpu.ProgramCounterHexString}: {cpu.InstructionDissasembled}");
            //}

            Console.ReadLine();
        }
    }
}
