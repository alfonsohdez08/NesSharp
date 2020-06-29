using MiNES.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    class Mask: Register
    {
        public bool RenderBackground => Value.GetBit(3);

    }
}
