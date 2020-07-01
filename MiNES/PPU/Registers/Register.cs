using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    internal abstract class Register<T>
    {
        public T Value { get; set; }
    }
}
