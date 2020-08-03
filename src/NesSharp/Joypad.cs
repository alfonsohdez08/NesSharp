namespace NesSharp
{
    /// <summary>
    /// The NES controller buttons.
    /// </summary>
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
        private int _incomingData;
        private int _snapshot;
        private bool _strobe;

        /* https://wiki.nesdev.com/w/index.php/Controller_reading_code
            bit	    7    	    6    	    5    	    4    	    3    	    2    	    1    	    0    
            button	A	B	Select	Start	Up	Down	Left	Right         
         */

        public void Strobe(bool strobe)
        {
            _strobe = strobe;
            _snapshot = _incomingData;
        }

        public void PressButton(Button button)
        {
            //if (!Poll)
            //    return;

            _incomingData |= (1 << (int)button);

            //// Reload mode
            //if (Strobe)
            //{
            //    int mask = 1 << (int)button;
            //    _incomingData |= mask;
            //}

            //Register &= 0xFF;
        }

        public void ReleaseButton(Button button)
        {
            int mask = 1 << (int)button;

            _incomingData = (_incomingData | mask) ^ mask;
        }

        public int ReadState()
        {
            int buttonState = (_snapshot & 0x80) == 0x80 ? 1 : 0;

            if (!_strobe)
            {
                _snapshot <<= 1;
                _snapshot &= 0xFF;
            }

            return buttonState;
        }
    }
}
