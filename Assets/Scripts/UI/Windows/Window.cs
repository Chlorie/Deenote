using Cysharp.Threading.Tasks;
using Deenote.Localization;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    public sealed class Window : MonoBehaviour
    {
        [SerializeField] TitleBar _titleBar;
        [SerializeField] Button _closeButton;
        [SerializeField] GraphicRaycaster _graphicRaycaster;

        private Action<bool> _onIsActivatedChanged;

        public bool GraphicRaycasterEnabled
        {
            get => _graphicRaycaster.enabled;
            set => _graphicRaycaster.enabled = value;
        }

        [SerializeField]
        private bool __isActivated;
        public bool IsActivated
        {
            get => __isActivated;
            set {
                if (__isActivated == value)
                    return;
                __isActivated = value;
                gameObject.SetActive(__isActivated);
                _onIsActivatedChanged?.Invoke(__isActivated);
            }
        }

        public UniTask OnCloseButtonClickAsync(CancellationToken cancellationToken) => _closeButton.OnClickAsync(cancellationToken);

        public void SetTitle(LocalizableText title)
        {
            _titleBar.SetTitle(title);
        }

        public void SetOnIsActivatedChanged(Action<bool> action)
        {
            _onIsActivatedChanged = action;
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => IsActivated = false);
        }

        private void OnEnable()
        {
            transform.SetAsLastSibling();
        }
    }
}