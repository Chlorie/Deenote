#nullable enable

using Deenote.GameStage;
using Deenote.Project.Models;
using Deenote.UI.ComponentModel;
using Deenote.UI.Controls;
using System;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class EditorPropertiesPanelView : MonoBehaviour
    {
        [SerializeField] KVNumericStepperProperty _musicSpeedProperty = default!;
        [SerializeField] KVBooleanProperty _showIndicatorProperty = default!;

        [SerializeField] KVInputProperty _horizontalGridCountProperty = default!;
        [SerializeField] KVInputProperty _verticalGridCountProperty = default!;
        [SerializeField] KVBooleanProperty _horizontalGridSnapProperty = default!;
        [SerializeField] KVBooleanProperty _verticalGridSnapProperty = default!;

        [SerializeField] ToggleList _curveKindList = default!;
        [SerializeField] Button _curveGenerateButton = default!;
        [SerializeField] Button _curveDisableButton = default!;
        [SerializeField] InputField _curveFillAmountInput = default!;
        [SerializeField] Button _curveFillButton = default!;
        [SerializeField] KVButtonProperty _curveSizeApplyProperty = default!;
        [SerializeField] KVBooleanProperty _curveSizeAutoApplyProperty = default!;
        [SerializeField] KVButtonProperty _curveSpeedApplyProperty = default!;
        [SerializeField] KVBooleanProperty _curveSpeedAutoApplyProperty = default!;

        [SerializeField] KVRangeInputProperty _bpmTimeRangeInput = default!;
        [SerializeField] InputField _bpmInput = default!;
        [SerializeField] Button _bpmFillButton = default!;

        private GridController.CurveKind _curveKind;
        private int _curveFillAmount;
        private bool _curveAutoApplySize;
        private bool _curveAutoApplySpeed;

        private float _bpmTimeStart;
        private float _bpmTimeEnd;
        private float _bpm;

        private void Start()
        {
            // Stage
            {
                // Music Speed

                _musicSpeedProperty.InputParser = static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 2) : null;
                _musicSpeedProperty.DisplayTextSelector = static ival => ival % 2 == 0 ? $"{ival / 2}.0" : $"{ival / 2}.5";
                _musicSpeedProperty.OnValueChanged.AddListener(val => MainSystem.GameStage.MusicSpeed = val);
                MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GameStageController.NotifyProperty.MusicSpeed,
                    stage => _musicSpeedProperty.SetValueWithoutNotify(stage.MusicSpeed));

                // Show Indicator

                _showIndicatorProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.Editor.IsNoteIndicatorOn = val ?? false);
                MainSystem.Editor.RegisterPropertyChangeNotificationAndInvoke(
                    Edit.EditorController.NotifyProperty.IsIndicatorOn,
                    editor => _showIndicatorProperty.CheckBox.SetValueWithoutNotify(editor.IsNoteIndicatorOn));
            }

            // Grids
            {
                // Time Grid Count

                _horizontalGridCountProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (int.TryParse(val, out var ival))
                        MainSystem.GameStage.Grids.TimeGridSubBeatCount = ival;
                    else
                        _horizontalGridCountProperty.InputField.SetValueWithoutNotify(MainSystem.GameStage.Grids.TimeGridSubBeatCount.ToString());
                });
                MainSystem.GameStage.Grids.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GridController.NotifyProperty.TimeGridSubBeatCount,
                    grids => _horizontalGridCountProperty.InputField.SetValueWithoutNotify(grids.TimeGridSubBeatCount.ToString()));
            
                // Position Grid count

                _verticalGridCountProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (int.TryParse(val, out var ival))
                        MainSystem.GameStage.Grids.VerticalGridCount = ival;
                    else
                        _verticalGridCountProperty.InputField.SetValueWithoutNotify(MainSystem.GameStage.Grids.VerticalGridCount.ToString());
                });
                MainSystem.GameStage.Grids.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GridController.NotifyProperty.VerticalGridCount,
                    grids => _verticalGridCountProperty.InputField.SetValueWithoutNotify(grids.VerticalGridCount.ToString()));
               
                // Time snap

                _horizontalGridSnapProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.Editor.SnapToTimeGrid = val ?? false);
                MainSystem.Editor.RegisterPropertyChangeNotificationAndInvoke(
                    Edit.EditorController.NotifyProperty.SnapToTimeGrid,
                    editor => _horizontalGridSnapProperty.CheckBox.SetValueWithoutNotify(editor.SnapToTimeGrid));
           
                // Position snap

                _verticalGridSnapProperty.CheckBox.OnValueChanged.AddListener(val => MainSystem.Editor.SnapToPositionGrid = val ?? false);
                MainSystem.Editor.RegisterPropertyChangeNotificationAndInvoke(
                    Edit.EditorController.NotifyProperty.SnapToPositionGrid,
                    editor => _verticalGridSnapProperty.CheckBox.SetValueWithoutNotify(editor.SnapToPositionGrid));
            }

            // Curves
            {
                _curveKindList.OnSelectedIndexChanged.AddListener(val =>
                {
                    switch (val) {
                        case 0: _curveKind = GridController.CurveKind.Linear; break;
                        case 1: _curveKind = GridController.CurveKind.Cubic; break;
                        default:
                            break;
                    }
                });
                _curveGenerateButton.OnClick.AddListener(() =>
                {
                    MainSystem.GameStage.Grids.InitializeCurve(MainSystem.Editor.SelectedNotes, _curveKind);
                    MainSystem.Editor.NotifyCurveGeneratedWithSelectedNotes();
                });
                _curveDisableButton.OnClick.AddListener(MainSystem.GameStage.Grids.HideCurve);
                MainSystem.GameStage.Grids.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GridController.NotifyProperty.IsCurveOn,
                    grids => _curveDisableButton.IsInteractable = grids.IsCurveOn);
          
                _curveFillAmountInput.OnEndEdit.AddListener(val =>
                {
                    if (int.TryParse(val, out var ival)) {
                        if (ival < 0) {
                            _curveFillAmount = 0;
                            _curveFillAmountInput.SetValueWithoutNotify(0.ToString());
                        }
                        else {
                            _curveFillAmount = ival;
                        }
                    }
                    _curveFillAmountInput.SetValueWithoutNotify(_curveFillAmount.ToString());
                });
                _curveFillButton.OnClick.AddListener(() =>
                {
                    Span<GridController.CurveApplyProperty> applyProps = stackalloc GridController.CurveApplyProperty[2];
                    int index = 0;
                    if (_curveAutoApplySize)
                        applyProps[index++] = GridController.CurveApplyProperty.Size;
                    if (_curveAutoApplySpeed)
                        applyProps[index++] = GridController.CurveApplyProperty.Speed;

                    MainSystem.Editor.AddNotesSnappingToCurve(_curveFillAmount, applyProps[..index]);
                });
                _curveSizeApplyProperty.Button.OnClick.AddListener(
                    () => MainSystem.Editor.ApplySelectedNotesWithCurveTransform(GridController.CurveApplyProperty.Size));
                _curveSizeAutoApplyProperty.CheckBox.OnValueChanged.AddListener(val => _curveAutoApplySize = val ?? false);

                _curveSpeedApplyProperty.Button.OnClick.AddListener(
                    () => MainSystem.Editor.ApplySelectedNotesWithCurveTransform(GridController.CurveApplyProperty.Speed));
                _curveSpeedAutoApplyProperty.CheckBox.OnValueChanged.AddListener(val => _curveAutoApplySpeed = val ?? false);

                MainSystem.Editor.RegisterPropertyChangeNotificationAndInvoke(
                    Edit.EditorController.NotifyProperty.SelectedNotes,
                    editor =>
                    {
                        bool val = editor.SelectedNotes.Length > 1;
                        _curveFillButton.IsInteractable = val;
                        _curveSizeApplyProperty.Button.IsInteractable = val;
                        _curveSpeedApplyProperty.Button.IsInteractable = val;
                    });
            }

            // BPM
            {
                _bpmTimeRangeInput.LowerInputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out float fval))
                        _bpmTimeStart = fval;
                    else
                        SyncInputField(_bpmTimeRangeInput.LowerInputField, _bpmTimeStart);
                });
                _bpmTimeRangeInput.UpperInputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out float fval))
                        _bpmTimeEnd = fval;
                    else
                        SyncInputField(_bpmTimeRangeInput.UpperInputField, _bpmTimeEnd);
                });
                _bpmInput.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out var fval))
                        _bpm = fval;
                    else
                        SyncInputField(_bpmInput, _bpm);
                });
                _bpmFillButton.OnClick.AddListener(() => MainSystem.Editor.InsertTempo(new Tempo(_bpm, _bpmTimeStart), _bpmTimeEnd));

                MainSystem.Editor.RegisterPropertyChangeNotificationAndInvoke(
                    Edit.EditorController.NotifyProperty.SelectedNotes,
                    editor =>
                    {
                        var selectedNotes = editor.SelectedNotes;
                        if (selectedNotes.IsEmpty)
                            return;

                        float start = selectedNotes[0].Data.Time;
                        _bpmTimeStart = start;
                        SyncInputField(_bpmTimeRangeInput.LowerInputField, _bpmTimeStart);

                        float end = selectedNotes[^1].Data.Time;
                        _bpmTimeEnd = end;
                        SyncInputField(_bpmTimeRangeInput.UpperInputField, _bpmTimeEnd);

                        if (selectedNotes.Length <= 1)
                            return;
                        float interval = end - start;
                        if (interval < MainSystem.Args.MinBeatLineInterval)
                            return;
                        var bpm = 60f / interval;
                        _bpm = bpm;
                        SyncInputField(_bpmInput, _bpm);
                    });

                static void SyncInputField(InputField inputField, float value)
                {
                    inputField.SetValueWithoutNotify(value.ToString("F3"));
                }
            }
        }
    }
}