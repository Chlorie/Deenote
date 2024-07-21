using Cysharp.Threading.Tasks;
using Deenote.Localization;
using System;
using System.Threading;
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

        public float FixedRatio => _fixedRatio;
        public bool IsFixedRatio => _fixedRatio > 0f;

        public bool IsActivated
        {
            get => __isActivated;
            set {
                if (__isActivated == value)
                    return;
                __isActivated = value;
                gameObject.SetActive(__isActivated);
                if (__isActivated) {
                    transform.SetAsLastSibling();
                }
                _onIsActivatedChanged?.Invoke(__isActivated);
            }
        }

        public Vector2 Size
        {
            get => ((RectTransform)transform).rect.size;
            set => ((RectTransform)transform).sizeDelta = value;
        }

        public void SetOnIsActivatedChanged(Action<bool> onIsActivatedChanged)
        {
            _onIsActivatedChanged = onIsActivatedChanged;
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => IsActivated = false);
        }

        private void OnEnable()
        {
            transform.SetAsLastSibling();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            MainSystem.WindowsManager.FocusOn(this);
        }
    }
}