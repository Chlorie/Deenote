#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Utilities
{
    public abstract class FrameCachedProperty
    {
        public void UpdateValue()
        {
            int currentFrame = Time.frameCount;
            if (currentFrame == _lastUpdateFrame) return;
            _lastUpdateFrame = currentFrame;
            DoUpdateValue();
        }

        protected abstract void DoUpdateValue();

        private int _lastUpdateFrame = -1;
    }

    public class FrameCachedProperty<T> : FrameCachedProperty
    {
        public FrameCachedProperty(Func<T> getter, bool autoUpdate = false)
        {
            _getter = getter;
            if (autoUpdate)
                FrameCachedPropertyPoller.Instance.AddProperty(this);
        }

        public T Value
        {
            get {
                UpdateValue();
                return _cache!;
            }
        }

        protected virtual void OnValueUpdated(T? oldValue, T newValue) { }

        protected override void DoUpdateValue()
        {
            T? oldValue = _cache;
            _cache = _getter();
            OnValueUpdated(oldValue, _cache!);
        }

        private Func<T> _getter;
        private T? _cache;
    }

    public class FrameCachedNotifyingProperty<T> : FrameCachedProperty<T>
    {
        public FrameCachedNotifyingProperty(Func<T> getter, bool autoUpdate = false)
            : this(getter, EqualityComparer<T>.Default, autoUpdate)
        {
        }

        public FrameCachedNotifyingProperty(Func<T> getter, IEqualityComparer<T> comparer, bool autoUpdate = false)
            : base(getter, autoUpdate) => _comparer = comparer;

        public delegate void ValueChangedHandler(T? oldValue, T newValue);
        public event ValueChangedHandler? OnValueChanged;

        protected override void OnValueUpdated(T? oldValue, T newValue)
        {
            if (oldValue is null) {
                if (newValue is null)
                    return;
            }
            else if (_comparer.Equals(oldValue, newValue))
                return;
            OnValueChanged?.Invoke(oldValue, newValue);
        }

        private IEqualityComparer<T> _comparer;
    }
}