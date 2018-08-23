using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolbarController : MonoBehaviour
{
    public static ToolbarController Instance { get; private set; }
    [HideInInspector] public bool hoverDetect; // Once a selectable is selected, hover actions over other selectables should be detected
    public RectTransform dropdownTransform;
    [SerializeField] private ToolbarDropdownItem _dropdownItemPrefab;
    private ObjectPool<ToolbarDropdownItem> _dropdownItemPool;
    private List<ToolbarDropdownItem> _dropdownItems = new List<ToolbarDropdownItem>();
    [HideInInspector] public ToolbarSelectable currentSelected;
    [HideInInspector] public int onObjectCount;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ToolbarController");
        }
        _dropdownItemPool = new ObjectPool<ToolbarDropdownItem>(_dropdownItemPrefab, 10, dropdownTransform);
    }
    public void InitializeDropdownItems(List<ToolbarOperation> infos, float offset)
    {
        Vector2 offsetMin = dropdownTransform.offsetMin;
        offsetMin.x = offset;
        dropdownTransform.offsetMin = offsetMin;
        float maxTextWidth = 0.0f;
        for (int i = 0; i < infos.Count; i++)
        {
            ToolbarDropdownItem item = _dropdownItemPool.GetObject();
            string[] strings = infos[i].strings.Clone() as string[];
            if (infos[i].operation.shortcut != null)
                for (int j = 0; j < strings.Length; j++)
                    strings[j] += "(" + infos[i].operation.shortcut + ")";
            item.description.Strings = strings;
            item.description.Color = item.shortcut.color = infos[i].isActive
                ? Parameters.Params.toolbarSelectableEnabledTextColor
                : Parameters.Params.toolbarSelectableDisabledTextColor;
            item.shortcut.text = infos[i].globalShortcut?.ToString();
            item.callback = infos[i].operation.callback;
            item.button.interactable = infos[i].isActive;
            item.gameObject.SetActive(true);
            float textWidth = LayoutUtility.GetPreferredWidth(item.descriptionTransform) + LayoutUtility.GetPreferredWidth(item.shortcutTransform);
            if (textWidth > maxTextWidth) maxTextWidth = textWidth;
            _dropdownItems.Add(item);
        }
        for (int i = 0; i < infos.Count; i++)
        {
            Vector2 sizeDelta = _dropdownItems[i].buttonTransform.sizeDelta;
            sizeDelta.x = maxTextWidth + 2 * Parameters.Params.toolbarMainButtonSideSpace + Parameters.Params.toolbarItemMiddleSpace;
            _dropdownItems[i].buttonTransform.sizeDelta = sizeDelta;
        }
    }
    public void ReturnAllDropdownItems()
    {
        while (_dropdownItems.Count > 0)
        {
            _dropdownItems[0].gameObject.SetActive(false);
            _dropdownItemPool.ReturnObject(_dropdownItems[0]);
            _dropdownItems.RemoveAt(0);
        }
    }
    public void CloseDropdown()
    {
        ReturnAllDropdownItems();
        hoverDetect = false;
    }
    public void DeselectAll()
    {
        CloseDropdown();
        currentSelected.SetDefaultColor();
        currentSelected = null;
        onObjectCount = 0;
    }
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || onObjectCount != 0) return;
        CloseDropdown();
        currentSelected?.SetDefaultColor();
        currentSelected = null;
    }
}
