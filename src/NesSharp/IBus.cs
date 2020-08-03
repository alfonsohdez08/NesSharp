using System;
using System.Collections.Generic;
using System.Text;

namespace NesSharp
{
    /// <summary>
    /// A general bus.
    /// </summary>
    interface IBus
    {
        byte Read(ushort address);
        void Write(ushort address, byte value);
    }
}
