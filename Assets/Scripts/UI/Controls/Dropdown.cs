#nullable enable

using Deenote.Localization;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed class Dropdown : MonoBehaviour
    {
        [SerializeField] Button _button = default!;
        [SerializeField] Image _arrowImage = default!;
        [SerializeField] RectTransform _contentRectTransform = default!;

        [SerializeField] List<Option> _options = new();

        private PooledObjectListView<DropdownItem> _dropdownItems;

        [SerializeField]
        private bool _isExpanded_bf;
        private bool IsExpanded
        {
            get => _isExpanded_bf;
            set {
                if (Utils.SetField(ref _isExpanded_bf, value)) {
                    _contentRectTransform.gameObject.SetActive(value);
                    _arrowImage.rectTransform.localScale = _arrowImage.rectTransform.localScale with { y = value ? -1 : 1 };
                }
            }
        }

        private int _selectedIndex_bf;

        public int SelectedIndex
        {
            get => _selectedIndex_bf;
            internal set => SetSelectedIndex(value, true);
        }

        public void SetValueWithoutNotify(int value)
            => SetSelectedIndex(value, false);

        private void SetSelectedIndex(int value, bool notify)
        {
            if ((uint)value >= (uint)_options.Count)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Dropdown selectedIndex out of range");

            if (Utils.SetField(ref _selectedIndex_bf, value)) {
                _button.Text.LocalizedText.SetText(_options[value].Text);

                if (notify)
                    OnValueChanged.Invoke(value);
            }
        }

        public ReadOnlySpan<Option> Options => _options.AsSpan();

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
                IsExpanded = true;
                // TODO: 展开后，屏蔽所有其他输入
                // 参考winui，expand之后其他所有地方都不能点，点一下就是取消
            });

            // TODO:需要仔细研究一下start的时机，还有-1怎么处理
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

        //[SerializeField] TMP_Dropdown _dropdown = default!;
        //private readonly List<LocalizableText> _optionsLegacy = new();

        //public IReadOnlyList<LocalizableText> Options => _optionsLegacy;

        public UnityEvent<int> OnValueChanged { get; } = new(); //=> _dropdown.onValueChanged;

        public int FindIndex(Predicate<LocalizableText> predicate)
        {
            for (int i = 0; i < _options.Count; i++) {
                if (predicate(_options[i].Text))
                    return i;
            }
            return -1;
        }

        public void ResetOptions(ReadOnlySpan<string> texts)
        {
            _options.Clear();
            using (var resetter = _dropdownItems.Resetting()) {
                for (int i = 0; i < texts.Length; i++) {
                    var option = new Option { Text = LocalizableText.Raw(texts[i]) };
                    _options.Add(option);
                    resetter.Add(out var item);
                    item.Initialize(i, option);
                }
            }
            _dropdownItems.SetSiblingIndicesInOrder();
        }

        public void ResetOptions(ReadOnlySpan<LocalizableText> texts)
        {
            _options.Clear();
            using (var resetter = _dropdownItems.Resetting()) {
                for (int i = 0; i < texts.Length; i++) {
                    var option = new Option { Text = texts[i] };
                    _options.Add(option);
                    resetter.Add(out var item);
                    item.Initialize(i, option);
                }
            }
            _dropdownItems.SetSiblingIndicesInOrder();
        }

        public void ResetOptions(ReadOnlySpan<Option> options)
        {
            _options.Clear();
            using (var resetter = _dropdownItems.Resetting()) {
                for (int i = 0; i < options.Length; i++) {
                    var option = options[i];
                    _options.Add(option);
                    resetter.Add(out var item);
                    item.Initialize(i, option);
                }
            }
            _dropdownItems.SetSiblingIndicesInOrder();
        }
    }
}
