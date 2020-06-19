namespace MiNES
{
    /// <summary>
    /// PPU's bus.
    /// </summary>
    public class PpuBus : Bus
    {

        public PpuBus(Memory memory): base(memory)
        {
        }

        public override byte Read(ushort address)
        {
            // Nametables and attribute tables (mirrored in the range [0x3000, 0x3EFF])
            if (address >= 0x2000 && address < 0x3F00)
                return memory.Fetch((ushort)(0x2000 + address % 0x1000));
            // Background palette and sprite palletes (mirrored in the range [0x3F20, 0x3FFF])
            else if (address >= 0x3F00 && address < 0x4000)
                return memory.Fetch((ushort)(0x3F00 + address % 0x0020));
            // Mirror of everything allocated from 0x000 until 0x3FFF
            else if (address >= 0x4000)
                return this.Read((ushort)(address % 0x4000));

            return memory.Fetch(address);
        }

        public override void Write(ushort address, byte val)
        {
            // Nametables and attribute tables (mirrored in the range [0x3000, 0x3EFF])
            if (address >= 0x2000 && address < 0x3F00)
                memory.Store((ushort)(0x2000 + address % 0x1000), val);
            // Background palette and sprite palletes (mirrored in the range [0x3F20, 0x3FFF])
            else if (address >= 0x3F00 && address < 0x4000)
                memory.Store((ushort)(0x3F00 + address % 0x0020), val);
            // Mirror of everything allocated from 0x000 until 0x3FFF
            else if (address >= 0x4000)
                this.Write((ushort)(address % 0x4000), val);
            else
                memory.Store(address, val);
        }
    }
}
