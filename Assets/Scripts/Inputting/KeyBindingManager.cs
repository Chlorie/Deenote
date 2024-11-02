using Deenote.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.Inputting
{
    public class KeyBindingManager : MonoBehaviour
    {
        public bool RegisterAction(string key, Func<bool> action) => _actions.TryAdd(key, action);

        public bool RegisterAction(string key, Action action) =>
            _actions.TryAdd(key, () =>
            {
                action();
                Debug.Log("Yeah!");
                return true;
            });

        public ContextualKeyBindingList GetBindings(string key)
        {
            if (!_actions.ContainsKey(key))
                throw new KeyNotFoundException($"There is no registered action with key {key}");
            if (_bindings.TryGetValue(key, out var list))
                return list;
            return _bindings[key] = new ContextualKeyBindingList();
        }

        public bool CheckKeyBinding(KeyBinding binding) =>
            binding.IsActive(_activeModifiers.Value) && !_inputFieldIsSelected.Value;

        private Dictionary<string, Func<bool>> _actions = new();
        private Dictionary<string, ContextualKeyBindingList> _bindings = new();
        private FrameCachedProperty<KeyModifiers> _activeModifiers = new(KeyboardUtils.GetPressedModifierKeys);
        private FrameCachedProperty<bool> _inputFieldIsSelected = new(CheckInputFieldIsSelected);

        private void Update()
        {
            var mods = _activeModifiers.Value;
            foreach (var (key, list) in _bindings) {
                if (!_actions.TryGetValue(key, out var action))
                    return;

                bool ShouldStopChecking(KeyBinding binding) =>
                    binding.IsActive(mods) && (_inputFieldIsSelected.Value || action());

                if (list.GetGlobalBindings().Any(ShouldStopChecking)) return;
                // TODO: check contextual bindings
            }
        }

        private static bool CheckInputFieldIsSelected()
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return obj != null && obj.GetComponent<TMP_InputField>() != null;
        }
    }
}