#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library.Collections;
using Deenote.Library.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Deenote.Library.Collections.Generic;
public sealed class SortedList<T> : IList<T>, IReadOnlyList<T>
{
    private T[] _array;
    private int _count;
    private int _version;
    private IComparer<T> _comparer;

    public T this[int index]
    {
        get {
            Guard.IsLessThan((uint)index, (uint)_count);
            return _array[index];
        }
        set {
            Guard.IsLessThan((uint)index, (uint)_count);
            _array[index] = value;
            NotifyEditedAt(index);
        }
    }

    public int Count => _count;

    public int Capacity
    {
        get => _array.Length;
        set {
            Guard.IsGreaterThanOrEqualTo(value, _count);
            if (value == _count)
                return;
            T[] array = new T[value];
            if (_count > 0) {
                Array.Copy(_array, array, _count);
            }
            _array = array;
        }
    }

    public IComparer<T> Comparer
    {
        get => _comparer;
        set {
            _comparer = value;
            Array.Sort(_array, 0, _count, _comparer);
        }
    }

    public SortedList(IComparer<T> comparer)
    {
        _comparer = comparer;
        _array = Array.Empty<T>();
    }

    public SortedList(int capacity, IComparer<T> comparer)
    {
        _comparer = comparer;
        _array = new T[capacity];
    }

    public ReadOnlySpan<T> AsSpan() => _array.AsSpan(0, _count);

    public ReadOnlySpan<T> AsSpan(int start, int length)
    {
        ThrowUtils.ValidateSliceArgs(start, length, _count);
        return _array.AsSpan(start, length);
    }

    public ReadOnlySpan<T> AsSpan(Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(_count);
        return AsSpan(ofs, len);
    }

    public int BinarySearch(T item) => AsSpan().BinarySearch(item, _comparer);

    public int BinarySearch<TComparable>(TComparable comparable) where TComparable : IComparable<T>
        => AsSpan().BinarySearch(comparable);

    public int LinearSearch(T item) => AsSpan().LinearSearch(item, _comparer);

    public int LinearSearchFromEnd(T item) => AsSpan().LinearSearchFromEnd(item, _comparer);

    public bool Contains(T item) => BinarySearch(item) >= 0;

    /// <summary>
    /// Binary search the index and insert with keeping the list in order
    /// </summary>
    public int Add(T item)
    {
        var index = BinarySearch(item);
        NumberUtils.FlipNegative(ref index);
        InsertAt(index, item);
        return index;
    }

    public void AddRange<TDerived>(ReadOnlySpan<TDerived> collection) where TDerived : T
    {
        foreach (var item in collection) {
            Add(item);
        }
    }

    public void AddRange<TDerived>(Span<TDerived> collection) where TDerived : T
        => AddRange((ReadOnlySpan<TDerived>)collection);

    public void AddFromEnd(T item)
    {
        var index = BinarySearch(item);
        NumberUtils.FlipNegative(ref index);
        InsertAt(index, item);
    }

    public void AddFromStart(T item)
    {
        var index = BinarySearch(item);
        NumberUtils.FlipNegative(ref index);
        InsertAt(index, item);
    }

    public void AddFrom(int searchStartIndex, T item)
    {
        if (searchStartIndex <= 0) {
            AddFromStart(item);
        }
        else if (searchStartIndex >= _count) {
            AddFromEnd(item);
        }
        else {
            if (_comparer.Compare(item, this[searchStartIndex]) < 0) {
                var index = _array.AsSpan(..searchStartIndex).LinearSearchFromEnd(item, _comparer);
                NumberUtils.FlipNegative(ref index);
                InsertAt(index, item);
            }
            else {
                var index = _array.AsSpan(searchStartIndex.._count).LinearSearch(item, _comparer);
                NumberUtils.FlipNegative(ref index);
                InsertAt(searchStartIndex + index, item);
            }
        }
    }

    public int Remove(T item)
    {
        var index = BinarySearch(item);
        if (index >= 0) {
            RemoveAt(index);
        }
        return index;
    }

    public void RemoveRange(Range range)
    {
        var (ofs, len) = range.GetOffsetAndLength(_count);
        var movCount = _count - ofs - len;
        Array.Copy(_array, ofs + len, _array, ofs, movCount);
        ArrayGrowHelper.FreeManaged(_array, (ofs + movCount)..);
        _count -= len;
        _version++;
    }

    public void RemoveRange<TDerived>(ReadOnlySpan<TDerived> collection) where TDerived : T
    {
        foreach (var item in collection) {
            Remove(item);
        }
    }

    public void RemoveRange<TDerived>(Span<TDerived> collection) where TDerived : T
        => RemoveRange((ReadOnlySpan<TDerived>)collection);

