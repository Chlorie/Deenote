using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    public sealed partial class Window : MonoBehaviour, IPointerDownHandler
    {
        [Header("UI")]
        [SerializeField] WindowTitleBar _titleBar;
        [SerializeField] Button _closeButton;
        [SerializeField] GameObject _contentGameObject;
        [SerializeField] GraphicRaycaster _graphicRaycaster;

        public WindowTitleBar TitleBar => _titleBar;

        public Button CloseButton => _closeButton;

        public GameObject Content => _contentGameObject;

        [Header("Datas")]
        [SerializeField]
        private float _fixedRatio;
        [SerializeField]
        private bool __isActivated;
        private Action<bool> _onIsActivatedChanged;
        private Action _onFirstActivating;

        private bool _isInitialized;

        public float FixedAspectRatio
        {
            get => _fixedRatio;
            set {
                if (_fixedRatio == value)
                    return;
                _fixedRatio = value;
                if (_fixedRatio <= 0f)
                    return;

                // Reset size in fixed aspect ratio
                Size = Size;
            }
        }
        public bool IsFixedAspectRatio => _fixedRatio > 0f;

        public bool IsActivated
        {
            get => __isActivated;
            set {
                if (__isActivated == value)
                    return;
                __isActivated = value;
                gameObject.SetActive(__isActivated);
                // Notifiers invoked in Unity Message
            }
        }

        public Vector2 Size
        {
            get => ((RectTransform)transform).rect.size;
            set {
                if (IsFixedAspectRatio) {
                    value.x = (value.y - WindowTitleBar.Height) * FixedAspectRatio;
                }
                ((RectTransform)transform).sizeDelta = value;
            }
        }

        public void SetOnIsActivatedChanged(Action<bool> onIsActivatedChanged)
        {
            _onIsActivatedChanged = onIsActivatedChanged;
        }

        public void SetOnFirstActivating(Action action)
        {
            _onFirstActivating = action;
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => IsActivated = false);
        }

        private void OnEnable()
        {
            if (!_isInitialized) {
                _isInitialized = true;
                _onFirstActivating?.Invoke();
            }

            transform.SetAsLastSibling();
            _onIsActivatedChanged?.Invoke(true);
        }

        private void OnDisable()
        {
            _onIsActivatedChanged?.Invoke(false);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            MainSystem.WindowsManager.FocusOn(this);
        }
    }
}