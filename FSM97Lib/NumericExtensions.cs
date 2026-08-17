using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSM97Lib
{
    public static class NumericExtensions
    {
        public static string ToStringTruncated(this double value, int decimalPlaces)
        {
            double power = Math.Pow(10, decimalPlaces);
            double truncated = Math.Truncate(value * power) / power;

            // Dynamically applies the correct format string (e.g., "F2", "F3")
            return truncated.ToString($"F{decimalPlaces}");
        }
    }
}
