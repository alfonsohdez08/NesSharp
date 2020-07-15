using MiNES.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    class Mask: Register<byte>
    {
        public override byte RegisterValue
        {
            get => base.RegisterValue;
            set
            {
                RenderBackground = value.GetBit(3);
                RenderSprites = value.GetBit(4);

                base.RegisterValue = value;
            }
        }

        public bool RenderBackground { get; private set; }
        public bool RenderSprites { get; private set; }
    }
}
