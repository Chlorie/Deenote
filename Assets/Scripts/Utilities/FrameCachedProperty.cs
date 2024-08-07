using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Utilities
{
    public class FrameCachedProperty<T>
    {
        public FrameCachedProperty(Func<T> getter) => _getter = getter;

        public T Value
        {
            get
            {
                TryUpdateValue();
                return _cache!;
            }
        }

        private Func<T> _getter;
        private T? _cache;
        private int _lastUpdateFrame = -1;

        protected virtual void OnValueUpdated(T? oldValue, T newValue) { }

        private void TryUpdateValue()
        {
            int currentFrame = Time.frameCount;
            if (currentFrame == _lastUpdateFrame) return;
            _lastUpdateFrame = currentFrame;
            T? oldValue = _cache;
            _cache = _getter();
            OnValueUpdated(oldValue, _cache!);
        }
    }

    public class FrameCachedNotifyingProperty<T> : FrameCachedProperty<T>
    {
        public FrameCachedNotifyingProperty(Func<T> getter) : this(getter, EqualityComparer<T>.Default) { }
        public FrameCachedNotifyingProperty(Func<T> getter, IEqualityComparer<T> comparer) : base(getter) => _comparer = comparer;

        public delegate void ValueChangedHandler(T? oldValue, T newValue);
        public event ValueChangedHandler? OnValueChanged;

        private IEqualityComparer<T> _comparer;

        protected override void OnValueUpdated(T? oldValue, T newValue)
        {
            if (oldValue is null)
            {
                if (newValue is null)
                    return;
            }
            else if (_comparer.Equals(oldValue, newValue))
                return;
            OnValueChanged?.Invoke(oldValue, newValue);
        }
    }
}
