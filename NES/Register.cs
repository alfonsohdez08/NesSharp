using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    /// <summary>
    /// Represents a general register for the 6502 CPU.
    /// </summary>
    class Register<T> where T: struct
    {
        /// <summary>
        /// Represents the value that a general register of a 6502 CPU can hold.
        /// </summary>
        private T _value;

        public Register()
        {

        }


        public Register(T initialValue)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Allocates a value in the register.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue(T value) => _value = value;

        /// <summary>
        /// Retrieves the value allocated in the register.
        /// </summary>
        /// <returns>The value allocated in the register.</returns>
        public T GetValue() => _value;
    }
}
