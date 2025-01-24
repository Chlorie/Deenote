#nullable enable

using System;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    public abstract class ModalDialog : MonoBehaviour
    {
        public event Action<ModalDialog, bool>? IsActiveChanged;

        protected virtual void Awake()
        {
            MainWindow.RegisterModalDialog(this);
        }

        protected void OpenSelfModalDialog()
        {
            gameObject.SetActive(true);
            IsActiveChanged?.Invoke(this, true);
        }

        protected void CloseSelfModalDialog()
        {
            gameObject.SetActive(false);
            IsActiveChanged?.Invoke(this, false);
        }
    }
}