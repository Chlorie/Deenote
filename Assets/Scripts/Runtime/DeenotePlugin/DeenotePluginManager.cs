#nullable enable

using Deenote.UI;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace Deenote.Plugin
{
    public sealed class DeenotePluginManager : MonoBehaviour
    {
        private DeenotePluginManager _instance = default!;

        [SerializeField] internal ToolkitGroupPanel _toolkitGroupPanelPrefab = default!;
        [SerializeField] internal ToolkitColumn _toolkitColumnPrefab = default!;
        [SerializeField] internal ToolkitButton _toolkitButtonPrefab = default!;

        [SerializeField] RectTransform _parentPanelRectTransform = default!;

        private static List<IDeenotePluginGroup> _plugins = new();

        private void Awake()
        {
            var instance = this;
#if DEBUG
            if (_instance is null)
                _instance = instance;
            else {
                Destroy(instance);
                Debug.LogError($"Unexpected multiple instances of {typeof(MainWindow).Name}.");
            }
#else
            _instance = instance;
#endif
            foreach (var group in _plugins) {
                var panel = Instantiate(_toolkitGroupPanelPrefab, _parentPanelRectTransform);
                panel.OnInstantiate(this, group);
            }
        }

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

            public string? GetGroupName(string LanguageCode) => null;

            public ImmutableArray<ImmutableArray<IDeenotePlugin>> Plugins { get; private set; }
        }
    }
}