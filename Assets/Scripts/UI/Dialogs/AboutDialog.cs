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

        [SerializeField] LocalizedText _contentText = default!;

        private void Awake()
        {
            foreach (var item in _menuItems) {
                item.Parent = this;
            }
        }

        internal void LoadPageContent(LocalizableText contentText)
        {
            _contentText.SetText(contentText);
        }

        public void Open()
        {
            _dialog.Open();
        }
    }
}