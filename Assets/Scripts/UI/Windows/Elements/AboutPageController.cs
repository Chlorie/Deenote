using Deenote.Localization;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    public sealed class AboutPageController : MonoBehaviour
    {
        [SerializeField] Toggle _menuButton;
        [SerializeField] AboutWindow _window;

        [SerializeField] SectionValues[] _sections;

        public SectionValues[] Sections => _sections;

        [Serializable]
        public struct SectionValues
        {
            [SerializeField] private string _sectionTextKey;
            [SerializeField] private string _contentTextKey;

            public readonly string SectionTextKey => _sectionTextKey;

            public readonly string ContentTextKey => _contentTextKey;
        }

        private void Awake()
        {
            _menuButton.onValueChanged.AddListener(selected =>
            {
                if (selected)
                    _window.LoadPage(this);
            });
        }
    }
}