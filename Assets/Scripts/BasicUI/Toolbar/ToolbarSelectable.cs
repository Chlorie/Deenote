﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolbarSelectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UIParameters uiParameters;
    public RectTransform textTransform;
    private RectTransform buttonTransform;
    public Button button;
    public List<ButtonInfo> items = new List<ButtonInfo>();
    private ToolbarController controller;
    private void UpdateMainButtonSize()
    {
        Vector2 sizeDelta = buttonTransform.sizeDelta;
        sizeDelta.x = LayoutUtility.GetPreferredWidth(textTransform) + 2 * uiParameters.toolbarMainButtonSideSpace;
        buttonTransform.sizeDelta = sizeDelta;
    }
    public void OnClick()
    {
        if (!controller.hoverDetect)
        {
            controller.currentSelected = this;
            SetSelectedColor();
            InitializeDropdown();
        }
        else
        {
            controller.CloseDropdown();
            controller.currentSelected = null;
            SetDefaultColor();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        controller.onObjectCount++;
        if (controller.hoverDetect)
        {
            InitializeDropdown();
            controller.currentSelected?.SetDefaultColor();
            controller.currentSelected = this;
            SetSelectedColor();
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        controller.onObjectCount--;
    }
    private void SetSelectedColor()
    {
        button.colors = new ColorBlock
        {
            normalColor = uiParameters.toolbarSelectableSelectedColor,
            highlightedColor = uiParameters.toolbarSelectableSelectedColor,
            pressedColor = uiParameters.toolbarSelectableDefaultColor,
            colorMultiplier = 1.0f
        };
    }
    public void SetDefaultColor()
    {
        button.colors = new ColorBlock
        {
            normalColor = uiParameters.toolbarSelectableDefaultColor,
            highlightedColor = uiParameters.toolbarSelectableHighlightedColor,
            pressedColor = uiParameters.toolbarSelectableSelectedColor,
            colorMultiplier = 1.0f
        };
    }
    private void InitializeDropdown()
    {
        controller.ReturnAllDropdownItems();
        controller.InitializeDropdownItems(items, buttonTransform.offsetMin.x);
        controller.hoverDetect = true;
    }
    private void Awake()
    {
        buttonTransform = button.GetComponent<RectTransform>();
        LanguageController.call += UpdateMainButtonSize;
    }
    private void Start()
    {
        UpdateMainButtonSize();
        controller = ToolbarController.instance;
    }
}
