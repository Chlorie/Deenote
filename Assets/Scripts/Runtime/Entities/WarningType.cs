#nullable enable

namespace Deenote.Entities
{
    public enum WarningType
    {
        Default = -1,
        SpeedChange = 0,
    }

    public static class WarningTypeExt
    {
        public static readonly string[] DropdownOptions = new string[]
        {
            "Default",
            "SpeedChange",
        };

        public static int ToIndex(this WarningType warningType) => warningType - WarningType.Default;

        public static WarningType FromIndex(int index) => index - WarningType.Default;

        public static WarningType FromJsonValue(int value) => (WarningType)value;
    }
}