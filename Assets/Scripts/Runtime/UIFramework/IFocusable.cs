#nullable enable

namespace Deenote.UIFramework
{
    public interface IFocusable
    {
        bool IsFocused { get; internal set; }
    }
}