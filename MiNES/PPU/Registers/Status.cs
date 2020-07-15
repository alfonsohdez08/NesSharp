using MiNES.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES.PPU.Registers
{
    class Status: Register<byte>
    {
        private bool _verticalBlank;
        public bool VerticalBlank
        {
            get => _verticalBlank;
            set
            {
                _verticalBlank = value;

                InternalValue.SetBit(7, value);
            }
        }

        private bool _spriteOverflow; 
        public bool SpriteOverflow
        {
            get => _spriteOverflow;
            set
            {
                _spriteOverflow = value;

                InternalValue.SetBit(5, value);
            }
        }

        private bool _spriteZeroHit;
        public bool SpriteZeroHit
        {
            get => _spriteZeroHit;
            set
            {
                _spriteZeroHit = value;

                InternalValue.SetBit(6, value);
            }
        }

        public override byte RegisterValue
        {
            get => base.RegisterValue;
            set
            {
                VerticalBlank = value.GetBit(7);
                SpriteOverflow = value.GetBit(5);

                base.RegisterValue = value;
            }
        }

        public Status()
        {
            RegisterValue = 0xA0;
        }
    }
}
