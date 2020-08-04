namespace NesSharp.PPU.Registers
{
    public class PpuLoopy
    {
        public int CoarseX { get; set; }
        public int CoarseY { get; set; }
        public int Nametable { get; set; }
        public int FineY { get; set; }
        public ushort Address { get; set; }
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
                Address = (ushort)(value & 0x3FFF);
            }
        }

        /// <summary>
        /// Increments the horizontal position.
        /// </summary>
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
