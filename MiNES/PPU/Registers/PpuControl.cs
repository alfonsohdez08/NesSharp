using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
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
}
