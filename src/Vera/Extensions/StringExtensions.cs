using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Vera.Extensions
{
    public static class StringExtensions
    {
        
        [DebuggerStepThrough]
        public static bool IsNotNullAndNotEmpty(this string s)
        {
            return !string.IsNullOrEmpty(s);
        }

        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        [DebuggerStepThrough]
        public static bool IsNotNullAndNotWhiteSpace(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        [DebuggerStepThrough]
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        [DebuggerStepThrough]
        public static bool Contains(this IEnumerable<string> values, string value, StringComparison comparison)
        {
            return values.Any(s => s.Equals(value, comparison));
        }
    }
}