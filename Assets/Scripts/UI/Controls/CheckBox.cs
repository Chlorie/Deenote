#nullable enable

using Deenote.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed partial class CheckBox : MonoBehaviour
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Image _borderImage = default!;
        [SerializeField] Image _checkmarkImage = default!;

        [SerializeField]
        private bool _isChecked_bf;

        public bool IsChecked
        {
            get => _isChecked_bf;
            set {
                if (Utils.SetField(ref _isChecked_bf, value)) {

                }
            }
        }

        [Header("Legacy")]
        [SerializeField] Toggle _toggle = default!;
        [SerializeField] Sprite _checkedSprite = default!;
        [SerializeField] Sprite _indeterminateSprite = default!;
        private bool? _status;

        public Toggle UnityToggle => _toggle;

        public bool IsInteractable
        {
            get => _toggle.interactable;
            set => _toggle.interactable = value;
        }

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
            _toggle.onValueChanged.AddListener(val => Value = val);
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

    partial class CheckBox
        : IPointerEnterHandler, IPointerExitHandler
        , IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visual")]
        [SerializeField] bool _isHovering;
        [SerializeField] bool _isPressed;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            TranslateVisual();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isHovering = true;
            TranslateVisual();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isHovering = false;
            TranslateVisual();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            TranslateVisual();
        }

        private void Awake_TranslateVisual()
        {
            var colors = MainSystem.Args.UIColors;
            _checkmarkImage.color = colors.AccentTextDefaultColor;
        }

        private void TranslateVisual()
        {
            var colors = MainSystem.Args.UIColors;

            if (IsChecked) {
                if (_isPressed) {
                    _backgroundImage.color = colors.ControlTertiaryColor;
                    _borderImage.color = colors.AccentTertiaryColor;
                    return;
                }

                if (_isHovering) {
                    _backgroundImage.color = colors.AccentSecondaryColor;
                    _borderImage.color = colors.AccentDefaultColor;
                    return;
                }

                _backgroundImage.color = colors.AccentDefaultColor;
                _borderImage.color = colors.AccentDefaultColor;
            }
            else {
                if (_isPressed) {
                    _backgroundImage.color = colors.ControlTertiaryColor;
                    _borderImage.color = colors.AccentTertiaryColor;
                    return;
                }

                if (_isHovering) {
                    _backgroundImage.color = colors.ControlSecondaryColor;
                    _borderImage.color = colors.AccentSecondaryColor;
                    return;
                }

                _backgroundImage.color = colors.ControlDefaultColor;
                _borderImage.color = colors.AccentDefaultColor;
            }
        }

        private void OnValidate()
        {
            TranslateVisual();
            _checkmarkImage.gameObject.SetActive(IsChecked);
        }
    }
}