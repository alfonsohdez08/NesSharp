using System;
using System.Collections.Generic;
using System.Text;

namespace NesSharp.PPU.Registers
{
    class PpuControl
    {
        public int BaseNametableAddress { get; private set; }
        public int VRamAddressIncrement { get; private set; }
        public int SpritePatternTableAddress { get; private set; }
        public int BackgroundPatternTableAddress { get; private set; }
        public bool SpriteSize { get; private set; }
        public bool TriggerNmi { get; private set; }
        public int Control
        {
            get => ((TriggerNmi ? 1 : 0) << 7)
                    | ((SpriteSize ? 1 : 0) << 5)
                    | ((BackgroundPatternTableAddress & 0x1000) >> 8)
                    | ((SpritePatternTableAddress & 0x1000) >> 9)
                    | ((VRamAddressIncrement == 32 ? 1 : 0) << 2)
                    | ((BaseNametableAddress & 0x0C00) >> 10);
            set
            {
                BaseNametableAddress = 0x2000 | ((value & 3) << 10);
                VRamAddressIncrement = (value & 4) == 4 ? 32 : 1;
                SpritePatternTableAddress = (value & 8) << 9;
                BackgroundPatternTableAddress = (value & 0x10) << 8;
                SpriteSize = (value & 0x20) == 0x20;
                TriggerNmi = (value & 0x80) == 0x80;
            }
        }
    }
}
