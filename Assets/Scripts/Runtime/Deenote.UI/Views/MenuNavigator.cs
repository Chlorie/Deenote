#nullable enable

using Deenote.UIFramework.Controls;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    public sealed class MenuNavigator : MonoBehaviour
    {
        [SerializeField] HorizontalLayoutGroup _layout = default!;
        [Header("Contents")]
        [SerializeField] ToggleButtonGroup _toggleGroup = default!;
        [SerializeField] ToggleButton _menuButton = default!;
        [SerializeField] ToggleButton _projectButton = default!;
        [SerializeField] ToggleButton _playerButton = default!;
        [SerializeField] ToggleButton _toolkitButton = default!;

        [SerializeField] GameObject _menuPage = default!;
        [SerializeField] GameObject _projectPage = default!;
        [SerializeField] GameObject _playerPage = default!;
        [SerializeField] GameObject _toolkitPage = default!;

        private void Awake()
        {
            _toggleGroup.ToggledOnButtonChanged += btn => UpdateLayoutPadding(btn is not null);

            _menuButton.IsCheckedChanged += _menuPage.SetActive;
            _projectButton.IsCheckedChanged += _projectPage.SetActive;
            _playerButton.IsCheckedChanged += _playerPage.SetActive;
            _toolkitButton.IsCheckedChanged += _toolkitPage.SetActive;
        }

        private void UpdateLayoutPadding(bool pageOn)
        {
            if (pageOn)
                _layout.padding.right = 0;
            else
                _layout.padding.right = _layout.padding.left;
        }

        private void OnValidate()
        {
            UpdateLayoutPadding(_menuPage.activeSelf || _projectPage.activeSelf || _playerPage.activeSelf || _toolkitPage.activeSelf);
        }
    }
}