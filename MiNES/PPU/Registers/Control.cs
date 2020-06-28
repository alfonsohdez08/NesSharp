using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    /// <summary>
    /// As its name says, controls the PPU operation (8-bit register).
    /// </summary>
    class Control : Register<byte>
    {
        public Control()
        {

        }


        public bool GetNmi() => (GetValue() & 0x80) == 0x80;

        public void SetNmi(bool val)
        {
            byte reg = GetValue();

            Ppu.Bit(7, val, ref reg);
            SetValue(reg);
        }

        public byte GetPatternTableAddress() => (byte)((GetValue() & 0x10) == 0x10 ? 1 : 0);

        public void SetPatterTableAddressBit(bool val)
        {
            byte reg = GetValue();

            Ppu.Bit(4, val, ref reg);
            SetValue(reg);
        }

        public bool GetVRamAddressIncrement() => (GetValue() & 0x04) == 0x04;

        public void SetVramAddressIncrement(bool val)
        {
            byte reg = GetValue();

            Ppu.Bit(2, val, ref reg);
            SetValue(reg);
        }

        public byte GetNametableAddress() => (byte)(GetValue() & 0x03);

        public void SetNametableAddress(byte val)
        {
            SetValue((byte)(GetValue() >> 2 | val));
        }
    }
}
