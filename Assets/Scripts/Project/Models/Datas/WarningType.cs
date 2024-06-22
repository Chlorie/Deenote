namespace Deenote.Project.Models.Datas
{
    public enum WarningType
    {
        Default = -1,
        NonDefault=0,
    }

    public static class WarningTypeExtension
    {
        public static int ToInt32(this WarningType warningType) => (int)warningType;
    }
}