using System;
using System.Collections.Generic;
using System.Text;

namespace NesSharp
{
    abstract class Clockeable
    {
        public int MasterClockCycles { get; set; }


        public abstract void RunUpTo(int masterClockCycles);
    }
}
