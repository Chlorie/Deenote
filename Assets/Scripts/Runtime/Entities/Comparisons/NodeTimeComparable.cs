#nullable enable

using Deenote.Entities.Models;
using System;
using System.Collections.Generic;

namespace Deenote.Entities.Comparisons
{
    public readonly struct NodeTimeComparable : IComparable<IStageTimeNode>
    {
        private readonly float _value;

        public NodeTimeComparable(float value) => _value = value;

        public int CompareTo(IStageTimeNode other) => Comparer<float>.Default.Compare(_value, other.Time);
    }
}