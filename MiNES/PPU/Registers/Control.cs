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
        public byte BaseNametableAddress => (byte)((Value.GetBit(0) ? 1 : 0) | ((Value.GetBit(1) ? 1 : 0) << 1));

        public bool VRamAddressIncrement => Value.GetBit(2);

        public bool BackgroundPatternTableAddress => Value.GetBit(4);

        public bool GenerateNMI => Value.GetBit(7);

        public bool SpriteSize => Value.GetBit(5);

        public Control()
        {

        }
    }
}