    public int RemoveFrom(T item, int searchStartIndex)
    {
        if (searchStartIndex <= 0) {
            var index = LinearSearch(item);
            if (index >= 0) {
                RemoveAt(index);
            }
            return index;
        }
        else if (searchStartIndex >= _count) {
            var index = LinearSearchFromEnd(item);
            if (index >= 0) {
                RemoveAt(index);
            }
            return index;
        }
        else {
            if (_comparer.Compare(item, this[searchStartIndex]) < 0) {
                var index = _array.AsSpan(..searchStartIndex).LinearSearchFromEnd(item, _comparer);
                if (index >= 0) {
                    RemoveAt(index);
                }
                return index;
            }
            else {
                var index = _array.AsSpan(searchStartIndex.._count).LinearSearch(item, _comparer);
                if (index >= 0) {
                    var rmvIndex = searchStartIndex + index;
                    RemoveAt(rmvIndex);
                    return rmvIndex;
                }
                return index - searchStartIndex;
            }
        }
    }

    public void RemoveAt(int index)
    {
        Guard.IsLessThan((uint)index, (uint)_count);

        Array.Copy(_array, index + 1, _array, index, _count - index - 1);
        ArrayGrowHelper.FreeManaged(_array, _count, 1);
        _count--;
        _version++;
    }

    public void Clear()
    {
        ArrayGrowHelper.FreeManaged(_array, 0, _count);
        _count = 0;
        _version++;
    }

    public void Resort()
    {
        Array.Sort(_array, 0, _count, _comparer);
    }

    public void ResortBubble()
    {
        AlgorithmUtils.BubbleSort(_array.AsSpan(0, _count), _comparer);
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity < _array.Length)
            return;
        ArrayGrowHelper.Grow(ref _array, capacity, _count);
    }

    public void NotifyEdited(T item)
    {
        var index = BinarySearch(item);
        if (index < 0)
            return;
        NotifyEditedAt(index);
    }

    private void InsertAt(int index, T item)
    {
        Debug.Assert(_count <= _array.Length);
        if (_count == _array.Length) {
            ArrayGrowHelper.GrowForInsertion(ref _array, _count + 1, _count, index, 1);
        }
        else {
            Array.Copy(_array, index, _array, index + 1, _count - index);
        }
        _array[index] = item;
        _count++;
        _version++;
    }

    private void MoveTo(int from, int to)
    {
        Debug.Assert((uint)from < (uint)_count);
        Debug.Assert((uint)to < (uint)_count);

        _array.AsSpan().MoveTo(from, to);
    }

    private void NotifyEditedAt(int index)
    {
        Debug.Assert(index < _count);

        var editItem = _array[index];
        if (index >= 1 && _comparer.Compare(_array[index - 1], editItem) > 0) {
            var destIndex = _array.AsSpan(0, index).BinarySearch(editItem, _comparer);
            NumberUtils.FlipNegative(ref destIndex);
            MoveTo(index, destIndex);
        }
        else if (index < _count - 1 && _comparer.Compare(editItem, _array[index + 1]) > 0) {
            var destIndex = _array.AsSpan(index, _count - 1 - index).BinarySearch(editItem, _comparer);
            NumberUtils.FlipNegative(ref destIndex);
            MoveTo(index, destIndex - 1);
        }
        else {
            // No Move
        }
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ThrowUtils.ValidateSliceArgs(arrayIndex, _count, array.Length);
        AsSpan().CopyTo(array.AsSpan(arrayIndex, _count));
    }

    public Enumerator GetEnumerator() => new(this);

    bool ICollection<T>.IsReadOnly => false;

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    int IList<T>.IndexOf(T item) => Math.Max(BinarySearch(item), -1);
    void IList<T>.Insert(int index, T item) => ThrowHelper.ThrowInvalidOperationException("Cannot insert at specific position to SortedList");
    bool ICollection<T>.Remove(T item) => Remove(item) >= 0;
    void ICollection<T>.Add(T item) => Add(item);

    public struct Enumerator : IEnumerator<T>
    {
        private readonly SortedList<T> _list;
        private int _index;
        private int _version;
        private T _current;

        internal Enumerator(SortedList<T> list)
        {
            _list = list;
            _index = 0;
            _version = _list._version;
            _current = default!;
        }

        public T Current => _current;

        object IEnumerator.Current => Current!;

        public bool MoveNext()
        {
            CheckVersion();

            if (_index < 0)
                return false;

            if (_index < _list.Count) {
                _current = _list[_index];
                _index++;
                return true;
            }
            else {
                _index = -1;
                _current = default!;
                return false;
            }
        }

        public void Reset()
        {
            CheckVersion();
            _index = 0;
        }

        public readonly void Dispose() { }

        private readonly void CheckVersion()
        {
            if (_version != _list._version)
                throw new InvalidOperationException("Collection modified after enumerator created");
        }
    }
}