using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    [RequireComponent(typeof(Toggle))]
    public sealed class CheckBox : MonoBehaviour
    {
        [SerializeField] Toggle _toggle = default!;
        [SerializeField] Sprite _checkedSprite = default!;
        [SerializeField] Sprite _indeterminateSprite = default!;
        private bool? _status;


        public Toggle UnityToggle => _toggle;

        public bool? Value
        {
            get => _status;
            set {
                if (_status == value)
                    return;

                switch (_status, value) {
                    case (_, false):
                        _toggle.SetIsOnWithoutNotify(false);
                        break;
                    case (false, _):
                        _toggle.SetIsOnWithoutNotify(true);
                        ((Image)_toggle.graphic).sprite = value is true ? _checkedSprite : _indeterminateSprite;
                        break;
                    default:
                        ((Image)_toggle.graphic).sprite = value is true ? _checkedSprite : _indeterminateSprite;
                        break;
                }

                _status = value;
                OnValueChanged.Invoke(_status);
            }
        }

        public UnityEvent<bool?> OnValueChanged { get; } = new();

        private void Awake()
        {
            _toggle.onValueChanged.AddListener(val => SetValueWithoutNotify(val));
        }

        public void SetValueWithoutNotify(bool? value)
        {
            if (_status == value)
                return;

            switch (_status, value) {
                case (_, false):
                    _toggle.SetIsOnWithoutNotify(false);
                    break;
                case (false, _):
                    _toggle.SetIsOnWithoutNotify(true);
                    ((Image)_toggle.graphic).sprite = value is true ? _checkedSprite : _indeterminateSprite;
                    break;
                default:
                    ((Image)_toggle.graphic).sprite = value is true ? _checkedSprite : _indeterminateSprite;
                    break;
            }

            _status = value;
        }
    }
}