using Deenote.Edit;
using Deenote.GameStage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed class EditorPropertiesWindow : MonoBehaviour
    {
        [SerializeField] Window _window;
        [SerializeField] GameStageController _stage;
        [SerializeField] EditorController _editor;

        [Header("UI")]
        [Header("BPM")]
        [Header("Player")]
        [SerializeField] Button _noteSpeedDecButton;
        [SerializeField] Button _noteSpeedIncButton;
        [SerializeField] TMP_Text _noteSpeedText;

        [SerializeField] Button _musicSpeedDecButton;
        [SerializeField] Button _musicSpeedIncButton;
        [SerializeField] TMP_InputField _musicSpeedInputField;
        [SerializeField] Button _musicSpeedResetButton;
        [SerializeField] Button _musicSpeedHalfButton;

        [SerializeField] TMP_InputField _effectVolumeInputField;
        [SerializeField] Slider _effectVolumeSlider;
        [SerializeField] TMP_InputField _musicVolumeInputField;
        [SerializeField] Slider _musicVolumeSlider;
        [SerializeField] TMP_InputField _pianoVolumeInputField;
        [SerializeField] Slider _pianoVolumeSlider;

        [SerializeField] Toggle _showLinksToggle;

        [SerializeField] TMP_InputField _suddenPlusInputField;
        [SerializeField] Slider _suddenPlusSlider;

        [Header("Placement")]
        [SerializeField] Toggle _showIndicatorToggle;

        [SerializeField] TMP_InputField _horizontalGridInputField;
        [SerializeField] Toggle _horizontalGridSnapToggle;
        [SerializeField] TMP_InputField _verticalGridInputField;
        [SerializeField] Toggle _verticalGridSnapToggle;

        [SerializeField] Button _cubicCurveButton;
        [SerializeField] Button _linearCurveButton;
        [SerializeField] TMP_InputField _fillAmountInputField;
        [SerializeField] Button _fillAmountButton;
        [SerializeField] Button _fillGridButton;

        private void Awake()
        {
            _noteSpeedDecButton.onClick.AddListener(() => _stage.NoteSpeed--);
            _noteSpeedIncButton.onClick.AddListener(() => _stage.NoteSpeed++);

            _musicSpeedDecButton.onClick.AddListener(() => _stage.MusicSpeed--);
            _musicSpeedIncButton.onClick.AddListener(() => _stage.MusicSpeed++);
            _musicSpeedInputField.onSubmit.AddListener(OnMusicSpeedChanged);
            _musicSpeedHalfButton.onClick.AddListener(() => _stage.MusicSpeed = 5);
            _musicSpeedResetButton.onClick.AddListener(() => _stage.MusicSpeed = 10);

            _effectVolumeInputField.onSubmit.AddListener(OnEffectVolumeChanged);
            _effectVolumeSlider.onValueChanged.AddListener(OnEffectVolumeChanged);
            _musicVolumeInputField.onSubmit.AddListener(OnMusicVolumeChanged);
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _pianoVolumeInputField.onSubmit.AddListener(OnPianoVolumeChanged);
            _pianoVolumeSlider.onValueChanged.AddListener(OnPianoVolumeChanged);

            // TODO:
            _showLinksToggle.onValueChanged.AddListener(null);

            _suddenPlusInputField.onSubmit.AddListener(OnSuddenPlusChanged);
            _suddenPlusSlider.onValueChanged.AddListener(OnSuddenPlusChanged);

            _showIndicatorToggle.onValueChanged.AddListener(OnShowIndicatorChanged);

            _horizontalGridInputField.onValueChanged.AddListener(null);
            _horizontalGridSnapToggle.onValueChanged.AddListener(OnHorizontalGridSnapChanged);
            _verticalGridInputField.onValueChanged.AddListener(null);
            _verticalGridSnapToggle.onValueChanged.AddListener(OnVerticalGridSnapChanged);

            _cubicCurveButton.onClick.AddListener(null);
            _linearCurveButton.onClick.AddListener(null);
            _fillAmountInputField.onSubmit.AddListener(null);
            _fillAmountButton.onClick.AddListener(null);
            _fillGridButton.onClick.AddListener(null);
        }

        #region UI Events

        // Invoke events to change values in GameStageController or somewhere,
        // GameStageController will call auto callbacks

        private void OnMusicSpeedChanged(string text)
        {
            if (!float.TryParse(text, out var val))
                return;

            int intVal = Mathf.RoundToInt(val * 10);
            _stage.MusicSpeed = Mathf.Clamp(intVal, 1, 30);
        }

        private void OnEffectVolumeChanged(string text)
        {
            if (!int.TryParse(text, out var val))
                return;
            _stage.EffectVolume = Mathf.Clamp(val, 0, 100);
        }

        private void OnEffectVolumeChanged(float value) => _stage.EffectVolume = Mathf.Clamp((int)value, 0, 100);

        private void OnMusicVolumeChanged(string text)
        {
            if (!int.TryParse(text, out var val))
                return;
            _stage.MusicVolume = Mathf.Clamp(val, 0, 100);
        }

        private void OnMusicVolumeChanged(float value) => _stage.MusicVolume = Mathf.Clamp((int)value, 0, 100);

        private void OnPianoVolumeChanged(string text)
        {
            if (!int.TryParse(text, out var val))
                return;
            _stage.PianoVolume = Mathf.Clamp(val, 0, 100);
        }

        private void OnPianoVolumeChanged(float value) => _stage.PianoVolume = Mathf.Clamp((int)value, 0, 100);

        private void OnSuddenPlusChanged(string text)
        {
            if (!int.TryParse(text, out var val))
                return;
            _stage.SuddenPlusRange = Mathf.Clamp(val, 0, 100);
        }

        private void OnSuddenPlusChanged(float value) => _stage.SuddenPlusRange = Mathf.Clamp((int)value, 0, 100);

        private void OnShowIndicatorChanged(bool value) => _editor.IsNoteIndicatorOn = value;

        private void OnHorizontalGridSnapChanged(bool value) => _editor.SnapToTimeGrid = value;

        private void OnVerticalGridSnapChanged(bool value) => _editor.SnapToPositionGrid = value;

        #endregion

        #region Notify

        public void NofityNoteSpeedChanged(int speed)
        {
            _noteSpeedText.text = speed % 2 == 0
                ? $"{speed / 2}.0"
                : $"{speed / 2}.5";
            _noteSpeedDecButton.gameObject.SetActive(speed > 1);
            _noteSpeedIncButton.gameObject.SetActive(speed < 19);
        }

        public void NotifyMusicSpeedChanged(int speed)
        {
            _musicSpeedInputField.SetTextWithoutNotify((speed / 10f).ToString("F1"));
            _musicSpeedDecButton.gameObject.SetActive(speed > 1);
            _musicSpeedIncButton.gameObject.SetActive(speed < 30);
        }

        public void NotifyEffectVolumeChanged(int volume)
        {
            _effectVolumeInputField.SetTextWithoutNotify(volume.ToString());
            _effectVolumeSlider.SetValueWithoutNotify(volume);
        }

        public void NotifyMusicVolumeChanged(int volume)
        {
            _musicVolumeInputField.SetTextWithoutNotify(volume.ToString());
            _musicVolumeSlider.SetValueWithoutNotify(volume);
        }

        public void NotifyPianoVolumeChanged(int volume)
        {
            _pianoVolumeInputField.SetTextWithoutNotify(volume.ToString());
            _pianoVolumeSlider.SetValueWithoutNotify(volume);
        }

        public void NotifySuddenPlusRangeChanged(int value)
        {
            _suddenPlusInputField.SetTextWithoutNotify(value.ToString());
            _suddenPlusSlider.SetValueWithoutNotify(value);
        }

        public void NotifyShowIndicatorChanged(bool value) => _showIndicatorToggle.SetIsOnWithoutNotify(value);

        public void NotifyHorizontalGridSnapChanged(bool value) => _horizontalGridSnapToggle.SetIsOnWithoutNotify(value);

        public void NotifyVerticalGridSnapChanged(bool value) => _verticalGridSnapToggle.SetIsOnWithoutNotify(value);

        #endregion
    }
}