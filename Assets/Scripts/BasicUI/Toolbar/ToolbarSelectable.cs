using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolbarSelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public RectTransform textTransform;
    private RectTransform _buttonTransform;
    public Button button;
    public List<ToolbarOperation> operations = new List<ToolbarOperation>();
    private ToolbarController _controller;
    public Shortcut shortcut;
    public void SetActive(OperationName operationName, bool value)
    {
        foreach (ToolbarOperation operation in operations)
            if (operation.name == operationName)
            {
                operation.isActive = value;
                return;
            }
    }
    private void UpdateMainButtonSize()
    {
        Vector2 sizeDelta = _buttonTransform.sizeDelta;
        sizeDelta.x = LayoutUtility.GetPreferredWidth(textTransform) + 2 * Parameters.Params.toolbarMainButtonSideSpace;
        _buttonTransform.sizeDelta = sizeDelta;
    }
    public void OnClick()
    {
        if (WindowsController.Instance.Blocking) return;
        if (!_controller.hoverDetect)
        {
            _controller.currentSelected = this;
            SetSelectedColor();
            InitializeDropdown();
        }
        else
        {
            _controller.CloseDropdown();
            _controller.currentSelected = null;
            SetDefaultColor();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        _controller.onObjectCount++;
        if (!WindowsController.Instance.Blocking)
        {
            SetDefaultColor();
            if (!_controller.hoverDetect) return;
            InitializeDropdown();
            _controller.currentSelected?.SetDefaultColor();
            _controller.currentSelected = this;
            SetSelectedColor();
        }
        else
            SetDeactivatedColor();
    }
    public void OnPointerExit(PointerEventData eventData) => _controller.onObjectCount--;
    private void SetSelectedColor() => button.colors = new ColorBlock
    {
        normalColor = Parameters.Params.toolbarSelectableSelectedColor,
        highlightedColor = Parameters.Params.toolbarSelectableSelectedColor,
        pressedColor = Parameters.Params.toolbarSelectableDefaultColor,
        colorMultiplier = 1.0f
    };
    public void SetDefaultColor() => button.colors = new ColorBlock
    {
        normalColor = Parameters.Params.toolbarSelectableDefaultColor,
        highlightedColor = Parameters.Params.toolbarSelectableHighlightedColor,
        pressedColor = Parameters.Params.toolbarSelectableSelectedColor,
        colorMultiplier = 1.0f
    };
    public void SetDeactivatedColor() => button.colors = new ColorBlock
    {
        normalColor = Parameters.Params.toolbarSelectableDefaultColor,
        highlightedColor = Parameters.Params.toolbarSelectableDefaultColor,
        pressedColor = Parameters.Params.toolbarSelectableDefaultColor,
        colorMultiplier = 1.0f
    };
    private void InitializeDropdown()
    {
        _controller.ReturnAllDropdownItems();
        _controller.InitializeDropdownItems(operations, _buttonTransform.offsetMin.x);
        _controller.hoverDetect = true;
    }
    private void Awake()
    {
        _buttonTransform = button.GetComponent<RectTransform>();
        LanguageController.Call += UpdateMainButtonSize;
    }
    private void Start()
    {
        UpdateMainButtonSize();
        _controller = ToolbarController.Instance;
        ShortcutController.Instance.toolbarSelectables.Add(this);
    }
}
