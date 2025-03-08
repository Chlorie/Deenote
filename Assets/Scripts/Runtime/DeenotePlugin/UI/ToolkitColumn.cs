#nullable enable

using System.Collections.Immutable;
using UnityEngine;

namespace Deenote.Plugin
{
    public sealed class ToolkitColumn : MonoBehaviour
    {
        [SerializeField] RectTransform _contentParentTransform = default!;

        private ImmutableArray<IDeenotePlugin> _plugins;

        internal void OnInstantiate(DeenotePluginManager manager, ImmutableArray<IDeenotePlugin> plugins)
        {
            _plugins = plugins;
            foreach (var plugin in plugins) {
                var p = Instantiate(manager._toolkitButtonPrefab, _contentParentTransform);
                p.OnInstantiate(plugin);
            }
        }
    }
}