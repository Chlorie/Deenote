using Deenote.Localization;
using Deenote.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Windows
{
    public sealed class WindowTitleBar : MonoBehaviour, IDragHandler
    {
        public const float Height = 30f;

        [SerializeField] Window _window;
        [SerializeField] LocalizedText _text;

        public void SetTitle(LocalizableText text)
        {
            _text.SetText(text);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var pointerPos = eventData.position;
            Vector3 delta = default;
            if (pointerPos.x >= 0 && pointerPos.x <= Screen.width) {
                delta.x += eventData.delta.x;
            }
            if (pointerPos.y >= 0 && pointerPos.y <= Screen.height) {
                delta.y += eventData.delta.y;
            }

            _window.transform.position += delta;
        }
    }
}