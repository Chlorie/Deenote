#nullable enable

using CommunityToolkit.Diagnostics;
using System;

namespace Deenote.Library.Mathematics
{
    public class PiecewiseLinearFunction
    {
        private readonly (float X, float Y)[] _nodes;
        private readonly bool _fillHorizontalBothSide;

        public PiecewiseLinearFunction(ReadOnlySpan<(float X, float Y)> nodes, bool fillHorizontalBothSide)
        {
            _nodes = new (float, float)[nodes.Length];
            nodes.CopyTo(_nodes);
            _fillHorizontalBothSide = fillHorizontalBothSide;
        }

        public float GetY(float x)
        {
            if (!TryGetY(x, out var y))
                ThrowHelper.ThrowInvalidOperationException("No y.");
            return y;
        }

        public bool TryGetY(float x, out float y)
        {
            var index = _nodes.AsSpan().BinarySearch(new XComparable(x));
            bool found = index >= 0;
            NumberUtils.FlipNegative(ref index);
            if (index == 0) {
                if (_fillHorizontalBothSide) {
                    y = _nodes[index].Y;
                    return true;
                }
                else {
                    y = default;
                    return false;
                }

            }
            if (index == 0 || index >= _nodes.Length) {
                if (_fillHorizontalBothSide) {
                    y = _nodes[index].Y;
                    return true;
                }
                else {
                    y = default;
                    return false;
                }
            }
            else {
                if (found) {
                    y = _nodes[index].Y;
                }
                else {
                    var (prevx, prevy) = _nodes[index - 1];
                    var (nextx, nexty) = _nodes[index];
                    y = MathUtils.MapTo(x, prevx, nextx, prevy, nexty);
                }
                return true;
            }
        }

        private readonly struct XComparable : IComparable<(float X, float Y)>
        {
            private readonly float _x;

            public XComparable(float x)
            {
                _x = x;
            }

            public int CompareTo((float X, float Y) other) => _x.CompareTo(other.X);
        }
    }
}