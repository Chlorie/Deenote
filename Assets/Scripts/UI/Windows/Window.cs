using Cysharp.Threading.Tasks;
using Deenote.Localization;
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

        public bool GraphicRaycasterEnabled
        {
            get => _graphicRaycaster.enabled;
            set => _graphicRaycaster.enabled = value;
        }

        public UniTask OnCloseButtonClickAsync(CancellationToken cancellationToken) => _closeButton.OnClickAsync(cancellationToken);

        public void SetTitle(LocalizableText title)
        {
            _titleBar.SetTitle(title);
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnEnable()
        {
            transform.SetAsLastSibling();
        }
    }
}