#nullable enable

using CommunityToolkit.HighPerformance;
using System;

namespace Deenote.Library.Collections.StackAlloc;
public readonly ref struct ReversedSpan<T>
{
    private readonly Span<T> _span;

    public ReversedSpan(Span<T> span)
    {
        _span = span;
    }

    public Enumerator GetEnumerator() => new Enumerator(_span);

    public ref struct Enumerator
    {
        private Span<T> _span;

        public Enumerator(Span<T> span)
        {
            _span = span;
        }

        public readonly ref T Current => ref _span.DangerousGetReferenceAt(_span.Length);

        public bool MoveNext()
        {
            if (_span.IsEmpty)
                return false;
            _span = _span[..^1];
            return true;
        }
    }
}