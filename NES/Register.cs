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
        public virtual void SetValue(T value) => _value = value;

        /// <summary>
        /// Retrieves the value allocated in the register.
        /// </summary>
        /// <returns>The value allocated in the register.</returns>
        public virtual T GetValue() => _value;

#if DEBUG
        public override string ToString()
        {
            if (typeof(T) == typeof(byte))
            {
                byte val = byte.Parse(_value.ToString());

                var sb = new StringBuilder();
                
                sb.AppendLine($"Hex: {Convert.ToString(val, 16)}");
                sb.AppendLine($"Binary: {Convert.ToString(val, 2)}");
                sb.AppendLine($"Decimal: {val}");

                string signedNumber;
                if (val.IsNegative())
                {
                    int positiveMagnitude = val & 0b01111111;
                    signedNumber = $"-{128 - positiveMagnitude}";
                }
                else
                {
                    signedNumber = $"+{val}";
                }

                sb.AppendLine($"Signed Number: {signedNumber}");

                return sb.ToString();
            }

            return base.ToString();
        }

#endif
    }
}
