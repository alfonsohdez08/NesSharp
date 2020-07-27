using System;
using System.Collections.Generic;
using System.Text;

namespace MiNES
{
    public enum Button
    {
        Right = 0,
        Left,
        Down,
        Up,
        Start,
        Select,
        B,
        A
    }

    public class Joypad
    {
        public int Register;
        public bool Poll;

        /* https://wiki.nesdev.com/w/index.php/Controller_reading_code
            bit	    7    	    6    	    5    	    4    	    3    	    2    	    1    	    0    
            button	A	B	Select	Start	Up	Down	Left	Right         
         */

        public void PressButton(Button button)
        {
            //if (!Poll)
            //    return;
            int mask = 1 << (int)button;
            Register |= mask;
            //Register &= 0xFF;
        }
    }
}
