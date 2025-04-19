#nullable enable

using Deenote.Localization;
using System.Collections.Immutable;

namespace Deenote.UI.Dialogs.Elements
{
    public readonly record struct MessageBoxArgs(
        LocalizableText Title,
        LocalizableText Content,
        ImmutableArray<LocalizableText> Buttons,
        int HightlightIndex = 0)
    {
        public MessageBoxArgs(LocalizableText title, LocalizableText content,
            LocalizableText button0, int HightlightIndex = 0)
            : this(title, content, ImmutableArray.Create(button0), HightlightIndex)
        { }
        public MessageBoxArgs(LocalizableText title, LocalizableText content,
            LocalizableText button0, LocalizableText button1, int HightlightIndex = 0)
            : this(title, content, ImmutableArray.Create(button0, button1), HightlightIndex)
        { }
        public MessageBoxArgs(LocalizableText title, LocalizableText content,
            LocalizableText button0, LocalizableText button1, LocalizableText button2, int HightlightIndex = 0)
            : this(title, content, ImmutableArray.Create(button0, button1, button2), HightlightIndex)
        { }
    }
}