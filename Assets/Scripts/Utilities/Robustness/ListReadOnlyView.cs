using System.Collections;
using System.Collections.Generic;

namespace Deenote.Utilities.Robustness
{
    public readonly struct ListReadOnlyView<T> : IEnumerable<T>, IReadOnlyList<T>
    {
        public static readonly ListReadOnlyView<T> Empty = new(new());

        private readonly List<T> _list;

        public T this[int index] => _list[index];

        public int Count => _list.Count;

        public bool IsNull => _list is null;

        public ListReadOnlyView(List<T> list) => _list = list;

        public static implicit operator ListReadOnlyView<T>(List<T> list) => new(list);

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}