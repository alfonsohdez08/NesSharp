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
        private readonly object _locker = new object();

        private int _inputs;
        private int _snapshot;
        private bool _poll;

        /// <summary>
        /// Gets the button state of the joypad.
        /// </summary>
        public byte State
        {
            get
            {
                // https://wiki.nesdev.com/w/index.php/Controller_reading_code
                int buttonState = (_snapshot & 0x80) == 0x80 ? 1 : 0;

                if (!_poll)
                {
                    _snapshot <<= 1;
                    _snapshot &= 0xFF;
                }

                return (byte)buttonState;
            }
        }

        /// <summary>
        /// Set a press signal.
        /// </summary>
        /// <param name="button">The button pressed.</param>
        public void PressButton(Button button) => _inputs |= (1 << (int)button);

        /// <summary>
        /// Unset the press signal.
        /// </summary>
        /// <param name="button">The button released (unpressed).</param>
        public void ReleaseButton(Button button)
        {
            int mask = 1 << (int)button;

            _inputs = (_inputs | mask) ^ mask;
        }

        /// <summary>
        /// Acknowledges the circuit to either start or stop capturing inputs.
        /// </summary>
        /// <param name="poll">True if start capturing inputs, otherwise false.</param>
        internal void Poll(bool poll)
        {
            _poll = poll;
            
            // If stop pulling, snapshot the inputs
            if (!_poll) 
            {
                lock(_locker)
                {
                    _snapshot = _inputs;
                }
            }
        }

        /// <summary>
        /// Resets the state of the joypad.
        /// </summary>
        public void ResetJoypadState()
        {
            _inputs = 0;
            _poll = false;

            lock (_locker)
            {
                _snapshot = 0;
            }
        }
    }
}
