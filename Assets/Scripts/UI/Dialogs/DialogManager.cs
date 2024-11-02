using Deenote.UI.Controls;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    public sealed class DialogManager : MonoBehaviour
    {
        [SerializeField] GameObject _blocker;
        [SerializeField] Dialog[] _dialogs;

        private readonly Stack<Dialog> _activeDialogs = new();

        private void Awake()
        {
            foreach (var dialog in _dialogs) {
                dialog.ActiveChanged += (dlg, active) =>
                {
                    if (active) {
                        _activeDialogs.Push(dlg);
                        _blocker.SetActive(true);
                        _blocker.transform.SetAsLastSibling();
                        dlg.transform.SetAsLastSibling();
                    }
                    else {
                        var popped = _activeDialogs.Pop();
                        Debug.Assert(popped == dlg);
                        if (_activeDialogs.Count == 0) {
                            _blocker.SetActive(false);
                        }
                        else {
                            var topDlg = _activeDialogs.Peek();
                            topDlg.transform.SetAsLastSibling();
                        }
                    }
                };
            }
        }
    }
}