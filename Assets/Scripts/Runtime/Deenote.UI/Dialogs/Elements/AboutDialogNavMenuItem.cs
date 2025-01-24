#nullable enable

using Deenote.Localization;
using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.UI.Dialogs.Elements
{
    public sealed class AboutDialogNavMenuItem : MonoBehaviour
    {
        [SerializeField] ToggleButton _button = default!;
        [SerializeField] LocalizableText _contentTitle;
        [SerializeField] LocalizableText _contentTexts;

        private AboutDialog _dialog = default!;

        public LocalizableText ContentTitle => _contentTitle;
        public LocalizableText Content => _contentTexts;

        internal void OnAwake(AboutDialog dialog)
        {
            _dialog = dialog;
            _button.IsCheckedChanged += on =>
            {
                if (on)
                    _dialog.LoadPage(this);
            };
        }

        public void Load()
        {
            _button.SetIsChecked(true);
        }
    }
}