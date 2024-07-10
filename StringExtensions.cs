#region License
/* Copyright 2014 mattmc3
 * This code is provided under the MIT license.
 * 
 * See dotmore.LICENSE for details.
 */
#endregion

namespace Bridle.Utilities
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhitespace(this string input) => string.IsNullOrWhiteSpace(input);
    }
}
