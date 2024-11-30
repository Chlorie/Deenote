#nullable enable

using Deenote.Localization;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Deenote.UI.Controls
{
    public sealed class Dropdown : MonoBehaviour
    {
        [SerializeField] Button _button = default!;
        [SerializeField] RectTransform _contentRectTransform = default!;

        [SerializeField] List<Option> _options = new();

        private PooledObjectListView<DropdownItem> _dropdownItems;

        private int _selectedIndex_bf;

        public int SelectedIndex
        {
            get => _selectedIndex_bf;
            set {
                if (Utils.SetField(ref _selectedIndex_bf, value)) {
                    _button.Text.SetText(_options[value].Text);
                }
            }
        }

        private void Awake()
        {
            _dropdownItems = new PooledObjectListView<DropdownItem>(
                UnityUtils.CreateObjectPool(MainSystem.Args.UIPrefabs.DropdownItem, _contentRectTransform,
                item => item.OnInstantiate(this), defaultCapacity: 0));

            for (int i = 0; i < _options.Count; i++) {
                _dropdownItems.Add(out var item);
                item.Initialize(i, _options[i]);
            }

            _button.OnClick.AddListener(() =>
            {
                _contentRectTransform.gameObject.SetActive(!_contentRectTransform.gameObject.activeSelf);
            });

            _selectedIndex_bf = -1;
        }

        private void Start()
        {
            if (_options.Count > 0) {
                SelectedIndex = 0;
            }
        }

        [Serializable]
        public struct Option
        {
            public Sprite? Sprite;
            public LocalizableText Text;
        }

        // TODO: New control

        [SerializeField] TMP_Dropdown _dropdown = default!;
        private readonly List<LocalizableText> _optionsLegacy = new();

        public IReadOnlyList<LocalizableText> Options => _optionsLegacy;

        public UnityEvent<int> OnValueChanged => _dropdown.onValueChanged;

        public int FindIndex(Predicate<LocalizableText> predicate)
        {
            return _optionsLegacy.FindIndex(predicate);
        }

        public void SetValueWithoutNotify(int value)
            => _dropdown.SetValueWithoutNotify(value);

        public void AddOptions(ReadOnlySpan<string> texts)
        {
            foreach (var text in texts) {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(text));
                _optionsLegacy.Add(LocalizableText.Raw(text));
            }
            _dropdown.RefreshShownValue();
        }

        public void AddOptions(ReadOnlySpan<LocalizableText> texts)
        {
            foreach (var text in texts) {
                _dropdown.options.Add(new TMP_Dropdown.OptionData(MainSystem.Localization.GetText(text)));
                _optionsLegacy.Add(text);
            }
            _dropdown.RefreshShownValue();
        }

        public void ResetOptions(ReadOnlySpan<LocalizableText> texts)
        {
            _dropdown.options.Clear();
            _optionsLegacy.Clear();
            AddOptions(texts);
        }

        public void ResetOptions(ReadOnlySpan<string> texts)
        {
            _dropdown.options.Clear();
            _optionsLegacy.Clear();
            AddOptions(texts);
        }
    }
}