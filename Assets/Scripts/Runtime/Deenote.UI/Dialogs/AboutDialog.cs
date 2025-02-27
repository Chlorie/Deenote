#nullable enable

using Deenote.UIFramework.Controls;
using Deenote.UI.Dialogs.Elements;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed class AboutDialog : ModalDialog
    {
        [SerializeField] Dialog _dialog = default!;

        [Header("Content")]
        [SerializeField] Collapsable _developersCollapsable = default!;
        [SerializeField] Collapsable _turorialsCollapsable = default!;
        [SerializeField] Collapsable _updateHistoryCollapsable = default!;
        [SerializeField] TextBlock _contentTitleText = default!;
        [SerializeField] TextBlock _contentText = default!;
        [SerializeField] AboutDialogNavMenuItem[] _navMenuItems = default!;

        protected override void Awake()
        {
            base.Awake();
            _dialog.CloseButton.Clicked += () => base.CloseSelfModalDialog();
            foreach (var item in _navMenuItems) {
                item.OnAwake(this);
            }
        }

        internal void LoadPage(AboutDialogNavMenuItem item)
        {
            _contentTitleText.SetText(item.ContentTitle);
            _contentText.SetText(item.Content);
        }

        public void Open(Page page)
        {
            base.OpenSelfModalDialog();

            var collap = page switch {
                Page.Developers => _developersCollapsable,
                Page.Tutorials => _turorialsCollapsable,
                Page.UpdateHistory => _updateHistoryCollapsable,
                _ => throw new System.NotImplementedException(),
            };
            var nav = collap.Content.GetChild(0).GetComponent<AboutDialogNavMenuItem>();
            nav.Load();
        }

        private void OnValidate()
        {
            _dialog ??= GetComponent<Dialog>();
        }

        public enum Page
        {
            Developers,
            Tutorials,
            UpdateHistory,
        }
    }
}