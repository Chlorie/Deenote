using UnityEngine;

namespace Deenote.Components
{
    public sealed class MenuDropDown : MonoBehaviour
    {
        [SerializeField] GameObject _contentPanel;

        public void OnToggle()
        {
            if (_contentPanel.activeSelf) {
                _contentPanel.SetActive(false);
            }
            else {
                _contentPanel.SetActive(true);
            }
        }
    }
}
