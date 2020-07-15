using MiNES.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.PortableExecutable;
using System.Text;

namespace MiNES.PPU.Registers
{
    class LoopyRegister: Register<ushort>
    {

        public override ushort RegisterValue
        { 
            get => base.RegisterValue;
            set
            {
                FineY = (byte)((value & 0x7000) >> 12);
                CoarseX = (byte)(value & 0x001F);
                CoarseY = (byte)((value & 0x03E0) >> 5);
                Nametable = (byte)((value & 0x0C00) >> 10);
                Address = (ushort)(value & 0x3FFF); // PPU address memory space is 14 bits
            }
        }


        private byte _fineY;

        /// <summary>
        /// The offset within a tile in the Y-axis (pixels of rows that need to be skipped).
        /// </summary>
        /// <remarks>It's a 3 bit value.</remarks>
        public byte FineY
        {
            get => _fineY;
            //get => (byte)((Value & 0x7000) >> 12);
            set
            {
                _fineY = value;

                InternalValue = (ushort)(((InternalValue | 0x7000) ^ 0x7000) | (value << 12));
                //Value = (ushort)(((Value | 0x7000) ^ 0x7000) | (value << 12));
            }
        }

        private byte _coarseX;

        /// <summary>
        /// The number of tiles that must be skipped in the X - axis.
        /// </summary>
        /// <remarks>It's a 5 bit value.</remarks>
        public byte CoarseX
        {
            //get => (byte)(Value & 0x001F);
            get => _coarseX;
            set
            {
                _coarseX = value;

                InternalValue = (ushort)(((InternalValue | 0x001F) ^ 0x001F) | value);
                //Value = (ushort)(((Value | 0x001F) ^ 0x001F) | value);
            }
        }

        private byte _coarseY;

        /// <summary>
        /// The number of tiles that must be skipped in the Y - axis.
        /// </summary>
        /// <remarks>It's a 5 bit value.</remarks>
        public byte CoarseY
        {
            //get => (byte)((Value & 0x03E0) >> 5);
            get => _coarseY;
            set
            {
                _coarseY = value;
                InternalValue = (ushort)(((InternalValue | 0x03E0) ^ 0x03E0) | (value << 5));

                //Value = (ushort)(((Value | 0x03E0) ^ 0x03E0) | (value << 5));
            }
        }

        private byte _nametable;

        /// <summary>
        /// Holds the base of the nametable address selected minus $2000.
        /// </summary>
        public byte Nametable
        {
            //get => (byte)((Value & 0x0C00) >> 10);
            get => _nametable;
            set
            {
                _nametable = value;
                InternalValue = (ushort)(((InternalValue | 0x0C00) ^ 0x0C00) | (value << 10));
                //Value = (ushort)(((Value | 0x0C00) ^ 0x0C00) | (value << 10));
            }
        }

        // Todo: change this... it seems i need only the first 14 bits
        public ushort Address { get; private set; }
        //private ushort _address;
        //public ushort Address
        //{
        //    get
        //    {
        //        ushort addr = RegisterValue;
        //        addr = (ushort)((addr | 0x4000) ^ 0x4000);

        //        return addr;
        //    }

        //    private set => _address = value;
        //}
    }
}
