using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Project;
using Deenote.Project.Models;
using Deenote.Utilities.Robustness;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed class EditorPropertiesWindow : MonoBehaviour
    {
        [SerializeField] Window _window;

        public Window Window => _window;

        [Header("Notify")]
        [SerializeField] ProjectManager _projectManager;
        [SerializeField] GameStageController _stage;
        [SerializeField] EditorController _editor;

        [Header("UI")]
        [Header("Player")]
        [SerializeField] Button _playerGroupButton;
        [SerializeField] GameObject _playerGroupGameObject;

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
        [SerializeField] Button _placementGroupButton;
        [SerializeField] GameObject _placementGroupGameObject;

        [SerializeField] Toggle _showIndicatorToggle;

        [SerializeField] TMP_InputField _verticalGridInputField;
        [SerializeField] Toggle _verticalGridSnapToggle;
        [SerializeField] TMP_InputField _horizontalGridInputField;
        [SerializeField] Toggle _horizontalGridSnapToggle;

        [SerializeField] Button _cubicCurveButton;
        [SerializeField] Button _linearCurveButton;
        [SerializeField] Button _disableCurveButton;
        [SerializeField] TMP_InputField _fillByAmountInputField;
        [SerializeField] Button _fillByAmountButton;
        [SerializeField] Button _fillByGridButton;
        [SerializeField] GameObject _curveFillGroupGameObject;

        [Header("BPM")]
        [SerializeField] Button _bpmGroupButton;
        [SerializeField] GameObject _bpmGroupGameObject;

        [SerializeField] TMP_InputField _bpmStartTimeInputField;
        [SerializeField] TMP_InputField _bpmEndTimeInputField;
        [SerializeField] TMP_InputField _bpmInputField;
        [SerializeField] Button _bpmFillButton;

        private void Awake()
        {
            // Player
            _playerGroupButton.onClick.AddListener(() =>
                _playerGroupGameObject.SetActive(!_playerGroupGameObject.activeSelf));

            _noteSpeedDecButton.onClick.AddListener(() => _stage.NoteSpeed--);
            _noteSpeedIncButton.onClick.AddListener(() => _stage.NoteSpeed++);

            _musicSpeedDecButton.onClick.AddListener(() => _stage.MusicSpeed--);
            _musicSpeedIncButton.onClick.AddListener(() => _stage.MusicSpeed++);
            _musicSpeedInputField.onEndEdit.AddListener(OnMusicSpeedChanged);
            _musicSpeedHalfButton.onClick.AddListener(() => _stage.MusicSpeed = 5);
            _musicSpeedResetButton.onClick.AddListener(() => _stage.MusicSpeed = 10);

            _effectVolumeInputField.onEndEdit.AddListener(OnEffectVolumeChanged);
            _effectVolumeSlider.onValueChanged.AddListener(OnEffectVolumeChanged);
            _musicVolumeInputField.onEndEdit.AddListener(OnMusicVolumeChanged);
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _pianoVolumeInputField.onEndEdit.AddListener(OnPianoVolumeChanged);
            _pianoVolumeSlider.onValueChanged.AddListener(OnPianoVolumeChanged);

            _showLinksToggle.onValueChanged.AddListener(OnShowLinksChanged);

            _suddenPlusInputField.onEndEdit.AddListener(OnSuddenPlusChanged);
            _suddenPlusSlider.onValueChanged.AddListener(OnSuddenPlusChanged);

            // Placement
            _placementGroupButton.onClick.AddListener(() =>
                _placementGroupGameObject.SetActive(!_placementGroupGameObject.activeSelf));

            _showIndicatorToggle.onValueChanged.AddListener(OnShowIndicatorChanged);

            _verticalGridInputField.onEndEdit.AddListener(OnVerticalGridCountChanged);
            _verticalGridSnapToggle.onValueChanged.AddListener(OnVerticalGridSnapChanged);
            _horizontalGridInputField.onEndEdit.AddListener(OnHorizontalGridCountChanged);
            _horizontalGridSnapToggle.onValueChanged.AddListener(OnHorizontalGridSnapChanged);

            _cubicCurveButton.onClick.AddListener(OnInitializeCubicCurve);
            _linearCurveButton.onClick.AddListener(OnInitializeLinearCurve);
            _disableCurveButton.onClick.AddListener(OnDisableCurve);
            _fillByAmountInputField.onEndEdit.AddListener(OnFillAmountChanged);
            _fillByAmountButton.onClick.AddListener(OnFillAmountedNotesToCurve);
            // TODO
            //_fillByGridButton.onClick.AddListener(null);

            // Bpm
            _bpmGroupButton.onClick.AddListener(() => _bpmGroupGameObject.SetActive(!_bpmGroupGameObject.activeSelf));
            _bpmStartTimeInputField.onEndEdit.AddListener(OnBpmStartTimeChanged);
            _bpmEndTimeInputField.onEndEdit.AddListener(OnBpmEndTimeChanged);
            _bpmInputField.onEndEdit.AddListener(OnBpmChanged);
            _bpmFillButton.onClick.AddListener(OnBpmFill);

            _window.SetOnIsActivatedChanged(isActivated =>
            {
                if (isActivated) OnWindowActivated();
            });
        }

        #region Player Events

        // Invoke events to change values in GameStageController or somewhere,
        // GameStageController will call auto callbacks

        private void OnMusicSpeedChanged(string text)
        {
            if (!float.TryParse(text, out var val)) {
                NotifyMusicSpeedChanged(_stage.MusicSpeed);
                return;
            }

            int intVal = Mathf.RoundToInt(val * 10);
            _stage.MusicSpeed = Mathf.Clamp(intVal, 1, 30);
        }

        private void OnEffectVolumeChanged(string text)
        {
            if (!int.TryParse(text, out var val)) {
                NotifyEffectVolumeChanged(_stage.EffectVolume);
                return;
            }
            _stage.EffectVolume = Mathf.Clamp(val, 0, 100);
        }

        private void OnEffectVolumeChanged(float value) => _stage.EffectVolume = Mathf.Clamp((int)value, 0, 100);

        private void OnMusicVolumeChanged(string text)
        {
            if (!int.TryParse(text, out var val)) {
                NotifyMusicVolumeChanged(_stage.MusicVolume);
                return;
            }
            _stage.MusicVolume = Mathf.Clamp(val, 0, 100);
        }

        private void OnMusicVolumeChanged(float value) => _stage.MusicVolume = Mathf.Clamp((int)value, 0, 100);

        private void OnPianoVolumeChanged(string text)
        {
            if (!int.TryParse(text, out var val)) {
                NotifyPianoVolumeChanged(_stage.PianoVolume);
                return;
            }
            _stage.PianoVolume = Mathf.Clamp(val, 0, 100);
        }

        private void OnPianoVolumeChanged(float value) => _stage.PianoVolume = Mathf.Clamp((int)value, 0, 100);

        private void OnShowLinksChanged(bool value) => _stage.IsShowLinkLines = value;

        private void OnSuddenPlusChanged(string text)
        {
            if (!int.TryParse(text, out var val)) {
                NotifySuddenPlusRangeChanged(_stage.SuddenPlusRange);
                return;
            }
            _stage.SuddenPlusRange = Mathf.Clamp(val, 0, 100);
        }

        private void OnSuddenPlusChanged(float value) => _stage.SuddenPlusRange = Mathf.Clamp((int)value, 0, 100);

        #endregion

        #region Grid Events

        private int _fillAmount;

        private void OnShowIndicatorChanged(bool value) => _editor.IsNoteIndicatorOn = value;

        private void OnVerticalGridCountChanged(string value)
        {
            if (int.TryParse(value, out var count))
                _stage.Grids.VerticalGridCount = count;
            else
                NotifyVerticalGridCountChanged(_stage.Grids.VerticalGridCount);
        }

        private void OnVerticalGridSnapChanged(bool value) => _editor.SnapToPositionGrid = value;

        private void OnHorizontalGridCountChanged(string value)
        {
            if (int.TryParse(value, out var count))
                _stage.Grids.TimeGridSubBeatCount = count;
            else
                NotifyTimeGridSubBeatCountChanged(_stage.Grids.TimeGridSubBeatCount);

        }

        private void OnHorizontalGridSnapChanged(bool value) => _editor.SnapToTimeGrid = value;

        private void OnInitializeCubicCurve()
        {
            _stage.Grids.InitializeCurve(_editor.SelectedNotes, GridController.CurveKind.Cubic);
            _editor.NotifyCurveGeneratedWithSelectedNotes();
        }

        private void OnInitializeLinearCurve()
        {
            _stage.Grids.InitializeCurve(_editor.SelectedNotes, GridController.CurveKind.Linear);
            _editor.NotifyCurveGeneratedWithSelectedNotes();
        }

        private void OnDisableCurve()
        {
            _stage.Grids.HideCurve();
        }

        private void OnFillAmountChanged(string value)
        {
            if (int.TryParse(value, out var fa)) {
                if (fa < 0) {
                    _fillAmount = 0;
                    _fillByAmountInputField.SetTextWithoutNotify("0");
                }
                _fillAmount = fa;
            }
        }

        private void OnFillAmountedNotesToCurve()
        {
            _editor.AddNotesSnappingToCurve(_fillAmount);
        }

        #endregion

        #region Bpm Events

        private float _bpmStartTime;
        private float _bpmEndTime;
        private float _bpm;

        private void OnBpmStartTimeChanged(string value)
        {
            Debug.Log($"StartTime: {value}");
            if (float.TryParse(value, out var time))
                _bpmStartTime = Mathf.Max(time, 0f);

            _bpmStartTimeInputField.SetTextWithoutNotify(_bpmStartTime.ToString("F3"));
        }

        private void OnBpmEndTimeChanged(string value)
        {
            Debug.Log($"EndTime: {value}");
            if (float.TryParse(value, out var time))
                _bpmEndTime = Mathf.Min(time, _stage.MusicLength);

            _bpmEndTimeInputField.SetTextWithoutNotify(_bpmEndTime.ToString("F3"));
        }

        private void OnBpmChanged(string value)
        {
            if (float.TryParse(value, out var bpm))
                _bpm = Mathf.Clamp(0f, bpm, MainSystem.Args.MaxBpm);

            _bpmInputField.SetTextWithoutNotify(_bpm.ToString("F3"));
        }

        private void OnBpmFill()
        {
            _editor.InsertTempo(new Tempo(_bpm, _bpmStartTime), _bpmEndTime);
        }

        #endregion

        public void OnWindowActivated()
        {
            NotifyNoteSelectionChanged(_editor.SelectedNotes);
            NotifyNoteSpeedChanged(_stage.NoteSpeed);
            NotifyMusicSpeedChanged(_stage.MusicSpeed);
            NotifyEffectVolumeChanged(_stage.EffectVolume);
            NotifyMusicVolumeChanged(_stage.MusicVolume);
            NotifyPianoVolumeChanged(_stage.PianoVolume);
            NotifyIsShowLinksChanged(_stage.IsShowLinkLines);
            NotifySuddenPlusRangeChanged(_stage.SuddenPlusRange);

            NotifyShowIndicatorChanged(_editor.IsNoteIndicatorOn);
            NotifyVerticalGridCountChanged(_stage.Grids.VerticalGridCount);
            NotifyVerticalGridSnapChanged(_editor.SnapToPositionGrid);
            NotifyTimeGridSubBeatCountChanged(_stage.Grids.TimeGridSubBeatCount);
            NotifyTimeGridSnapChanged(_editor.SnapToTimeGrid);
            NotifyCurveOn(_stage.Grids.IsCurveOn);
        }

        #region Player Notify

        public void NotifyNoteSpeedChanged(int speed)
        {
            if (!_window.IsActivated)
                return;

            _noteSpeedText.text = speed % 2 == 0
                ? $"{speed / 2}.0"
                : $"{speed / 2}.5";
            _noteSpeedDecButton.gameObject.SetActive(speed > 1);
            _noteSpeedIncButton.gameObject.SetActive(speed < 19);
        }

        public void NotifyMusicSpeedChanged(int speed)
        {
            if (!_window.IsActivated)
                return;

            _musicSpeedInputField.SetTextWithoutNotify((speed / 10f).ToString("F1"));
            _musicSpeedDecButton.gameObject.SetActive(speed > 1);
            _musicSpeedIncButton.gameObject.SetActive(speed < 30);
        }

        public void NotifyEffectVolumeChanged(int volume)
        {
            if (!_window.IsActivated)
                return;

            _effectVolumeInputField.SetTextWithoutNotify(volume.ToString());
            _effectVolumeSlider.SetValueWithoutNotify(volume);
        }

        public void NotifyMusicVolumeChanged(int volume)
        {
            if (!_window.IsActivated)
                return;

            _musicVolumeInputField.SetTextWithoutNotify(volume.ToString());
            _musicVolumeSlider.SetValueWithoutNotify(volume);
        }

        public void NotifyPianoVolumeChanged(int volume)
        {
            if (!_window.IsActivated)
                return;

            _pianoVolumeInputField.SetTextWithoutNotify(volume.ToString());
            _pianoVolumeSlider.SetValueWithoutNotify(volume);
        }

        public void NotifyIsShowLinksChanged(bool value)
        {
            if (!_window.IsActivated)
                return;

            _showLinksToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifySuddenPlusRangeChanged(int value)
        {
            if (!_window.IsActivated)
                return;

            _suddenPlusInputField.SetTextWithoutNotify(value.ToString());
            _suddenPlusSlider.SetValueWithoutNotify(value);
        }

        #endregion

        #region Placement Notify

        public void NotifyShowIndicatorChanged(bool value)
        {
            if (!_window.IsActivated)
                return;

            _showIndicatorToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifyVerticalGridCountChanged(int count)
        {
            if (!_window.IsActivated)
                return;

            _verticalGridInputField.SetTextWithoutNotify(count.ToString());
        }

        public void NotifyVerticalGridSnapChanged(bool value)
        {
            if (!_window.IsActivated)
                return;
            _verticalGridSnapToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifyTimeGridSubBeatCountChanged(int count)
        {
            if (!_window.IsActivated)
                return;
            _horizontalGridInputField.SetTextWithoutNotify(count.ToString());
        }

        public void NotifyTimeGridSnapChanged(bool value)
        {
            if (!_window.IsActivated)
                return;
            _horizontalGridSnapToggle.SetIsOnWithoutNotify(value);
        }

        public void NotifyCurveOn(bool isOn)
        {
            if (!_window.IsActivated)
                return;

            _disableCurveButton.interactable = isOn;
            _curveFillGroupGameObject.SetActive(isOn);
        }

        #endregion

        #region Bpm Notify

        public void NotifyNoteSelectionChanged(ReadOnlySpan<NoteModel> selectedNotes)
        {
            if (!_window.IsActivated)
                return;

            if (selectedNotes.IsEmpty)
                return;

            float start = selectedNotes[0].Data.Time;
            _bpmStartTime = start;
            _bpmStartTimeInputField.text = start.ToString("F3");
            float end = selectedNotes[^1].Data.Time;
            _bpmEndTime = end;
            _bpmEndTimeInputField.text = end.ToString("F3");
            if (selectedNotes.Length < 2)
                return;

            float interval = end - start;
            if (interval < MainSystem.Args.MinBeatLineInterval)
                return;

            var bpm = 60f / interval;
            _bpm = bpm;
            _bpmInputField.text = bpm.ToString("F3");
        }

        #endregion
    }
}