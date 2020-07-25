namespace MiNES.PPU.Registers
{
    public class PpuLoopy
    {
        public int CoarseX;
        public int CoarseY;
        public int Nametable;
        public int FineY;
        public uint Address;

        public int Loopy
        {
            get => (FineY << 12)
                | (Nametable << 10)
                | (CoarseY << 5)
                | CoarseX;
            set
            {
                CoarseX = value & 0x1F;
                CoarseY = (value & 0x03E0) >> 5;
                Nametable = (value & 0x0C00) >> 10;
                FineY = (value & 0x7000) >> 12;
                Address = (uint)(value & 0x3FFF);
            }
        }
    }
}
