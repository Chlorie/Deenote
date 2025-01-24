#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    public sealed class RadioButtonGroup : MonoBehaviour
    {
        public static RadioButtonGroup Shared => SharedRadioButtonGroupProvider.Instance;

        private readonly List<RadioButton> _radioButtons = new();
        private RadioButton? _currentActiveRadio;

        [SerializeField] private bool _isInteractable_bf = true;
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

        internal void AddButtonOnAwake(RadioButton radioButton)
        {
            Debug.Assert(radioButton.Group == this);
            _radioButtons.Add(radioButton);
        }

        internal void SetCheckedRatio(RadioButton radio)
        {
            Guard.IsTrue(radio.Group == this, nameof(radio), "Radio button has wrong group");

            if (_currentActiveRadio != null) {
                _currentActiveRadio.SetIsCheckedInternal(false, true);
            }
            radio.SetIsCheckedInternal(true, true);
            _currentActiveRadio = radio;
        }

        private static class SharedRadioButtonGroupProvider
        {
            private static RadioButtonGroup? _instance;
            public static RadioButtonGroup Instance => _instance ??= UnityUtils.CreatePersistentComponent<RadioButtonGroup>();
        }
    }
}
