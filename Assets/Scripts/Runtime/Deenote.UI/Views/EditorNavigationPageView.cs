#nullable enable

using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Library.Components;
using Deenote.UIFramework.Controls;
using System;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class EditorNavigationPageView : MonoBehaviour
    {
        [SerializeField] NumericStepper _musicSpeedNumericStepper = default!;
        [SerializeField] TextBox _horizontalGridCountInput = default!;
        [SerializeField] TextBox _verticalGridCountInput = default!;
        [SerializeField] ToggleSwitch _horizontalGridSnapToggle = default!;
        [SerializeField] ToggleSwitch _verticalGridSnapToggle = default!;

        [SerializeField] RadioButtonGroup _curveKindRadioGroup = default!;
        [SerializeField] RadioButton _linearCurveRadio = default!;
        [SerializeField] RadioButton _cubicCurveRadio = default!;
        [SerializeField] Button _generateCurveButton = default!;
        [SerializeField] Button _disableCurveButton = default!;
        [SerializeField] TextBox _fillCurveAmountInput = default!;
        [SerializeField] Button _fillCurveButton = default!;
        [SerializeField] ToggleSwitch _curveAutoApplySizeToggle = default!;
        [SerializeField] ToggleSwitch _curveAutoApplySpeedToggle = default!;
        [SerializeField] Button _curveApplySizeButton = default!;
        [SerializeField] Button _curveApplySpeedButton = default!;

        [SerializeField] TextBox _bpmStartTimeInput = default!;
        [SerializeField] TextBox _bpmEndTimeInput = default!;
        [SerializeField] TextBox _bpmValueInput = default!;
        [SerializeField] Button _bpmFillButton = default!;

        private const int MinCurveFillAmount = 0;
        private const int MaxCurveFillAmount = 256;

        private GridsManager.CurveKind _currentCurveKind;
        private int _curveFillAmount;
        private bool _curveAutoApplySize;
        private bool _curveAutoApplySpeed;
        private float _bpmStartTime;
        private float _bpmEndTime;
        private float _bpmValue;

        private void Start()
        {
            _linearCurveRadio.SetChecked();
        }

        private void Awake()
        {
            RegisterNotifications();
        }

        internal void RegisterNotifications()
        {
            // Stage
            {
                _musicSpeedNumericStepper.Initialize(GamePlayManager.MinMusicSpeed, GamePlayManager.MaxMusicSpeed);
                _musicSpeedNumericStepper.SetInputParser(static input => float.TryParse(input, out var val) ? Mathf.RoundToInt(val * 10f) : null);
                _musicSpeedNumericStepper.SetDisplayerTextSelector(static ival => $"{ival / 10}.{ival % 10}");
                _musicSpeedNumericStepper.ValueChanged += val => MainSystem.GamePlayManager.MusicSpeed = val;
                MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                    GamePlayManager.NotificationFlag.MusicSpeed,
                    stage => _musicSpeedNumericStepper.SetValueWithoutNotify(stage.MusicSpeed));
            }

            // Grids
            {
                void SyncHorizontal(GridsManager grids) => _horizontalGridCountInput.SetValueWithoutNotify(grids.TimeGridSubBeatCount.ToString());
                void SyncVertical(GridsManager grids) => _verticalGridCountInput.SetValueWithoutNotify(grids.PositionGridCount.ToString());

                _horizontalGridCountInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var ival))
                        MainSystem.GamePlayManager.Grids.TimeGridSubBeatCount = ival;
                    SyncHorizontal(MainSystem.GamePlayManager.Grids);
                };
                MainSystem.GamePlayManager.Grids.RegisterNotificationAndInvoke(
                    GridsManager.NotificationFlag.TimeGridSubBeatCountChanged,
                    SyncHorizontal);

                _verticalGridCountInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var ival))
                        MainSystem.GamePlayManager.Grids.PositionGridCount = ival;
                    SyncVertical(MainSystem.GamePlayManager.Grids);
                };
                MainSystem.GamePlayManager.Grids.RegisterNotificationAndInvoke(
                    GridsManager.NotificationFlag.PositionGridChanged,
                    SyncVertical);

                _horizontalGridSnapToggle.IsCheckedChanged += val => MainSystem.StageChartEditor.Placer.SnapToTimeGrid = val;
                MainSystem.StageChartEditor.Placer.RegisterNotificationAndInvoke(
                    StageNotePlacer.NotificationFlag.SnapToTimeGrid,
                    placer => _horizontalGridSnapToggle.SetIsCheckedWithoutNotify(placer.SnapToTimeGrid));

                _verticalGridSnapToggle.IsCheckedChanged += val => MainSystem.StageChartEditor.Placer.SnapToPositionGrid = val;
                MainSystem.StageChartEditor.Placer.RegisterNotificationAndInvoke(
                    StageNotePlacer.NotificationFlag.SnapToPositionGrid,
                    placer => _verticalGridSnapToggle.SetIsCheckedWithoutNotify(placer.SnapToPositionGrid));
            }

            // Curves
            {
                void SyncCurveFillAmount(GridsManager grids) => _fillCurveAmountInput.SetValueWithoutNotify(_curveFillAmount.ToString());

                _linearCurveRadio.Checked += () => _currentCurveKind = GridsManager.CurveKind.Linear;
                _cubicCurveRadio.Checked += () => _currentCurveKind = GridsManager.CurveKind.Cubic;
                _generateCurveButton.Clicked += () =>
                {
                    MainSystem.GamePlayManager.Grids.InitializeCurve(MainSystem.StageChartEditor.Selector.SelectedNotes, _currentCurveKind);
                    // Remove notes in between
                    MainSystem.StageChartEditor.RemoveNotes(MainSystem.StageChartEditor.Selector.SelectedNotes[1..^1]);
                };
                _disableCurveButton.Clicked += () => MainSystem.GamePlayManager.Grids.HideCurve();
                MainSystem.GamePlayManager.Grids.RegisterNotificationAndInvoke(
                    GridsManager.NotificationFlag.IsCurveOnChanged,
                    grids => _disableCurveButton.IsInteractable = grids.IsCurveOn);

                _fillCurveAmountInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var ival)) {
                        _curveFillAmount = Mathf.Clamp(ival, MinCurveFillAmount, MaxCurveFillAmount);
                    }
                    SyncCurveFillAmount(MainSystem.GamePlayManager.Grids);
                };
                _fillCurveButton.Clicked += () =>
                {
                    Span<GridsManager.CurveApplyProperty> applyProps = stackalloc GridsManager.CurveApplyProperty[2];
                    int index = 0;
                    if (_curveAutoApplySize)
                        applyProps[index++] = GridsManager.CurveApplyProperty.Size;
                    if (_curveAutoApplySpeed)
                        applyProps[index++] = GridsManager.CurveApplyProperty.Speed;

                    MainSystem.StageChartEditor.AddNotesSnappingToCurve(_curveFillAmount, applyProps[..index]);
                };
                _curveAutoApplySizeToggle.IsCheckedChanged += val => _curveAutoApplySize = val;
                _curveAutoApplySpeedToggle.IsCheckedChanged += val => _curveAutoApplySpeed = val;
                _curveApplySizeButton.Clicked += () => MainSystem.StageChartEditor.ApplySelectedNotesWithCurveTranform(GridsManager.CurveApplyProperty.Size);
                _curveApplySpeedButton.Clicked += () => MainSystem.StageChartEditor.ApplySelectedNotesWithCurveTranform(GridsManager.CurveApplyProperty.Speed);

                MainSystem.StageChartEditor.Selector.RegisterNotificationAndInvoke(
                    StageNoteSelector.NotificationFlag.SelectedNotesChanged,
                    selector =>
                    {
                        bool val = selector.SelectedNotes.Length > 1;
                        _fillCurveButton.IsInteractable = val;
                        _curveApplySizeButton.IsInteractable = val;
                        _curveApplySpeedButton.IsInteractable = val;
                    });
            }

            // BPM
            {
                void SyncFloatInput(TextBox input, float value) => input.SetValueWithoutNotify(value.ToString("F3"));

                _bpmStartTimeInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var fval))
                        _bpmStartTime = fval;
                    SyncFloatInput(_bpmStartTimeInput, _bpmStartTime);
                };
                _bpmEndTimeInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var fval))
                        _bpmEndTime = fval;
                    SyncFloatInput(_bpmEndTimeInput, _bpmEndTime);
                };
                _bpmValueInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var fval))
                        _bpmValue = fval;
                    SyncFloatInput(_bpmValueInput, _bpmValue);
                };
                _bpmFillButton.Clicked += () => MainSystem.StageChartEditor.InsertTempo(new Entities.TempoRange(_bpmValue, _bpmStartTime, _bpmEndTime));

                MainSystem.StageChartEditor.Selector.RegisterNotificationAndInvoke(
                    StageNoteSelector.NotificationFlag.SelectedNotesChanged,
                    selector =>
                    {
                        var selectedNotes = selector.SelectedNotes;
                        if (selectedNotes.IsEmpty)
                            return;

                        float start = selectedNotes[0].Time;
                        float end = selectedNotes[^1].Time;
                        _bpmStartTime = start;
                        _bpmEndTime = end;
                        SyncFloatInput(_bpmStartTimeInput, _bpmStartTime);
                        SyncFloatInput(_bpmEndTimeInput, _bpmEndTime);

                        if (selectedNotes.Length == 1)
                            return;

                        float interval = end - start;
                        if (interval < Tempo.MinBeatLineInterval)
                            return;
                        _bpmValue = 60f / interval;
                        SyncFloatInput(_bpmValueInput, _bpmValue);
                    });
            }
        }
    }
}