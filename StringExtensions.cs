#region License
/* Copyright 2014 mattmc3
 * This code is provided under the MIT license.
 * 
 * See dotmore.LICENSE for details.
 */
#endregion

using System;
using static System.String;

namespace Bridle.Utilities
{
    public static class StringExtensions
    {
        /// <summary>
        /// Provides a more natural way to call String.Format() on a string.
        /// </summary>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        public static string FormatWith(this string s, params object[] args)
        {
            return s == null ? null : Format(s, args);
        }

        /// <summary>
        /// Provides a more natural way to call String.Format() on a string.
        /// </summary>
        /// <param name="provider">An object that supplies the culture specific formatting</param>
        /// <param name="args">An object array that contains zero or more objects to format</param>
        public static string FormatWith(this string s, IFormatProvider provider, params object[] args)
        {
            return s == null ? null : Format(provider, s, args);
        }

        public static bool IsNullOrWhitespace(this string input)
        {
            return IsNullOrWhiteSpace(input);
        }
    }
}
