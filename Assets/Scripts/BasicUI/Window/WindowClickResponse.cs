using UnityEngine;
using UnityEngine.EventSystems;

// Add this on Buttons/InputFields in Windows
public class WindowClickResponse : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Window _parentWindow;
    public void OnPointerDown(PointerEventData eventData)
    {
        _parentWindow?.OnPointerDown(eventData);
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
        _parentWindow = window;
    }
}
