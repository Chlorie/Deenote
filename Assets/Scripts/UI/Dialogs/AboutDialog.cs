using Deenote.Localization;
using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed class AboutDialog : MonoBehaviour
    {
        [SerializeField] Dialog _dialog = default!;
        [SerializeField] AboutDialogNavMenuItem[] _menuItems = default!;

        [SerializeField] LocalizedText _contentTitleText = default!;
        [SerializeField] LocalizedText _contentText = default!;

        private void Awake()
        {
            foreach (var item in _menuItems) {
                item.Parent = this;
            }
        }

        internal void LoadPage(in AboutDialogNavMenuItem.Page page)
        {
            _contentTitleText.SetText(page.Title);
            _contentText.SetText(page.Content);
        }

        public void Open(int sectionIndex)
        {
            _dialog.Open();
            var menuItem = _menuItems[sectionIndex];
            LoadPage(menuItem.Pages[0]);
            menuItem.SetCollapsableState(expanded: true);
        }
    }
}