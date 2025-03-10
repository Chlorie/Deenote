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
            MainWindow.DialogManager.RegisterModalDialog(this);
        }

        protected void OpenSelfModalDialog()
        {
            if (!gameObject.activeSelf) {
                gameObject.SetActive(true);
                IsActiveChanged?.Invoke(this, true);
            }
            else {
                Debug.LogWarning("Trying to open modal dialog when dialog is already active");
            }
        }

        protected void CloseSelfModalDialog()
        {
            if (gameObject.activeSelf) {
                gameObject.SetActive(false);
                IsActiveChanged?.Invoke(this, false);
            }
            else {
                Debug.LogWarning("Trying to close modal dialog when dialog is already inactive");
            }
        }
    }
}