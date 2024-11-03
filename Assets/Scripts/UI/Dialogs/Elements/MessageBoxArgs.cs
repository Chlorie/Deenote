#nullable enable

using Deenote.Localization;
using System.Collections.Immutable;

namespace Deenote.UI.Dialogs.Elements
{
    public readonly record struct MessageBoxArgs(
        LocalizableText Title,
        LocalizableText Content,
        ImmutableArray<LocalizableText> Buttons)
    {
        public MessageBoxArgs(LocalizableText title, LocalizableText content,
            LocalizableText button0)
            : this(title, content, ImmutableArray.Create(button0))
        { }
        public MessageBoxArgs(LocalizableText title, LocalizableText content,
            LocalizableText button0, LocalizableText button1)
            : this(title, content, ImmutableArray.Create(button0, button1))
        { }
        public MessageBoxArgs(LocalizableText title, LocalizableText content,
            LocalizableText button0, LocalizableText button1, LocalizableText button2)
            : this(title, content, ImmutableArray.Create(button0, button1, button2))
        { }
    }
}