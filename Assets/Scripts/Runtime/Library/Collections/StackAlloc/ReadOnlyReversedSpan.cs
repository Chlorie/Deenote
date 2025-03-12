#nullable enable

using CommunityToolkit.HighPerformance;
using System;

namespace Deenote.Library.Collections.StackAlloc;
public readonly ref struct ReadOnlyReversedSpan<T>
{
    private readonly ReadOnlySpan<T> _span;

    public ReadOnlyReversedSpan(ReadOnlySpan<T> span)
    {
        _span = span;
    }

    public Enumerator GetEnumerator() => new Enumerator(_span);

    public ref struct Enumerator
    {
        private ReadOnlySpan<T> _span;

        public Enumerator(ReadOnlySpan<T> span)
        {
            _span = span;
        }

        public readonly ref readonly T Current => ref _span.DangerousGetReferenceAt(_span.Length);

        public bool MoveNext()
        {
            if (_span.IsEmpty)
                return false;
            _span = _span[..^1];
            return true;
        }
    }
}