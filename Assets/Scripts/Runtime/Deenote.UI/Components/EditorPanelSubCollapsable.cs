#nullable enable

using Deenote.UIFramework.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    [RequireComponent(typeof(Collapsable))]
    public sealed class EditorPanelSubCollapsable : MonoBehaviour
    {
        [SerializeField] Collapsable _collapsable = default!;
        [SerializeField] HorizontalLayoutGroup _headerLayoutGroup = default!;

        [SerializeField] int _expandedHeaderLeftPadding;

        private void Awake()
        {
            _collapsable.IsExpandedChanged += expand =>
            {
                _headerLayoutGroup.padding.left = expand ? _expandedHeaderLeftPadding : 0;
            };
        }

        private void OnValidate()
        {
            _collapsable ??= GetComponent<Collapsable>();
            _headerLayoutGroup ??= _collapsable.Header.GetComponent<HorizontalLayoutGroup>();
        }
    }
}