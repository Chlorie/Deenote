#nullable enable

using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class ToggleButtonGroup : MonoBehaviour
    {
        [SerializeField] bool _allowToggleOff;

        private ToggleButton? _currentOnToggleButton;

        internal void Toggle(ToggleButton button)
        {
            Debug.Assert(button.Group == this, "Toggle button has wrong group");

            if (_allowToggleOff) {
                button.IsToggleOn = !button.IsToggleOn;
                if (button.IsToggleOn) {
                    if (_currentOnToggleButton is not null)
                        _currentOnToggleButton.IsToggleOn = false;
                    _currentOnToggleButton = button;
                }
                else {
                    _currentOnToggleButton = null;
                }
            }
            else {
                if (button.IsToggleOn)
                    return;
                button.IsToggleOn = true;
                if (_currentOnToggleButton is not null)
                    _currentOnToggleButton.IsToggleOn = false;
                _currentOnToggleButton = button;
            }
        }
    }
}