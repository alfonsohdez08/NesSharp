namespace MiNES.PPU.Registers
{
    class PpuMask
    {
        public bool GreyscaleMode;
        public bool RenderLeftSideBackground;
        public bool RenderLeftSideSprites;
        public bool RenderBackground;
        public bool RenderSprites;

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
