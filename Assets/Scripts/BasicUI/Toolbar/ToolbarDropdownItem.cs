using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolbarDropdownItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Callback Callback;
    public LocalizedText description;
    public Text shortcut;
    public RectTransform shortcutTransform;
    public RectTransform descriptionTransform;
    public RectTransform buttonTransform;
    public void OnClick()
    {
        ToolbarController instance = ToolbarController.instance;
        instance.CloseDropdown();
        instance.currentSelected.SetDefaultColor();
        instance.currentSelected = null;
        instance.onObjectCount = 0;
        Callback?.Invoke();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        ToolbarController.instance.onObjectCount++;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        ToolbarController.instance.onObjectCount--;
    }
}
