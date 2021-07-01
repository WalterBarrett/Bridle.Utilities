#region License
/* Copyright 2014 mattmc3
 * This code is provided under the MIT license.
 * 
 * See dotmore.LICENSE for details.
 */
#endregion

using System;
using System.Collections.Generic;

namespace Bridle.Utilities
{
    public class Comparer2<T> : Comparer<T>
    {
        private readonly Comparison<T> _compareFunction;

        public Comparer2(Comparison<T> comparison)
        {
            if (comparison == null) throw new ArgumentNullException(nameof(comparison));
            _compareFunction = comparison;
        }

        public override int Compare(T arg1, T arg2)
        {
            return _compareFunction(arg1, arg2);
        }
    }
}
