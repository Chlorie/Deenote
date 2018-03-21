﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Window : MonoBehaviour, IPointerDownHandler
{
    public UIParameters uiParameters;
    private RectTransform windowTransform;
    // Window Attributes
    public bool horizontalResizable;
    public bool verticalResizable;
    public bool movable;
    public bool front;
    public Vector2 minSize;
    public bool flexibleTagWidth;
    public Vector2Int aspectRatio;
    // Child components
    public RectTransform tagTransform;
    private GameObject contents;
    [HideInInspector] public LocalizedText tagContent;
    private RectTransform tagContentTransform;
    // Properties
    private float TagWidth
    {
        get
        {
            return Mathf.Max(LayoutUtility.GetPreferredWidth(tagContentTransform) + uiParameters.tagRightSpace
                + uiParameters.tagLeftSpace, uiParameters.minTagWidth);
        }
    }
    private Vector2 MinPosition { get { return new Vector2(0.0f, 45.0f - Screen.height); } }
    private Vector2 MaxPosition { get { return new Vector2(Screen.width - TagWidth + 1, -25.0f); } }
    private Vector2 MinSize
    {
        get
        {
            float width = TagWidth - 2;
            if (width > minSize.x)
                if (aspectRatio.x == 0 || aspectRatio.y == 0)
                    return new Vector2(width, minSize.y);
                else
                    return new Vector2(width, width * aspectRatio.y / aspectRatio.x);
            else
                return minSize;
        }
    }
    public Rect rect
    {
        get
        {
            Vector2 size = windowTransform.offsetMax - windowTransform.offsetMin;
            Vector2 position = windowTransform.offsetMin;
            Rect result = new Rect(position, size);
            return result;
        }
        set
        {
            Vector2 position = value.position + new Vector2(0.0f, value.size.y);
            if (position.x < MinPosition.x) position.x = MinPosition.x;
            if (position.y < MinPosition.y) position.y = MinPosition.y;
            if (position.x > MaxPosition.x) position.x = MaxPosition.x;
            if (position.y > MaxPosition.y) position.y = MaxPosition.y;
            position -= new Vector2(0.0f, value.size.y);
            Vector2 offsetMin = position;
            Vector2 offsetMax = position + value.size;
            Vector2 minimumSize = MinSize;
            if (value.size.x < minimumSize.x) offsetMax.x += minimumSize.x - value.size.x;
            if (value.size.y < minimumSize.y) offsetMin.y -= minimumSize.y - value.size.y;
            windowTransform.offsetMin = offsetMin;
            windowTransform.offsetMax = offsetMax;
        }
    }
    // Methods
    public void MoveToCenter()
    {
        Rect currentRect = rect;
        currentRect.position = new Vector2(Screen.width / 2 - currentRect.size.x / 2,
            -Screen.height / 2 - currentRect.size.y / 2);
        rect = currentRect;
    }
    private void AdjustTagWidth()
    {
        if (flexibleTagWidth)
        {
            Vector2 sizeDelta = tagTransform.sizeDelta;
            sizeDelta.x = TagWidth;
            tagTransform.sizeDelta = sizeDelta;
        }
    }
    protected virtual void Open()
    {
        if (front) WindowsController.instance.frontWindows.Add(this);
        contents.SetActive(true);
    }
    public virtual void Close()
    {
        if (front) WindowsController.instance.frontWindows.Remove(this);
        contents.SetActive(false);
        Cursor.SetCursor(uiParameters.cursorDefault, uiParameters.cursorDefaultHotspot, CursorMode.Auto);
    }
    public void SetFocus()
    {
        if (contents.activeSelf) WindowsController.instance.SetFocusToWindow(this);
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        SetFocus();
    }
    private void Awake()
    {
        LanguageController.call += AdjustTagWidth;
    }
    private void Start()
    {
        WindowsController.instance.windows.Add(this);
        contents = transform.Find("Contents").gameObject;
        tagContent = tagTransform.GetComponentInChildren<LocalizedText>();
        tagContentTransform = tagContent.GetComponent<RectTransform>();
        windowTransform = GetComponent<RectTransform>();
        AdjustTagWidth();
    }
}