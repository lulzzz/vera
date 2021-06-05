using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Vera.Extensions
{
    public static class StringExtensions
    {
        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        [DebuggerStepThrough]
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }
    }
}
