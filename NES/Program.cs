using System;
using System.Collections.Generic;

namespace NES
{
    class Program
    {
        static void Main(string[] args)
        {
            //string[] hexDump = new string[] { "a9", "01" ,"8d", "00", "02", "a9" ,"05" ,"8d" ,"01" ,"02" ,"a9" ,"08", "8d" ,"02" ,"02" };
            string[] hexDump = new string[] { "a9", "01", "e9","78" };
            var memory = new Memory();
            const ushort startingAddress = 0x0600;
            ushort address = startingAddress; //initial address

            for (int i = 0; i < hexDump.Length; i++)
            {
                byte b = Convert.ToByte(hexDump[i], 16);

                memory.Store(address, b);
                address++;
            }

            var cpu = new Cpu(memory, startingAddress);

            cpu.Start();

            Console.ReadLine();
        }
    }
}
