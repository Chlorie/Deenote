#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    [RequireComponent(typeof(ToggleGroup))]
    public sealed class ToggleList : MonoBehaviour
    {
        [SerializeField] ToggleGroup _group = default!;
        private int _selectedIndex;
        private readonly List<ToggleListItem> _items = new();

        public ToggleGroup UnityToggleGroup => _group;

        private bool _interactable;
        public bool IsInteractable
        {
            get => _interactable;
            set {
                if (_interactable == value)
                    return;

                _interactable = value;
                foreach (var item in _items) {
                    item.UnityToggle.interactable = _interactable;
                }
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set {
                if (_selectedIndex == value)
                    return;

                if (value < 0) {
                    if (_selectedIndex >= 0)
                        _items[_selectedIndex].SetValueWithoutNotify(false);
                    else {
                        // No change.
                        return;
                    }
                }
                else {
                    _selectedIndex = value;
                    _items[_selectedIndex].SetValueWithoutNotify(true);
                }
                OnSelectedIndexChanged.Invoke(_selectedIndex);
            }
        }

        public UnityEvent<int> OnSelectedIndexChanged { get; } = new();

        private void Awake()
        {
            _group.GetComponentsInChildren(results: _items);
            for(int i = 0; i < _items.Count; i++) {
                var item = _items[i];
                item.BindToList(this, i);
            }
        }

        public void SetSelectedIndexWithoutNotify(int value)
        {
            if (_selectedIndex == value)
                return;

            _selectedIndex = value;
            if (value < 0) {
                if (_selectedIndex >= 0)
                    _items[_selectedIndex].SetValueWithoutNotify(false);
                else {
                    // No change.
                    return;
                }
            }
            else {
                _selectedIndex = value;
                _items[_selectedIndex].SetValueWithoutNotify(true);
            }
        }
    }
}