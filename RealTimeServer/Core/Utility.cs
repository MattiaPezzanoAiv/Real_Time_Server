using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealTimeServer
{
    public static class Utility
    {
        /// <summary>
        /// Set specific bit in a specific byte
        /// </summary>
        /// <param name="b" type="byte">referenced byte</param>
        /// <param name="pos" type="int">bit position. ATTENTION: the bits position increment from right to left.</param>
        /// <param name="value" type="bool"></param>
        /// <returns>Return a new byte with the selected bit, setted as param value</returns>
        public static byte SetBitOnByte(byte b, int pos, bool value)
        {
            return value ? (byte)(b | (1 << pos)) : (byte)(b & ~(1 << pos));
        }

        /// <summary>
        /// Check if selected bit is setted on 1
        /// </summary>
        /// <param name="b" type="byte">referenced byte</param>
        /// <param name="pos" type="int">bit position. ATTENTION: the bits position increment from right to left.</param>
        /// <returns>Return true if selected bit is 1</returns>
        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }
}
