using Deenote.Localization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Windows
{
    public sealed class TitleBar : MonoBehaviour, IDragHandler
    {
        [SerializeField] Window _window;
        [SerializeField] LocalizedText _text;

        public void SetTitle(LocalizableText text)
        {
            _text.SetText(text);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            _window.transform.position += (Vector3)eventData.delta;
        }
    }
}