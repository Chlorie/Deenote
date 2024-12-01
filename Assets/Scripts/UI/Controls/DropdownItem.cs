#nullable enable

using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class DropdownItem : MonoBehaviour
    {
        [SerializeField] Button _button = default!;

        private int _selfIndex;

        public Dropdown Dropdown { get; private set; } = default!;

        internal void OnInstantiate(Dropdown dropdown)
        {
            Dropdown = dropdown;
            _button.OnClick.AddListener(() => Dropdown.SelectedIndex = _selfIndex);
        }

        internal void Initialize(int selfIndex, Dropdown.Option option)
        {
            _selfIndex = selfIndex;
            _button.Image.sprite = option.Sprite!;
            _button.Text.SetText(option.Text);
        }
    }
}
