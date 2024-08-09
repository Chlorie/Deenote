using Deenote.Localization;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Deenote.UI.Windows.Components
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class WindowDropdown : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown _dropdown;

        [Header("Datas")]
        [SerializeField] List<LocalizableText> _options = new();

        public TMP_Dropdown Dropdown => _dropdown;

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
                _dropdown.options.Add(new TMP_Dropdown.OptionData(MainSystem.Localization.GetText(localized)));
            }
            _dropdown.RefreshShownValue();
        }

        public void AddOptions(IEnumerable<LocalizableText> texts)
        {
            _options.AddRange(texts);
            foreach (var text in texts) {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(MainSystem.Localization.GetText(text)));
            }
            _dropdown.RefreshShownValue();
        }

        public void ResetOptions(IEnumerable<string> texts)
        {
            _options.Clear();
            _dropdown.options.Clear();
            AddOptions(texts);
        }

        public void ResetOptions(IEnumerable<LocalizableText> texts)
        {
            _options.Clear();
            _dropdown.options.Clear();
            AddOptions(texts);
        }

        public void SetOption(int index, LocalizableText text)
        {
            _options[index] = text;
            _dropdown.options[index].text = MainSystem.Localization.GetText(text);
            _dropdown.RefreshShownValue();
        }

        public void ClearOptions()
        {
            _options.Clear();
            _dropdown.options.Clear();
            _dropdown.RefreshShownValue();
        }

        public int FindIndex(Predicate<LocalizableText> predicate)
        {
            return _options.FindIndex(predicate);
        }

        private void OnLanguageChanged()
        {
            for (int i = 0; i < _options.Count; i++) {
                _dropdown.options[i].text = MainSystem.Localization.GetText(_options[i]);
            }
            _dropdown.RefreshShownValue();
        }
    }
}