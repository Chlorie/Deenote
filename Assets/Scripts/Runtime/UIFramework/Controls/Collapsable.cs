#nullable enable

using Deenote.Library;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UIFramework.Controls
{
    public sealed partial class Collapsable : UIVisualTransitionControl
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] Button _collapseButton = default!;
        [SerializeField] Image _lineImage = default!;
        [SerializeField] Image _arrowImage = default!;
        [SerializeField] RectTransform _contentRectTransform = default!;

        public Button Header => _collapseButton;
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
                    IsExpandedChanged?.Invoke(value);
                }
            }
        }

        public event Action<bool>? IsExpandedChanged;

        protected override void Awake()
        {
            base.Awake();
            _collapseButton.Clicked += () => IsExpanded = !IsExpanded;
        }

        protected override void DoVisualTransition(UIThemeArgs args)
        {
            _backgroundImage.enabled = IsExpanded;
            _contentRectTransform.gameObject.SetActive(IsExpanded);
            _arrowImage.rectTransform.localScale = _arrowImage.rectTransform.localScale with { y = IsExpanded ? -1 : 1 };
        }

        protected override void OnThemeChanged(UIThemeArgs args)
        {
            base.OnThemeChanged(args);
            _backgroundImage.color = args.CardBackgroundDefaultColor;
            _lineImage.color = args.TextDisabledColor;
            _arrowImage.color = args.TextPrimaryColor;
        }
    }
}