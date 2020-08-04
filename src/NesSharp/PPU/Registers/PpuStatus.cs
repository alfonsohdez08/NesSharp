namespace NesSharp.PPU.Registers
{
    class PpuStatus
    {
        public bool SpriteOverflow { get; set; }
        public bool SpriteZeroHit { get; set; }
        public bool VerticalBlank { get; set; }
        public int Status
        {
            get => ((VerticalBlank ? 1 : 0) << 7)
                | ((SpriteZeroHit ? 1 : 0) << 6)
                | ((SpriteOverflow ? 1 : 0) << 5);
            set
            {
                SpriteOverflow = (value & 0x20) == 0x20;
                SpriteZeroHit = (value & 0x40) == 0x40;
                SpriteOverflow = (value & 0x80) == 0x80;
            }
        }
    }
}
