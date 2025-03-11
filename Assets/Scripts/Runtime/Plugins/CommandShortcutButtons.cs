#nullable enable

using Deenote.Plugin;
using System.Collections.Immutable;

namespace Deenote.Runtime.Plugins
{
    public sealed class CommandShortcutButtons : IDeenotePluginGroup
    {
        public string? GetGroupName(string LanguageCode) => LanguageCode switch {
            "zh" => "基础命令",
            "en" or _ => "Basic Commands",
        };

        public ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; private set; }

        public CommandShortcutButtons()
        {
            Plugins = ImmutableArray.Create(
                ImmutableArray.Create<IDeenotePlugin>(
                    new DelegatePlugin("Undo", new[] { ("zh", "撤销") }, context => { context.Editor.OperationMemento.Undo(); return default; }),
                    new DelegatePlugin("Redo", new[] { ("zh", "重做") }, context => { context.Editor.OperationMemento.Redo(); return default; })),
                ImmutableArray.Create<IDeenotePlugin>(
                    new DelegatePlugin("Cut", new[] { ("zh", "剪切") }, context => { context.Editor.CutSelectedNotes(); return default; }),
                    new DelegatePlugin("Copy", new[] { ("zh", "复制") }, context => { context.Editor.CopySelectedNotes(); return default; }),
                    new DelegatePlugin("Paste", new[] { ("zh", "粘贴") }, context => { context.Editor.PasteNotes(); return default; })),
                ImmutableArray.Create<IDeenotePlugin>(
                    new DelegatePlugin("Quantize", new[] { ("zh", "吸附网格") }, context => { context.Editor.EditSelectedNotesPositionCoord(c => context.GameManager.Grids.Quantize(c, true, true)); return default; }),
                    new DelegatePlugin("Mirror", new[] { ("zh", "镜像") }, context => { context.Editor.EditSelectedNotesPosition(p => -p); return default; })));
        }

    }
}