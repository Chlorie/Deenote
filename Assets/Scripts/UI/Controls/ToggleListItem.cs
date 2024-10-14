using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    [RequireComponent(typeof(Toggle))]
    public sealed class ToggleListItem : MonoBehaviour
    {
        [SerializeField] Toggle _toggle = default!;

        private ToggleList _list;
        private int _selfIndex;

        public Toggle UnityToggle => _toggle;

        public bool Value
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }

        public UnityEvent<bool> OnValueChanged => _toggle.onValueChanged;

        private void Awake()
        {
            OnValueChanged.AddListener(val => _list.SelectedIndex = _selfIndex);
        }

        public void SetValueWithoutNotify(bool value)
            => _toggle.SetIsOnWithoutNotify(value);

        public void BindToList(ToggleList list,int index)
        {
            _list = list;
            _selfIndex = index;
        }
    }
}