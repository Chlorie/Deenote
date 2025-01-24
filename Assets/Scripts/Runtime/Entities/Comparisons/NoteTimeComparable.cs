#nullable enable

using System;
using System.Collections.Generic;

namespace Deenote.Entities.Comparisons
{
    public readonly struct NoteTimeComparable : IComparable<IStageTimeNode>
    {
        private readonly float _value;

        public NoteTimeComparable(float value) => _value = value;

        public int CompareTo(IStageTimeNode other) => Comparer<float>.Default.Compare(_value, other.Time);
    }
}