using System;
using System.Collections.Generic;

namespace Deenote.Utilities
{
    public class FrameCachedPropertyPoller : PersistentSingletonBehavior<FrameCachedPropertyPoller>
    {
        public void AddProperty(FrameCachedProperty property) =>
            _properties.Add(new WeakReference<FrameCachedProperty>(property));

        private List<WeakReference<FrameCachedProperty>> _properties = new();

        private void Update() => _properties.RemoveAll(ProcessAndMaybeRemove);

        private bool ProcessAndMaybeRemove(WeakReference<FrameCachedProperty> weak)
        {
            if (!weak.TryGetTarget(out var property)) return true;
            property.UpdateValue();
            return false;
        }
    }
}