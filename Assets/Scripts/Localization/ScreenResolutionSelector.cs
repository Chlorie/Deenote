using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.UI.Dropdown;

public static class ScreenResolutionSelector
{
    public static readonly IReadOnlyList<Vector2Int> PredefinedResoluion = new[]
    {
        new Vector2Int(960, 540),
        new Vector2Int(1280, 720),
        new Vector2Int(1920, 1080)
    };

    private static Vector2Int _resolution;

    public static UnityEvent resolutionChange = new UnityEvent();

    public const string WidthKey = "Resolution Width";
    public const string HeightKey = "Resolution Height";

    public static Vector2Int Resolution
    {
        set
        {
            if (value.x == _resolution.x && value.y == _resolution.y) return;

            //保存设定分辨率
            PlayerPrefs.SetInt(WidthKey, value.x);
            PlayerPrefs.SetInt(HeightKey, value.y);
            Screen.SetResolution(value.x, value.y, false);

            Utility.stageWidth = value.x * 3 / 4;
            Utility.stageHeight = value.y;

            resolutionChange.Invoke();

            _resolution = value;
        }
        get => _resolution;
    }

    //Initialize resolution dropdown
    public static void InitializeResolutionDropdown(Dropdown dropdown)
    {
        var defaultResolution = PredefinedResoluion[0];
        var previousResolution = new Vector2Int(
                PlayerPrefs.GetInt(WidthKey, defaultResolution.x),
                PlayerPrefs.GetInt(HeightKey, defaultResolution.y)
        );

        //Only 3 opinion to select
        var optionDataList = new List<OptionData>(3);
        var index = 0;
        var currentValue = -1;
        foreach (var resolution in PredefinedResoluion)
        {
            optionDataList.Add(new OptionData { text = $"{resolution.x} × {resolution.y}" });
            if (currentValue != -1 || (resolution.x == previousResolution.x && resolution.y == previousResolution.y))
                currentValue = index;
            ++index;
        }
        dropdown.options = optionDataList;
        dropdown.value = currentValue;
    }

    public static void SetScreenResolution(int section) => Resolution = PredefinedResoluion[section];
}