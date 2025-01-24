#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    public sealed class ToggleButtonGroup : MonoBehaviour
    {
        [SerializeField] bool _allowToggleOff;

        private ToggleButton? _currentOnToggleButton;

        private readonly List<ToggleButton> _toggleButtons = new();

        public bool AllowToggleOff => _allowToggleOff;

        private bool _isInteractable_bf;
        public bool IsInteractable
        {
            get => _isInteractable_bf;
            set {
                if (Utils.SetField(ref _isInteractable_bf, value)) {
                    foreach (var toggle in _toggleButtons) {
                        toggle.UpdateVisual();
                    }
                }
            }
        }

        public ToggleButton? ToggledOnButton => _currentOnToggleButton;

        public event Action<ToggleButton?>? ToggledOnButtonChanged;

        /// <summary>
        /// Force toggle off current button, event AllowToggleOff is false
        /// </summary>
        public void ForceToggleOff()
        {
            if (_currentOnToggleButton is not null) {
                _currentOnToggleButton.SetIsCheckedInternal(false);
                _currentOnToggleButton = null;
                ToggledOnButtonChanged?.Invoke(_currentOnToggleButton);
            }
        }

        internal void AddButton(ToggleButton toggle)
        {
            Debug.Assert(toggle.Group == this);
            _toggleButtons.Add(toggle);
        }

        internal void Toggle(ToggleButton button)
        {
            Debug.Assert(button.Group == this, "Toggle button has wrong group");

            if (_allowToggleOff) {
                button.SetIsCheckedInternal(!button.IsChecked);
                if (button.IsChecked) {
                    _currentOnToggleButton?.SetIsCheckedInternal(false);
                    _currentOnToggleButton = button;
                }
                else {
                    _currentOnToggleButton = null;
                }
                ToggledOnButtonChanged?.Invoke(_currentOnToggleButton);
            }
            else {
                if (button.IsChecked)
                    return;
                button.SetIsCheckedInternal(true);
                _currentOnToggleButton?.SetIsCheckedInternal(false);
                _currentOnToggleButton = button;
            }
        }
    }
}
