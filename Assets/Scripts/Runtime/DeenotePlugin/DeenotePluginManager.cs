#nullable enable

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Deenote.Plugin
{
    public static class DeenotePluginManager
    {
        private static List<IDeenotePluginGroup> _plugins = new();

        public static IReadOnlyList<IDeenotePluginGroup> Plugins => _plugins;

        public static void RegisterPlugin(IDeenotePlugin plugin)
        {
            _plugins.Add(new SingletonPluginGroup(plugin));
        }

        public static void RegisterPluginGroup(IDeenotePluginGroup pluginGroup)
        {
            _plugins.Add(pluginGroup);
        }

        private sealed class SingletonPluginGroup : IDeenotePluginGroup
        {
            public SingletonPluginGroup(IDeenotePlugin plugin)
                => Plugins = ImmutableArray.Create(ImmutableArray.Create(plugin));

            public string? GroupName => null;

            public ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; private set; }
        }
    }
}