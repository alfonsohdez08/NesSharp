using MiNES.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    /// <summary>
    /// As its name says, controls the PPU operation (8-bit register).
    /// </summary>
    class Control: Register<byte>
    {
        public byte BaseNametableAddress => (byte)((RegisterValue.GetBit(0) ? 1 : 0) | ((RegisterValue.GetBit(1) ? 1 : 0) << 1));

        public bool VRamAddressIncrement { get; private set; }
        public int SpritesPatternTableBaseAddress { get; private set; }
        public int BackgroundPatternTableBaseAddress { get; private set; }
        public bool GenerateNmi { get; private set; }
        public bool SpriteSize { get; private set; }

        public override byte RegisterValue
        { 
            get => base.RegisterValue;
            set
            {
                VRamAddressIncrement = value.GetBit(2);
                SpritesPatternTableBaseAddress = value.GetBit(3) ? 0x1000 : 0;
                BackgroundPatternTableBaseAddress = value.GetBit(4) ? 0x1000 : 0;
                SpriteSize = value.GetBit(5);
                GenerateNmi = value.GetBit(7);

                base.RegisterValue = value;
            }
        }

        public Control()
        {

        }
    }
}
