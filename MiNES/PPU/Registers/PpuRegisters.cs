using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    // Deferred execution registers

    class PpuControl
    {
        public int BaseNametableAddress;
        public bool VRamAddressIncrement;
        public int SpritePatternTableAddress;
        public int BackgroundPatternTableAddress;
        public bool SpriteSize;
        public bool TriggerNmi;

        public int Control
        {
            get => ((TriggerNmi ? 1 : 0) << 7)
                    | ((SpriteSize ? 1 : 0) << 5)
                    | ((BackgroundPatternTableAddress & 0x1000) >> 8)
                    | ((SpritePatternTableAddress & 0x1000) >> 9)
                    | ((VRamAddressIncrement ? 1 : 0) << 2)
                    | ((BaseNametableAddress & 0x0C00) >> 10);
            set
            {
                BaseNametableAddress = 0x2000 | ((value & 3) << 10);
                VRamAddressIncrement = (value & 4) == 4;
                SpritePatternTableAddress = (value & 8) << 9;
                BackgroundPatternTableAddress = (value & 0x10) << 8;
                SpriteSize = (value & 0x20) == 0x20;
                TriggerNmi = (value & 0x80) == 0x80;
            }
        }
    }

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

    class PpuStatus
    {
        public bool SpriteOverflow;
        public bool SpriteZeroHit;
        public bool VerticalBlank;

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

    public class Loopy
    {
        public int CoarseX;
        public int CoarseY;
        public int Nametable;
        public int FineY;
        public uint Address;

        public int LoopyRegister
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
