using UnityEngine;
using UnityEngine.EventSystems;

// Add this on Buttons/InputFields in Windows
public class WindowClickResponse : MonoBehaviour, IPointerDownHandler
{
    public Window parentWindow;
    public void OnPointerDown(PointerEventData eventData)
    {
        parentWindow?.OnPointerDown(eventData);
    }
    // Called in editor, automatically fill Parent Window field
    private void OnValidate()
    {
        Window window = null;
        Transform windowTransform = transform;
        while (windowTransform != null && window == null)
        {
            window = windowTransform.GetComponent<Window>();
            windowTransform = windowTransform.parent;
        }
        parentWindow = window;
    }
}
