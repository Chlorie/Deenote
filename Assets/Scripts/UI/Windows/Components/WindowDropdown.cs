using Deenote.Localization;
using Deenote.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Deenote.UI.Windows.Components
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class WindowDropdown : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private List<LocalizableText> _options = new();

        public TMP_Dropdown Dropdown => this.MaybeGetComponent(ref _dropdown);
        private TMP_Dropdown? _dropdown;

        public IReadOnlyList<LocalizableText> Options => _options;

        private void Awake()
        {
            MainSystem.Localization.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            MainSystem.Localization.OnLanguageChanged -= OnLanguageChanged;
        }

        public void AddOptions(IEnumerable<string> texts)
        {
            foreach (var text in texts) {
                var localized = LocalizableText.Raw(text);
                _options.Add(localized);
                Dropdown.options.Add(new TMP_Dropdown.OptionData(MainSystem.Localization.GetText(localized)));
            }
            Dropdown.RefreshShownValue();
        }

        public void AddOptions(IEnumerable<LocalizableText> texts)
        {
            foreach (var text in texts) {
                _options.Add(text);
                Dropdown.options.Add(new TMP_Dropdown.OptionData(MainSystem.Localization.GetText(text)));
            }
            Dropdown.RefreshShownValue();
        }

        public void ResetOptions(IEnumerable<string> texts)
        {
            _options.Clear();
            Dropdown.options.Clear();
            AddOptions(texts);
        }

        public void ResetOptions(IEnumerable<LocalizableText> texts)
        {
            _options.Clear();
            Dropdown.options.Clear();
            AddOptions(texts);
        }

        public void SetOption(int index, LocalizableText text)
        {
            _options[index] = text;
            Dropdown.options[index].text = MainSystem.Localization.GetText(text);
            Dropdown.RefreshShownValue();
        }

        public void ClearOptions()
        {
            _options.Clear();
            Dropdown.options.Clear();
            Dropdown.RefreshShownValue();
        }

        public int FindIndex(Predicate<LocalizableText> predicate)
        {
            return _options.FindIndex(predicate);
        }

        private void OnLanguageChanged(string _)
        {
            for (int i = 0; i < _options.Count; i++) {
                Dropdown.options[i].text = MainSystem.Localization.GetText(_options[i]);
            }
            Dropdown.RefreshShownValue();
        }
    }
}