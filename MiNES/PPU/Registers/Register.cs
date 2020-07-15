using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    internal abstract class Register<T>
    {
        protected T InternalValue;

        public virtual T RegisterValue { get => InternalValue; set => InternalValue = value; }
    }
}
