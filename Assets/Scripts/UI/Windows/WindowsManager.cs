using UnityEngine;

namespace Deenote.UI.Windows
{
    public sealed class WindowsManager : MonoBehaviour
    {
        private Window _focusedWindow;

        public Window FocusedWindow => _focusedWindow;

        public void FocusOn(Window window)
        {
            if (_focusedWindow == window) {
                return;
            }
            _focusedWindow = window;
            _focusedWindow.transform.SetAsLastSibling();
        }
    }
}