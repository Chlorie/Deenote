using UnityEngine;
using UnityEngine.EventSystems;

public class InputFieldResponse : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public void OnDeselect(BaseEventData eventData)
    {
        ShortcutController.selectedInputField--;
    }
    public void OnSelect(BaseEventData eventData)
    {
        ShortcutController.selectedInputField++;
    }
}
