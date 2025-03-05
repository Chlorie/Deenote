#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Localization;
using System;
using System.Collections.Generic;
using Trarizon.Library.Collections;
using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    public sealed class Dropdown : MonoBehaviour, IInteractableControl
    {
        [SerializeField] Button _button = default!;
        [SerializeField] UnityEngine.UI.Image _arrowImage = default!;
        [SerializeField] RectTransform _contentRectTransform = default!;

        [SerializeField] List<Option> _options = new();

        private PooledObjectListView<DropdownItem> _dropdownItems;

        private bool _isInteractable_bf;
        public bool IsInteractable
        {
            get => _isInteractable_bf;
            set {
                if (Utils.SetField(ref _isInteractable_bf, value)) {
                    _button.IsInteractable = value;
                }
            }
        }

        [SerializeField]
        private bool _isExpanded_bf;
        internal bool IsExpanded
        {
            get => _isExpanded_bf;
            set {
                if (Utils.SetField(ref _isExpanded_bf, value)) {
                    _contentRectTransform.gameObject.SetActive(value);
                    if (value is true)
                        DropdownExpandBlocker.Instance.EnableBlockFor(this);
                    else
                        DropdownExpandBlocker.Instance.DisableBlocker();
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

        /// <summary>
        /// Note that selected index could be negative
        /// </summary>
        public event Action<int>? SelectedIndexChanged;

        public void SetValueWithoutNotify(int value)
            => SetSelectedIndex(value, false);

        private void SetSelectedIndex(int value, bool notify)
        {
            if (value != -1 && (uint)value >= (uint)_options.Count)
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), value, "Dropdown selectedIndex out of range");

            if (Utils.SetField(ref _selectedIndex_bf, value, out var old)) {
                if (value < 0) {
                    _button.Text.gameObject.SetActive(false);
                }
                else {
                    if (old < 0)
                        _button.Text.gameObject.SetActive(true);
                    _button.Text.SetText(_options[value].Text);
                }

                if (notify)
                    SelectedIndexChanged?.Invoke(value);
            }
        }

        public ReadOnlySpan<Option> Options => _options.AsSpan();

        private void Awake()
        {
            _dropdownItems = new PooledObjectListView<DropdownItem>(
                UnityUtils.CreateObjectPool(UISystem.ThemeResources.DropdownItemPrefab, _contentRectTransform,
                item => item.OnInstantiate(this), defaultCapacity: 0));

            for (int i = 0; i < _options.Count; i++) {
                _dropdownItems.Add(out var item);
                item.Initialize(i, _options[i]);
            }

            _button.Clicked += () =>
            {
                IsExpanded = !IsExpanded;
            };

            _selectedIndex_bf = -1;
        }

        private void Start()
        {
            if (_options.Count > 0) {
                SelectedIndex = 0;
            }
        }

        private void OnValidate()
        {
            _contentRectTransform.gameObject.SetActive(IsExpanded);
        }

        [Serializable]
        public struct Option
        {
            public Sprite? Sprite;
            public LocalizableText Text;
            public object? Item;
        }

        public int FindIndex(Predicate<LocalizableText> predicate)
        {
            for (int i = 0; i < _options.Count; i++) {
                if (predicate(_options[i].Text))
                    return i;
            }
            return -1;
        }

        public int FindItemIndex(object item)
        {
            for (int i = 0; i < _options.Count; i++) {
                if (_options[i].Item == item)
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

        public void ResetOptions<T>(IReadOnlyCollection<T> items, Func<T, LocalizableText> textSelector)
        {
            _options.Clear();
            using (var resetter = _dropdownItems.Resetting()) {
                int i = 0;
                foreach (var item in items) {
                    var option = new Option { Text = textSelector(item), Item = item };
                    _options.Add(option);
                    resetter.Add(out var dropdownItem);
                    dropdownItem.Initialize(i, option);
                    i++;
                }
            }
            _dropdownItems.SetSiblingIndicesInOrder();
        }

        public void ResetOptions<T>(ReadOnlySpan<T> items, Func<T, LocalizableText> textSelector)
        {
            _options.Clear();
            using (var resetter = _dropdownItems.Resetting()) {
                int i = 0;
                foreach (var item in items) {
                    var option = new Option { Text = textSelector(item), Item = item };
                    _options.Add(option);
                    resetter.Add(out var dropdownItem);
                    dropdownItem.Initialize(i, option);
                    i++;
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
