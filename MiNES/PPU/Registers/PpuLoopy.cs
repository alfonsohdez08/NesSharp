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

        public void IncrementHorizontalPosition()
        {
            var coarseX = CoarseX;
            if (coarseX == 31)
            {
                coarseX = 0;
                Nametable ^= 1;
            }
            else
            {
                coarseX++;
            }

            CoarseX = coarseX;
        }

        /// <summary>
        /// Increments the vertical position in the V register (vertical positions are denoted by the bits 5-9 which denotes the coarse Y scroll, and the bits 12-14
        /// which denotes the pixels offset in the y axis within a tile: fine y).
        /// </summary>
        public void IncrementVerticalPosition()
        {
            var fineY = FineY;
            if (fineY < 7)
            {
                fineY++;
            }
            else
            {
                fineY = 0;

                // Increments coarse Y then
                var coarseY = CoarseY;
                if (coarseY == 29)
                {
                    coarseY = 0;

                    Nametable ^= 2;
                }
                else if (coarseY == 31)
                {
                    coarseY = 0;
                }
                else
                {
                    coarseY++;
                }

                CoarseY = coarseY;
            }

            FineY = fineY;
        }
    }
}
