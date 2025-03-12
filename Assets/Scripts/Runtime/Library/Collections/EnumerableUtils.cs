#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Deenote.Library.Collections
{
    public static class EnumerableUtils
    {
        public static bool TryFirst<T>(this IEnumerable<T> source, [MaybeNullWhen(false)] out T value)
        {
            using var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext()) {
                value = enumerator.Current;
                return true;
            }
            value = default;
            return false;
        }
        public static IEnumerable<T> Merge<T, TComparer>(this IEnumerable<T> first, IEnumerable<T> second, TComparer comparer) where TComparer : IComparer<T>
        {
            using var enumerator = first.GetEnumerator();
            using var enumerator2 = second.GetEnumerator();

            switch (enumerator.MoveNext(), enumerator2.MoveNext()) {
                case (true, true):
                    goto CompareAndSetNext;
                case (false, true):
                    goto IterSecondOnly;
                case (true, false):
                    goto IterFirstOnly;
                case (false, false):
                    yield break;
            }

        CompareAndSetNext:
            var left = enumerator.Current;
            var right = enumerator2.Current;
            if (comparer.Compare(left, right) <= 0) {
                yield return left;
                if (enumerator.MoveNext())
                    goto CompareAndSetNext;
                else
                    goto IterSecondOnly;
            }
            else {
                yield return right;
                if (enumerator2.MoveNext())
                    goto CompareAndSetNext;
                else
                    goto IterFirstOnly;
            }

        IterFirstOnly:
            do {
                yield return enumerator.Current;
            } while (enumerator.MoveNext());
            yield break;

        IterSecondOnly:
            do {
                yield return enumerator2.Current;
            } while (enumerator2.MoveNext());
            yield break;
        }

        public static (T Min, T Max)? MinMaxOrNull<T>(this IEnumerable<T> source)
            => MinMaxOrNull(source, Comparer<T>.Default);

        public static (T Min, T Max)? MinMaxOrNull<T, TComparer>(this IEnumerable<T> source, TComparer comparer) where TComparer : IComparer<T>
        {
            T min, max;

            using var enumerator = source.GetEnumerator();
            if (enumerator.MoveNext())
                min = max = enumerator.Current;
            else {
                return null;
            }

            while (enumerator.MoveNext()) {
                var curr = enumerator.Current;
                if (comparer.Compare(curr, min) < 0)
                    min = curr;
                if (comparer.Compare(curr, max) > 0)
                    max = curr;
            }
            return (min, max);
        }

        public static IEnumerable<(T, T)> Adjacent<T>(this IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                yield break;
            T prev = enumerator.Current;

            while (enumerator.MoveNext()) {
                var curr = enumerator.Current;
                yield return (prev, curr);
                prev = curr;
            }
        }
    }
}