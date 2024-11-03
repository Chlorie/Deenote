#nullable enable

namespace Deenote.Project.Models.Datas
{
    public enum WarningType
    {
        Default = -1,
        NonDefault = 0,
    }

    public static class WarningTypeExt
    {
        public static int ToInt32(this WarningType warningType) => (int)warningType;

        public static WarningType FromInt32(int value) => (WarningType)value;
    }
}