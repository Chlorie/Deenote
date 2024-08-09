namespace Deenote.UI.Windows
{
    partial class PropertiesWindow
    {
        private bool __isIneffectivePropertiesVisible;

        public bool IsIneffectivePropertiesVisible
        {
            get => __isIneffectivePropertiesVisible;
            set {
                if (__isIneffectivePropertiesVisible == value) {
                    return;
                }
                __isIneffectivePropertiesVisible = value;

                _chartSpeedInputField.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _chartRemapVMinInputField.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _chartRemapVMaxInputField.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _noteShiftInputField.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _noteSpeedInputField.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _noteDurationInputField.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _noteVibrateToggle.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _noteSwipeToggle.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _noteWarningTypeInputField.gameObject.SetActive(__isIneffectivePropertiesVisible);
                _noteEventIdInputField.gameObject.SetActive(__isIneffectivePropertiesVisible);
                
                MainSystem.PreferenceWindow.NotifyIsIneffectivePropertiesVisible(__isIneffectivePropertiesVisible);
            }
        }
    }
}