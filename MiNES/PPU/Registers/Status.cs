using MiNES.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    class Status: Register<byte>
    {

        public bool VerticalBlank
        {
            get => Value.GetBit(7);
            set
            {
                // Workaround
                byte v = Value;

                v.SetBit(7, value);
                Value = v;
            }
        }

        public Status()
        {
            Value = 0xA0;
        }
    }
}
