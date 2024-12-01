#nullable enable

using Deenote.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class RadioButtonGroup : MonoBehaviour
    {
        public static RadioButtonGroup Shared => SharedRadioButtonGroupProvider.Instance;

        private readonly List<RadioButton> _radioButtons = new();
        private RadioButton? _currentActiveRadio;

        private bool _isInteractable_bf;
        public bool IsInteractable
        {
            get => _isInteractable_bf;
            set {
                if (Utils.SetField(ref _isInteractable_bf, value)) {
                    foreach (var radio in _radioButtons) {
                        radio.UpdateVisual();
                    }
                }
            }
        }

        internal void AddButton(RadioButton radioButton)
        {
            Debug.Assert(radioButton.Group == this);
            _radioButtons.Add(radioButton);
        }

        internal void SelectCheckedRadio(RadioButton radio)
        {
            Debug.Assert(radio.Group == this);
            if (_currentActiveRadio != null) {
                _currentActiveRadio.SetIsChecked(false);
            }
            _currentActiveRadio = radio;
        }

        private static class SharedRadioButtonGroupProvider
        {
            public static readonly RadioButtonGroup Instance = UnityUtils.CreatePersistentComponent<RadioButtonGroup>();
        }
    }
}
