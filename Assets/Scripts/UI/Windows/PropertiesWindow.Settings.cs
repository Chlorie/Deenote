using UnityEngine;

namespace Deenote.UI.Windows
{
    partial class PropertiesWindow
    {
        private bool __isIneffectivePropertiesVisible;

        [SerializeField] GameObject[] _ineffectivePropertyGameObjects;

        public bool IsIneffectivePropertiesVisible
        {
            get => __isIneffectivePropertiesVisible;
            set {
                if (__isIneffectivePropertiesVisible == value) {
                    return;
                }
                __isIneffectivePropertiesVisible = value;

                foreach (var go in _ineffectivePropertyGameObjects) {
                    go.SetActive(__isIneffectivePropertiesVisible);
                }

                MainSystem.PreferenceWindow.NotifyIsIneffectivePropertiesVisible(__isIneffectivePropertiesVisible);
            }
        }
    }
}