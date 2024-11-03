#nullable enable

using Deenote.Localization;
using System.Collections.Immutable;

public enum ProjectAutoSaveOption
{
    Off,
    On,
    OnAndSaveJson,
}

public static class ProjectAutoSaveOptionExt
{
    public static ImmutableArray<LocalizableText> DropDownOptions = ImmutableArray.Create(
        LocalizableText.Localized("PreferencesDialog_Body_AutoSave_Off"),
        LocalizableText.Localized("PreferencesDialog_Body_AutoSave_On"),
        LocalizableText.Localized("PreferencesDialog_Body_AutoSave_OnAndSaveJson"));

    public static ProjectAutoSaveOption FromDropdownIndex(int index) => (ProjectAutoSaveOption)index;

    public static int ToDropdownIndex(this ProjectAutoSaveOption option) => (int)option;
}