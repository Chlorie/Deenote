using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Utilities.Robustness
{
    [Serializable]
    public readonly struct PooledObjectListView<T> : IEnumerable<T> where T : class
    {
        private readonly ObjectPool<T> _pool;
        [SerializeField]
        private readonly List<T> _items;

        public T this[int index] => _items[index];

        public int Count => _items.Count;

        public PooledObjectListView(ObjectPool<T> pool)
        {
            _pool = pool;
            _items = new();
        }

        public int IndexOf(T item) => _items.IndexOf(item);

        public void Add(out T item)
        {
            item = _pool.Get();
            _items.Add(item);
        }

        public void Insert(int index, out T item)
        {
            item = _pool.Get();
            _items.Insert(index, item);
        }

        public bool Remove(T item)
        {
            if (!_items.Remove(item)) return false;
            _pool.Release(item);
            return true;
        }

        public void RemoveAt(Index index)
        {
            var offset = index.GetOffset(_items.Count);
            var item = _items[offset];
            _items.RemoveAt(offset);
            _pool.Release(item);
        }

        public void RemoveRange(Range range)
        {
            var (offset, length) = range.GetOffsetAndLength(_items.Count);
            int end = offset + length;
            for (int i = offset; i < end; i++) {
                _pool.Release(_items[i]);
            }
            _items.RemoveRange(offset, length);
        }

        public void Clear(bool clearPool = false)
        {
            foreach (var item in _items) {
                _pool.Release(item);
            }
            _items.Clear();
            if (clearPool) {
                _pool.Clear();
            }
        }

        public void SetCount(int count)
        {
            if (count <= 0) {
                Clear();
            }
            else if (count < _items.Count) {
                for (int i = count; i < _items.Count; i++) {
                    _pool.Release(_items[i]);
                }
                _items.RemoveRange(count..);
            }
            else {
                for (int i = _items.Count; i < count; i++) {
                    _items.Add(_pool.Get());
                }
            }
        }

        public ResettingScope Resetting() => new(this);

        public ReadOnlySpan<T> AsSpan() => _items.AsSpan();

        public List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

        public ResettingScope Resetting(int minCapacity)
        {
            if (_items.Capacity < minCapacity) {
                _items.Capacity = minCapacity;
            }
            return Resetting();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct ResettingScope : IDisposable
        {
            private PooledObjectListView<T> _list;
            private int _count;

            internal ResettingScope(PooledObjectListView<T> list)
            {
                _list = list;
                _count = 0;
            }

            public void Add(out T item)
            {
                if (_count < _list.Count) {
                    item = _list[_count];
                    _count++;
                }
                else {
                    _list.Add(out item);
                    _count++;
                }
            }

            public readonly void Dispose()
            {
                if (_count < _list.Count) {
                    _list.SetCount(_count);
                }
            }
        }
    }
}