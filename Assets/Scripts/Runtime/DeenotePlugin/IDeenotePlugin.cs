#nullable enable

using Cysharp.Threading.Tasks;
using System.Collections.Immutable;

namespace Deenote.Plugin
{
    public interface IDeenotePlugin
    {
        string Name { get; }
        string? Description { get; }
        UniTask ExecuteAsync(DeenotePluginContext context);
    }

    public interface IDeenotePluginGroup
    {
        string? GroupName { get; }
        ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; }
    }
}