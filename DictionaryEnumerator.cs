#region License
/* Copyright 2014 mattmc3
 * This code is provided under the MIT license.
 * 
 * See dotmore.LICENSE for details.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace Bridle.Utilities
{
    public class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IDisposable
    {
        readonly IEnumerator<KeyValuePair<TKey, TValue>> _impl;
        public void Dispose() => _impl.Dispose();
        public DictionaryEnumerator(IDictionary<TKey, TValue> value)
        {
            this._impl = value.GetEnumerator();
        }
        public void Reset() { _impl.Reset(); }
        public bool MoveNext() { return _impl.MoveNext(); }
        public DictionaryEntry Entry
        {
            get
            {
                var pair = _impl.Current;
                return new DictionaryEntry(pair.Key, pair.Value);
            }
        }
        public object Key { get { return _impl.Current.Key; } }
        public object Value { get { return _impl.Current.Value; } }
        public object Current { get { return Entry; } }
    }
}
