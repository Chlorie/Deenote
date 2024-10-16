using Deenote.UI.Controls;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    public sealed class DialogManager : MonoBehaviour
    {
        [SerializeField] GameObject _blocker;
        [SerializeField] Dialog[] _dialogs;

        private void Awake()
        {
            foreach (var dialog in _dialogs) {
                dialog.ActiveChanged += (dlg, active) =>
                {
                    if (active) {
                        _blocker.SetActive(true);
                        _blocker.transform.SetAsLastSibling();
                        dlg.transform.SetAsLastSibling();
                    }
                    else {
                        _blocker.SetActive(false);
                    }
                };
            }
        }
    }
}