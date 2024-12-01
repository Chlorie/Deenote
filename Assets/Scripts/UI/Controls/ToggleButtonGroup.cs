#nullable enable

using Deenote.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class ToggleButtonGroup : MonoBehaviour
    {
        [SerializeField] bool _allowToggleOff;

        private ToggleButton? _currentOnToggleButton;

        private readonly List<ToggleButton> _toggleButtons = new();

        private bool _isInteractable_bf;
        public bool IsInteractable
        {
            get => _isInteractable_bf;
            set {
                if(Utils.SetField(ref  _isInteractable_bf, value)) {
                    foreach (var toggle in _toggleButtons) {
                        toggle.UpdateVisual();
                    }
                }
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
                button.SetIsChecked(!button.IsChecked);
                if (button.IsChecked) {
                    if (_currentOnToggleButton is not null)
                        _currentOnToggleButton.SetIsChecked(false);
                    _currentOnToggleButton = button;
                }
                else {
                    _currentOnToggleButton = null;
                }
            }
            else {
                if (button.IsChecked)
                    return;
                button.SetIsChecked(true);
                if (_currentOnToggleButton is not null)
                    _currentOnToggleButton.SetIsChecked(false);
                _currentOnToggleButton = button;
            }
        }
    }
}