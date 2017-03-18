using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildText.src
{
    /// <summary>
    /// This contains useful functions that are used throughout the tool.
    /// </summary>
    static class Util
    {
        /// <summary>
        /// Pads the given offset to the boundary given by padValue.
        /// </summary>
        /// <param name="inputOffset">Offset to pad</param>
        /// <param name="padValue">Value to pad to</param>
        /// <returns></returns>
        static public long PadOffset(long inputOffset, int padValue)
        {
            return (inputOffset + (padValue - 1)) & ~(padValue - 1);
        }
    }
}
