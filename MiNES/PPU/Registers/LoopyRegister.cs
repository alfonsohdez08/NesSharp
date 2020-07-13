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
        /// <summary>
        /// The offset within a tile in the Y-axis (pixels of rows that need to be skipped).
        /// </summary>
        /// <remarks>It's a 3 bit value.</remarks>
        public byte FineY
        {
            get => (byte)((Value & 0x7000) >> 12);
            set
            {
                Value = (ushort)(((Value | 0x7000) ^ 0x7000) | (value << 12));
            }
        }
        
        /// <summary>
        /// The number of tiles that must be skipped in the X - axis.
        /// </summary>
        /// <remarks>It's a 5 bit value.</remarks>
        public byte CoarseX
        {
            get => (byte)(Value & 0x001F);
            set
            {
                Value = (ushort)(((Value | 0x001F) ^ 0x001F) | value);
            }
        }

        /// <summary>
        /// The number of tiles that must be skipped in the Y - axis.
        /// </summary>
        /// <remarks>It's a 5 bit value.</remarks>
        public byte CoarseY
        {
            get => (byte)((Value & 0x03E0) >> 5);
            set
            {
                Value = (ushort)(((Value | 0x03E0) ^ 0x03E0) | (value << 5));
            }
        }

        /// <summary>
        /// Holds the base of the nametable address selected minus $2000.
        /// </summary>
        public byte Nametable
        {
            get => (byte)((Value & 0x0C00) >> 10);
            set
            {
                Value = (ushort)(((Value | 0x0C00) ^ 0x0C00) | (value << 10));
            }
        }

        public ushort Address
        {
            get
            {
                ushort addr = Value;
                addr = (ushort)((addr | 0x4000) ^ 0x4000);

                return addr;
            }
        }
    }
}
