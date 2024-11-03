#nullable enable

using Deenote.Localization;
using Deenote.UI.Controls;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
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

        private ImmutableArray<Button> _pageButtons = default!;

        public AboutDialog Parent { get; internal set; } = default!;

        public ImmutableArray<Page> Pages => ImmutableCollectionsMarshal.AsImmutableArray(_pages);

        private void Awake()
        {
            var parentTransform = _sectionTitle.Content.transform;
            var buttonsBuilder = new Button[_pages.Length];
            foreach (ref var btn in buttonsBuilder.AsSpan()) {
                btn = Instantiate(_sectionPrefab, parentTransform);
            }
            _pageButtons = ImmutableCollectionsMarshal.AsImmutableArray(buttonsBuilder);
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

        internal void SetCollapsableState(bool expanded)
        {
            _sectionTitle.Content.SetActive(expanded);
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