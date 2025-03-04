#nullable enable

using Deenote.Plugin;
using Deenote.Runtime.Plugins;
using UnityEngine;

namespace Deenote
{
    public static class PreLaunch
    {
        [RuntimeInitializeOnLoadMethod]
        private static void RegisterBuiltinPlugins()
        {
            DeenotePluginManager.RegisterPlugin(new LoadLegacyDeenoteConfigurations());
            DeenotePluginManager.RegisterPluginGroup(new CommandShortcutButtons());
        }
    }
}