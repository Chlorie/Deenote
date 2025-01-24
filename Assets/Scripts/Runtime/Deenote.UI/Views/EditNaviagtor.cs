#nullable enable

using Deenote.UIFramework.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    public sealed class EditNaviagtor : MonoBehaviour
    {
        [SerializeField] HorizontalLayoutGroup _layout = default!;
        [Header("Contents")]
        [SerializeField] ToggleButtonGroup _toggleButtonGroup = default!;
        [SerializeField] ToggleButton _editorButton = default!;

        [SerializeField] GameObject _editorPage = default!;

        private void Awake()
        {
            _toggleButtonGroup.ToggledOnButtonChanged += btn => UpdateLayoutPadding(btn is not null);
            _editorButton.IsCheckedChanged += _editorPage.SetActive;
        }

        private void UpdateLayoutPadding(bool pageOn)
        {
            if (pageOn)
                _layout.padding.left = 0;
            else
                _layout.padding.left = _layout.padding.right;
        }

        private void OnValidate()
        {
            UpdateLayoutPadding(_editorPage.activeSelf);
        }
    }
}