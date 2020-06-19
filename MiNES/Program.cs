using MiNES._6502;
using MiNES.Rom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MiNES
{
    class Program
    {
        private static readonly string NesRootPath = Environment.GetEnvironmentVariable("NES", EnvironmentVariableTarget.Machine);

        static void Main(string[] args)
        {
#if CPU_NES_TEST
TestCPU();
#else
            var superMarioRom = File.ReadAllBytes(Path.Combine(NesRootPath, "super_mario_bros.nes"));
            var nes = new NES(superMarioRom);

            var frame = nes.Frame();
#endif
        }
#if CPU_NES_TEST
        private static void TestCPU()
        {
            string nesTestFilePath = Path.Combine(NesRootPath, "nestest.nes");
            byte[] nesFile = File.ReadAllBytes(nesTestFilePath);

            iNESParser.ParseNesCartridge(nesFile, out Memory cpuMemoryMapped, out Memory ppuMemoryMapped);

            var cpuBus = new CpuBus(cpuMemoryMapped);
            var cpu = new Cpu(cpuBus);

            ////cpu.Run();

            const string myCpuLogFile = "my_cpu_test_log.txt";
            using (FileStream myCpuTestLog = File.Create(Path.Combine(NesRootPath, myCpuLogFile)))
            using (StreamWriter streamWriter = new StreamWriter(myCpuTestLog))
            {
                while (!cpu.CpuTestDone)
                {
                    cpu.Step();
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

            //var cpuLog = GetCpuTestLog();

            //const string newCpuExpectedLog = "new_cpu_expected_log.txt";
            //using (StreamWriter streamWriter = File.CreateText(Path.Combine(NesRootPath, newCpuExpectedLog)))
            //{
            //    foreach (string logLine in cpuLog)
            //    {
            //        streamWriter.WriteLine(logLine);
            //    }
            //}


            //var nes = new NES(File.ReadAllBytes(Path.Combine(NesRootPath, "super_mario_bros.nes")));
        }

                /// <summary>
        /// Parses the NES test CPU log that does not have the instruction dissasembled details.
        /// </summary>
        /// <returns>The NES test CPU log without the innstruction dissasembled details.</returns>
        private static List<string> GetCpuTestLog()
        {
            const string cpuExpectedLogFile = "nes_cpu_test_expected_log.txt";

            //var log = new StringBuilder();
            var log = new List<string>();
            foreach (string line in File.ReadLines(Path.Combine(NesRootPath, cpuExpectedLogFile)))
            {
                string pcAddress = line.Substring(0, 4);

                string hexDump = line.Substring(6, 8).TrimEnd();
                //byte[] dump = rawDump.Split(' ').Select(b => Convert.ToByte(b, 16)).ToArray();
                
                //string instruction = string.Join(' ', line.Substring(16, 32).TrimEnd().Split(' ').Take(2));
                //string instructionDisssasembled = Assembler.DissasembleInstruction(dump, Convert.ToUInt16(pcAddress, 16));

                //if (instruction != instructionDisssasembled)
                //    throw new InvalidOperationException($"Both dissasemble are differents. From CPU log: {instruction} From my own dissasembler: {instructionDisssasembled}.");

                //string registers = line.Substring(48);
                string registers = line.Substring(48, 25);
                string accumulativeCycles = line.Substring(86);

                //log.AppendLine($"{pcAddress} {rawDump.PadRight(10, ' ')}{instruction.PadRight(32, ' ')}{registers}");
                //log.Add($"{pcAddress} {rawDump.PadRight(10, ' ')}{instruction.PadRight(32, ' ')}{registers}");
                log.Add($"{pcAddress} {hexDump.PadRight(10, ' ')}{registers} {accumulativeCycles}");
            }

            return log;

            //return log.ToString();
        }
#endif
    }
}
