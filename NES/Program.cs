using NES.Rom;
using System;
using System.IO;

namespace NES
{
    class Program
    {
        private static readonly string NesRootPath = Environment.GetEnvironmentVariable("NES", EnvironmentVariableTarget.Machine);

        static void Main(string[] args)
        {
            string nesTestFilePath = Path.Combine(NesRootPath, "nestest.nes");
            byte[] nesFile = File.ReadAllBytes(nesTestFilePath);

            iNESParser.ParseNesRom(nesFile, out Memory cpuMemoryMapped, out Memory ppuMemoryMapped);
            
            var cpuBus = new CpuBus(cpuMemoryMapped);
            var cpu = new Cpu(cpuBus, 0xC000);

            //cpu.Run();

            const string myCpuLogFile = "my_cpu_test_log.txt";
            using (FileStream cpuTestLog = File.Create(Path.Combine(NesRootPath, myCpuLogFile)))
            using (StreamWriter streamWriter = new StreamWriter(cpuTestLog))
            {
                while (cpu.StepInstruction())
                {
                    streamWriter.WriteLine(cpu.TestLineResult);
                }
            }

            //const string justCpuLogFile = "nes_cpu_test_expected_log_just_cpu.txt";
            //const string cpuExpectedLogFile = "nes_cpu_test_expected_log.txt";

            //using (FileStream cpuLogFile = File.Create(Path.Combine(NesRootPath, justCpuLogFile)))
            //using (StreamWriter streamWriter = new StreamWriter(cpuLogFile))
            //{
            //    foreach (string line in File.ReadLines(Path.Combine(NesRootPath, cpuExpectedLogFile)))
            //    {
            //        streamWriter.WriteLine(line.Substring(0, 73));
            //    }
            //}
        }
    }
}
