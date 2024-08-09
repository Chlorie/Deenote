using Deenote.Settings;
using UnityEngine;

namespace Deenote.UI.Windows
{
    public sealed class WindowsManager : MonoBehaviour
    {
        [SerializeField] CursorsArgs _cursorsArgs;
        
        private Window _focusedWindow;

        public CursorsArgs CursorsArgs => _cursorsArgs;

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