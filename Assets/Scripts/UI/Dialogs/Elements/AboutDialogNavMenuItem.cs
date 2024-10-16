using Deenote.Localization;
using Deenote.UI.Controls;
using System;
using UnityEngine;

namespace Deenote.UI.Dialogs.Elements
{
    [RequireComponent(typeof(Collapsable))]
    public sealed class AboutDialogNavMenuItem : MonoBehaviour
    {
        [SerializeField] Collapsable _sectionTitle = default!;
        [SerializeField] Page[] _pages = default!;

        [Header("Prefabs")]
        [SerializeField] Button _sectionPrefab = default!;

        private Button[] _pageButtons = default!;

        public AboutDialog Parent { get; internal set; } = default!;

        private void Awake()
        {
            var parentTransform = _sectionTitle.Content.transform;
            _pageButtons = new Button[_pages.Length];
            foreach (ref var btn in _pageButtons.AsSpan()) {
                btn = Instantiate(_sectionPrefab, parentTransform);
            }
        }

        private void Start()
        {
            Debug.Assert(_pageButtons.Length == _pages.Length);
            for (int i = 0; i < _pages.Length; i++) {
                var btn = _pageButtons[i];
                var page = _pages[i];
                btn.Text.SetText(page.Title);
                btn.OnClick.AddListener(() => Parent.LoadPage(page));
            }
        }

        [Serializable]
        public struct Page
        {
            [SerializeField] LocalizableText _title;
            [SerializeField] LocalizableText _content;

            public readonly LocalizableText Title => _title;
            public readonly LocalizableText Content => _content;
        }
    }
}