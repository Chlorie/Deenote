using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.MenuBar.Components
{
    public sealed class MenuItemToggle : Toggle
    {
        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
        }
    }
}