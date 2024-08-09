using UnityEngine;

namespace Deenote.Components
{
    public sealed class MenuDropDown : MonoBehaviour
    {
        [SerializeField] private GameObject _contentPanel = null!;

        public void OnToggle() => _contentPanel.SetActive(!_contentPanel.activeSelf);
    }
}
