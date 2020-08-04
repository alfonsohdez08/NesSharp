namespace NesSharp.PPU.Registers
{
    class PpuMask
    {
        public bool GreyscaleMode { get; private set; }
        public bool RenderLeftSideBackground { get; private set; }
        public bool RenderLeftSideSprites { get; private set; }
        public bool RenderBackground { get; private set; }
        public bool RenderSprites { get; private set; }
        public int Mask
        {
            get => ((RenderSprites ? 1 : 0) << 4)
                | ((RenderBackground ? 1 : 0) << 3)
                | ((RenderLeftSideSprites ? 1 : 0) << 2)
                | ((RenderLeftSideBackground ? 1 : 0) << 1)
                | (GreyscaleMode ? 1 : 0);
            set
            {
                GreyscaleMode = (value & 1) == 1;
                RenderLeftSideBackground = (value & 2) == 2;
                RenderLeftSideSprites = (value & 4) == 4;
                RenderBackground = (value & 8) == 8;
                RenderSprites = (value & 0x10) == 0x10;
            }
        }
    }
}
