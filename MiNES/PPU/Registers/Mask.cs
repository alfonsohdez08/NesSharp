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
                ShowBackgroundLeftSideScreen = value.GetBit(1);
                ShowSpritesLeftSideScreen = value.GetBit(2);
                RenderBackground = value.GetBit(3);
                RenderSprites = value.GetBit(4);

                base.RegisterValue = value;
            }
        }

        public bool RenderBackground { get; private set; }
        public bool RenderSprites { get; private set; }
        public bool ShowBackgroundLeftSideScreen { get; private set; }
        public bool ShowSpritesLeftSideScreen { get; private set; }
    }
}
