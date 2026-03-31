using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    public static class EnumerableExtensions
    {
        public static string Join<T>(this IEnumerable<T> values, string delim)
        {
            return String.Join(delim, values.Select(v => v == null ? "null" : v.ToString()));
        }
    }
}
