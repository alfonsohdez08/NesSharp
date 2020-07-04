using MiNES.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    class Mask: Register<byte>
    {
        public bool RenderBackground => Value.GetBit(3);
        public bool RenderSprites => Value.GetBit(4);
    }
}
