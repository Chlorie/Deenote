#nullable enable

using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class Collapsable : MonoBehaviour
    {
        [SerializeField] Button _collapseButton = default!;
        [SerializeField] GameObject _content = default!;

        public Button CollapseButton => _collapseButton;
        public GameObject Content => _content;

        private void Awake()
        {
            _collapseButton.OnClick.AddListener(() =>
                _content.SetActive(!_content.activeSelf));
        }
    }
}