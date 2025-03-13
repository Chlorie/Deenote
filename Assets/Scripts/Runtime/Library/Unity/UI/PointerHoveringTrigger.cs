#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.Library.Unity.UI
{
    public sealed class PointerHoveringTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool IsHovering { get; private set; }

        public event Action<PointerHoveringTrigger, bool>? IsHoveringChanged;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            IsHovering = true;
            IsHoveringChanged?.Invoke(this, true);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            IsHovering = false;
            IsHoveringChanged?.Invoke(this, false);
        }
    }
}