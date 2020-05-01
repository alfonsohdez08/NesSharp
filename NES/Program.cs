using System;
using System.Collections.Generic;

namespace NES
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] hexDump = new string[] { "a9", "01" ,"8d", "00", "02", "a9" ,"05" ,"8d" ,"01" ,"02" ,"a9" ,"08", "8d" ,"02" ,"02" };
            var memory = new Memory();
            ushort address = 0x0200; //initial address

            for (int i = 0; i < hexDump.Length; i++)
            {
                byte b = Convert.ToByte(hexDump[i], 16);

                memory.Store(address, b);
                address++;
            }

            var cpu = new Cpu(memory);

            cpu.Start();

            Console.ReadLine();
        }
    }
}
