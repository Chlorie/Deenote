#nullable enable

using CommunityToolkit.Diagnostics;
using CommunityToolkit.HighPerformance;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Deenote.Library.Collections.StackAlloc;

public readonly ref struct ReadOnlyConcatSpan<T>
{
    private readonly ReadOnlySpan<T> _first;
    private readonly ReadOnlySpan<T> _second;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyConcatSpan(ReadOnlySpan<T> first, ReadOnlySpan<T> second)
    {
        _first = first;
        _second = second;
    }

    public int Length => _first.Length + _second.Length;

    public ref readonly T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get {
            if (index < _first.Length)
                return ref _first.DangerousGetReferenceAt(index);
            index -= _first.Length;
            // _second[index] will throw if out of range
            return ref _second[index];
        }
    }

    public bool IsEmpty => _first.IsEmpty && _second.IsEmpty;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyConcatSpan<T> Slice(int startIndex, int length)
    {
        Guard.IsLessThanOrEqualTo(startIndex + length, Length);

        if (length == 0)
            return default;

        if (startIndex < _first.Length) {
            var endIndex = startIndex + length;
            if (endIndex < _first.Length) {
                return new(_first.Slice(startIndex, length), ReadOnlySpan<T>.Empty);
            }
            else {
                endIndex -= _first.Length;
                return new(_first, _second[..endIndex]);
            }
        }
        else {
            startIndex -= _first.Length;
            return new(_second.Slice(startIndex, length), ReadOnlySpan<T>.Empty);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlyConcatSpan<T> Slice(int startIndex)
    {
        if (startIndex < _first.Length) {
            return new(_first[startIndex..], _second);
        }
        else {
            var start = startIndex - _first.Length;
            return new(_second[start..], ReadOnlySpan<T>.Empty);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(Span<T> destination)
    {
        Guard.HasSizeGreaterThanOrEqualTo(destination, Length);

        _first.CopyTo(destination);
        _second.CopyTo(destination[_first.Length..]);
    }

    public bool TryCopyTo(Span<T> destination)
    {
        if (destination.Length >= Length) {
            _first.CopyTo(destination);
            _second.CopyTo(destination[_first.Length..]);
            return true;
        }
        return false;
    }

    public T[] ToArray()
    {
        if (Length == 0)
            return Array.Empty<T>();

        T[] array = new T[Length];
        CopyTo(array);
        return array;
    }

    public override string ToString()
    {
        if (typeof(T) == typeof(char)) {
#if NETSTANDARD2_0
            return $"{_first.ToString()}{_second.ToString()}";
#else
            var buffer = (stackalloc char[Length]);

            var first = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, char>(ref _first.DangerousGetReference()), _first.Length);
            var second = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, char>(ref _second.DangerousGetReference()), _second.Length);
            first.CopyTo(buffer);
            second.CopyTo(buffer[first.Length..]);
            return new string(buffer);
#endif
        }
        return $"ReadOnlyConcatSpan<{typeof(T).Name}>[{Length}]";
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly ReadOnlyConcatSpan<T> _span;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlyConcatSpan<T> span)
        {
            _span = span;
            _index = -1;
        }

        public readonly ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index + 1;
            if (index < _span.Length) {
                _index = index;
                return true;
            }
            return false;
        }
    }
}
