#nullable enable

using Deenote.Plugin;
using System.Collections.Immutable;

namespace Deenote.Runtime.Plugins
{
    public sealed class CommandShortcutButtons : IDeenotePluginGroup
    {
        public string? GroupName => "Commands";

        public ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; private set; }

        public CommandShortcutButtons()
        {
            Plugins = ImmutableArray.Create(
                ImmutableArray.Create<IDeenotePlugin>(
                    new DelegatePlugin("Undo", context => { context.Editor.UndoOperation(); return default; }),
                    new DelegatePlugin("Redo", context => { context.Editor.RedoOperation(); return default; })),
                ImmutableArray.Create<IDeenotePlugin>(
                    new DelegatePlugin("Cut", context => { context.Editor.CutSelectedNotes(); return default; }),
                    new DelegatePlugin("Copy", context => { context.Editor.CopySelectedNotes(); return default; }),
                    new DelegatePlugin("Paste", context => { context.Editor.PasteNotes(); return default; })),
                ImmutableArray.Create<IDeenotePlugin>(
                    new DelegatePlugin("Quantize", context => { context.Editor.EditSelectedNotesPositionCoord(c => context.GameManager.Grids.Quantize(c, true, true)); return default; }),
                    new DelegatePlugin("Mirror", context => { context.Editor.EditSelectedNotesPosition(p => -p); return default; })));
        }
    }
}