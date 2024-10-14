using Deenote.Localization;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Deenote.UI.Controls
{
    public sealed class Dropdown : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown _dropdown;
        private readonly List<LocalizableText> _options = new();

        public IReadOnlyList<LocalizableText> Options => _options;

        public UnityEvent<int> OnValueChanged => _dropdown.onValueChanged;

        public void SetValueWithoutNotify(int value)
            => _dropdown.SetValueWithoutNotify(value);

        public void AddOptions(ReadOnlySpan<string> texts)
        {
            foreach (var text in texts) {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(text));
                _options.Add(LocalizableText.Raw(text));
            }
            _dropdown.RefreshShownValue();
        }

        public void AddOptions(ReadOnlySpan<LocalizableText> texts)
        {
            foreach (var text in texts) {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(MainSystem.Localization.GetText(text)));
                _options.Add(text);
            }
            _dropdown.RefreshShownValue();
        }

        public void ResetOptions(ReadOnlySpan<LocalizableText> texts)
        {
            _dropdown.options.Clear();
            _options.Clear();
            AddOptions(texts);
        }

        public void ResetOptions(ReadOnlySpan<string> texts)
        {
            _dropdown.options.Clear();
            _options.Clear();
            AddOptions(texts);
        }
    }
}