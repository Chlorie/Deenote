#nullable enable

using Deenote.Utilities;
using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed partial class Collapsable : MonoBehaviour
    {
        [SerializeField] Button _collapseButton = default!;
        [SerializeField] RectTransform _content = default!;

        public RectTransform Content => _content;

        private bool _isCollapsed_bf;
        public bool IsExpanded
        {
            get => _isCollapsed_bf;
            set {
                if (Utils.SetField(ref _isCollapsed_bf, value)) {
                    _content.gameObject.SetActive(value);
                }
            }
        }

        private void Awake()
        {
            _collapseButton.OnClick.AddListener(() => IsExpanded = !IsExpanded);
        }
    }
}