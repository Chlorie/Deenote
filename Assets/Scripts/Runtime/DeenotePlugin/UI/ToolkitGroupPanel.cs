#nullable enable

using Deenote.Localization;
using Deenote.UIFramework.Controls;
using UnityEngine;

namespace Deenote.Plugin
{
    public sealed class ToolkitGroupPanel : MonoBehaviour
    {
        [SerializeField] RectTransform _columnParentRectTransform = default!;
        [SerializeField] GameObject _headerGameObject = default!;
        [SerializeField] TextBlock _headerText = default!;

        private ToolkitColumn[] _columns = default!;
        private IDeenotePluginGroup _pluginGroup = default!;

        private void Awake()
        {
            LocalizationSystem.LanguageChanged += SetHeader;
        }

        internal void OnInstantiate(DeenotePluginManager manager, IDeenotePluginGroup pluginGroup)
        {
            _pluginGroup = pluginGroup;
            var columns = pluginGroup.Plugins;
            _columns = new ToolkitColumn[columns.Length];
            for (int i = 0; i < columns.Length; i++) {
                var panel = Instantiate(manager._toolkitColumnPrefab, _columnParentRectTransform);
                _columns[i] = panel;
                panel.OnInstantiate(manager, columns[i]);
            }
            SetHeader(LocalizationSystem.CurrentLanguage);
        }

        private void SetHeader(LanguagePack lang)
        {
            var name = _pluginGroup.GetGroupName(lang.LanguageCode);
            if (name is null) {
                _headerGameObject.SetActive(false);
            }
            else {
                _headerGameObject.SetActive(true);
                _headerText.SetRawText(name);
            }
        }
    }
}