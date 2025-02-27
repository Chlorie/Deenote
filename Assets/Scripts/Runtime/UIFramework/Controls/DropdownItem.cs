#nullable enable

using Deenote.Library;
using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    public sealed class DropdownItem : MonoBehaviour
    {
        [SerializeField] Button _button = default!;

        private int _selfIndex;

        public Dropdown Dropdown { get; private set; } = default!;

        internal void OnInstantiate(Dropdown dropdown)
        {
            Dropdown = dropdown;
            _button.Clicked += () =>
            {
                Dropdown.SelectedIndex = _selfIndex;
                Dropdown.IsExpanded = false;
            };
        }

        internal void Initialize(int selfIndex, in Dropdown.Option option)
        {
            _selfIndex = selfIndex;
            if (option.Sprite is null)
                _button.Image.WithColorAlpha(0);
            else {
                _button.Image.WithColorAlpha(1);
                _button.Image.sprite = option.Sprite!;
            }

            _button.Text.SetText(option.Text);
        }
    }
}
