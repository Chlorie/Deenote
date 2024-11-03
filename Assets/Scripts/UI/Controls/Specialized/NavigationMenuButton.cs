#nullable enable

using Deenote.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Controls.Specialized
{
    [RequireComponent(typeof(Toggle))]
    public sealed class NavigationMenuItem : MonoBehaviour
    {
        [SerializeField] Image _iconImage = default!;
        [SerializeField] LocalizedText _headerText = default!;
        [SerializeField] Toggle _toggleButton = default!;

        [SerializeField] GameObject _navPageGameObject;

        private void Awake()
        {
            _toggleButton.onValueChanged.AddListener(_navPageGameObject.SetActive);
        }

        private void Start()
        {
            _toggleButton.SetIsOnWithoutNotify(_navPageGameObject.activeSelf);
        }
    }
}