#nullable enable

using Deenote.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed partial class Collapsable : MonoBehaviour
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Button _collapseButton = default!;
        [SerializeField] Image _arrowImage = default!;
        [SerializeField] RectTransform _contentRectTransform = default!;

        public RectTransform Content => _contentRectTransform;

        [SerializeField]
        private bool _isExpanded_bf;
        public bool IsExpanded
        {
            get => _isExpanded_bf;
            set {
                if (Utils.SetField(ref _isExpanded_bf, value)) {
                    _backgroundImage.enabled = value;
                    _contentRectTransform.gameObject.SetActive(value);
                    _arrowImage.rectTransform.localScale = _arrowImage.rectTransform.localScale with { y = value ? -1 : 1 };
                }
            }
        }

        private void Awake()
        {
            _collapseButton.OnClick.AddListener(() => IsExpanded = !IsExpanded);
        }

        private void OnValidate()
        {
            _isExpanded_bf = !_isExpanded_bf;
            IsExpanded = !_isExpanded_bf;
        }
    }
}
