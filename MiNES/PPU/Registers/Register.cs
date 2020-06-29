using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    internal abstract class Register
    {
        public byte Value { get; set; }
    }
}
