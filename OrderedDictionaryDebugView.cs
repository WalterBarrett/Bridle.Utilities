#region License
/* Copyright 2014 mattmc3
 * This code is provided under the MIT license.
 * 
 * See dotmore.LICENSE for details.
 */
#endregion

using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Bridle.Utilities
{
    [DebuggerDisplay("{" + nameof(Value) + "}", Name = "[{" + nameof(Index) + "}]: {" + nameof(Key) + "}")]
    internal class IndexedKeyValuePairs
    {
        public IDictionary Dictionary { get; private set; }
        public int Index { get; private set; }
        public object Key { get; private set; }
        public object Value { get; private set; }

        public IndexedKeyValuePairs(IDictionary dictionary, int index, object key, object value)
        {
            Index = index;
            Value = value;
            Key = key;
            Dictionary = dictionary;
        }
    }

    internal class OrderedDictionaryDebugView
    {

        private readonly IOrderedDictionary _dict;
        public OrderedDictionaryDebugView(IOrderedDictionary dict)
        {
            _dict = dict;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed), SuppressMessage("ReSharper", "UnusedMember.Global")]
        public IndexedKeyValuePairs[] IndexedKeyValuePairs
        {
            get
            {
                IndexedKeyValuePairs[] nkeys = new IndexedKeyValuePairs[_dict.Count];

                int i = 0;
                foreach (object key in _dict.Keys)
                {
                    nkeys[i] = new IndexedKeyValuePairs(_dict, i, key, _dict[key]);
                    i += 1;
                }
                return nkeys;
            }
        }
    }
}
