using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Window : MonoBehaviour, IPointerDownHandler
{
    private RectTransform _windowTransform;
    // Window Attributes
    public bool horizontalResizable;
    public bool verticalResizable;
    public bool movable;
    public bool blocking;
    public Vector2 minSize;
    public bool flexibleTagWidth;
    public Vector2Int aspectRatio;
    // Child components
    private RectTransform _tagTransform;
    private GameObject _contents;
    protected LocalizedText tagContent;
    private RectTransform _tagContentTransform;
    // Events
    public delegate void WindowResizeHandler(Rect rect);
    public event WindowResizeHandler OnWindowResize;
    // Actions
    [HideInInspector] public List<Operation> operations = new List<Operation>();
    // Properties
    public bool Opened => _contents.activeInHierarchy;
    private float TagWidth => Mathf.Max(LayoutUtility.GetPreferredWidth(_tagContentTransform) + Parameters.Params.tagRightSpace
      + Parameters.Params.tagLeftSpace, Parameters.Params.minTagWidth);
    private Vector2 MinPosition => new Vector2(0.0f, 45.0f - Screen.height);
    private Vector2 MaxPosition => new Vector2(Screen.width - TagWidth + 1, -25.0f);
    private Vector2 MinSize
    {
        get
        {
            float width = TagWidth - 2;
            if (!(width > minSize.x)) return minSize;
            if (aspectRatio.x == 0 || aspectRatio.y == 0) return new Vector2(width, minSize.y);
            return new Vector2(width, width * aspectRatio.y / aspectRatio.x);
        }
    }
    public Rect Rect
    {
        get
        {
            Vector2 size = _windowTransform.offsetMax - _windowTransform.offsetMin;
            Vector2 position = _windowTransform.offsetMin;
            Rect result = new Rect(position, size);
            return result;
        }
        set
        {
            if (value == Rect.zero)
            {
                MoveToCenter();
                return;
            }
            if (Rect.size != value.size) OnWindowResize?.Invoke(value);
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
            _windowTransform.offsetMin = offsetMin;
            _windowTransform.offsetMax = offsetMax;
        }
    }
    // Methods
    public void SetTagContent(params string[] texts)
    {
        if (tagContent != null) tagContent.Strings = texts;
        AdjustTagWidth();
    }
    public void MoveToCenter()
    {
        Rect currentRect = Rect;
        currentRect.position = new Vector2((int)(Screen.width / 2 - currentRect.size.x / 2),
            (int)(-Screen.height / 2 - currentRect.size.y / 2));
        Rect = currentRect;
    }
    private void AdjustTagWidth()
    {
        if (!flexibleTagWidth) return;
        Vector2 sizeDelta = _tagTransform.sizeDelta;
        sizeDelta.x = TagWidth;
        _tagTransform.sizeDelta = sizeDelta;
    }
    protected virtual void Open()
    {
        bool active = _contents.activeInHierarchy;
        _contents.SetActive(true);
        SetFocus();
        if (blocking && !active) WindowsController.Instance.AddBlockingWindow(this);
        tagContent.ForceUpdate();
    }
    public virtual void Close()
    {
        _contents.SetActive(false);
        Cursor.SetCursor(Parameters.Params.cursorDefault, Parameters.Params.cursorDefaultHotspot, CursorMode.Auto);
        WindowsController.Instance.MoveWindowToBottom(this);
        if (blocking) WindowsController.Instance.RemoveBlockingWindow(this);
        WindowsController.Instance.UpdateFocusedWindowRef();
    }
    public void SetFocus()
    {
        if (_contents.activeSelf) WindowsController.Instance.SetFocusToWindow(this);
    }
    public virtual void OnPointerDown(PointerEventData eventData) => SetFocus();
    private void Awake() => LanguageController.Call += AdjustTagWidth;
    protected virtual void Start()
    {
        Transform contentsTransform = transform.Find("Contents");
        _contents = contentsTransform.gameObject;
        _tagTransform = contentsTransform.Find("Tag").GetComponent<RectTransform>();
        tagContent = _tagTransform.GetComponentInChildren<LocalizedText>();
        _tagContentTransform = tagContent.GetComponent<RectTransform>();
        _windowTransform = GetComponent<RectTransform>();
        AdjustTagWidth();
        OnWindowResize?.Invoke(Rect);
    }
}
