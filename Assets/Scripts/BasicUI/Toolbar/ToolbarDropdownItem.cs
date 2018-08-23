using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolbarDropdownItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Callback callback;
    public LocalizedText description;
    public Text shortcut;
    public RectTransform shortcutTransform;
    public RectTransform descriptionTransform;
    public RectTransform buttonTransform;
    public Button button;
    public void OnClick()
    {
        ToolbarController.Instance.DeselectAll();
        callback?.Invoke();
    }
    public void OnPointerEnter(PointerEventData eventData) => ToolbarController.Instance.onObjectCount++;
    public void OnPointerExit(PointerEventData eventData) => ToolbarController.Instance.onObjectCount--;
}
