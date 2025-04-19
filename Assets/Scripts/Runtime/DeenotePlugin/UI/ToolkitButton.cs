#nullable enable

using Deenote.Localization;
using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.Plugin
{
    public sealed class ToolkitButton : MonoBehaviour
    {
        [SerializeField] Button _button = default!;

        private IDeenotePlugin _plugin = default!;

        private void Awake()
        {
            _button.Clicked += () =>
            {
                _plugin.ExecuteAsync(DeenotePluginContext.Instance, new DeenotePluginArgs {
                    CurrentLanguage = LocalizationSystem.CurrentLanguage,
                });
            };
            LocalizationSystem.LanguageChanged += SetButtonText;
        }

        internal void OnInstantiate(IDeenotePlugin plugin)
        {
            _plugin = plugin;
            SetButtonText(LocalizationSystem.CurrentLanguage);
        }

        private void SetButtonText(LanguagePack lang)
        {
            _button.Text.SetRawText(_plugin.GetName(lang.LanguageCode));
        }
    }
}