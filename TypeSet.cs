using System;
using System.Collections;
using System.Collections.Generic;

namespace Bridle.Utilities
{
    public class TypeSet<T> : ICollection<Type>
    {
        private readonly HashSet<Type> _types = new HashSet<Type>();

        public void Add(Type type)
        {
            if (_types.Contains(type))
            {
                return;
            }

            if (typeof(T).IsInterface)
            {
                if (!type.ImplementsInterface(typeof(T)))
                {
                    throw new ArgumentException($"{type} doesn't implement interface {typeof(T)}.");
                }
            }
            else if (!type.IsSameOrSubclassOf(typeof(T)))
            {
                throw new ArgumentException($"{type} doesn't implement inherit from {typeof(T)}.");
            }

            _types.Add(type);
        }

        public void CopyTo(Type[] array, int arrayIndex) => _types.CopyTo(array, arrayIndex);
        public bool Remove(Type type) => _types.Remove(type);
        public int Count => _types.Count;
        public bool IsReadOnly => false;
        public IEnumerator<Type> GetEnumerator() => _types.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_types).GetEnumerator();
        public void Clear() => _types.Clear();
        public bool Contains(Type item) => _types.Contains(item);
    }
}
