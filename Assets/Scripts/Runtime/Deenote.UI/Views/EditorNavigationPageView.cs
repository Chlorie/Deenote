#nullable enable

using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Entities;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.Library.Numerics;
using Deenote.UIFramework.Controls;
using System;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class EditorNavigationPageView : MonoBehaviour
    {
        [SerializeField] TextBox _highlightNoteSpeedInput = default!;
        [SerializeField] ToggleButton _applySpeedDiffToggle = default!;
        [SerializeField] ToggleButton _filterNoteSpeedToggle = default!;

        [SerializeField] NumericStepper _musicSpeedNumericStepper = default!;
        [SerializeField] TextBox _horizontalGridCountInput = default!;
        [SerializeField] TextBox _verticalGridCountInput = default!;
        [SerializeField] Button _horizontalGridCountDecButton = default!;
        [SerializeField] Button _horizontalGridCountIncButton = default!;
        [SerializeField] Button _verticalGridCountDecButton = default!;
        [SerializeField] Button _verticalGridCountIncButton = default!;
        [SerializeField] ToggleButton _horizontalGridSnapToggle = default!;
        [SerializeField] ToggleButton _horizontalGridVisibleToggle = default!;
        [SerializeField] ToggleButton _verticalGridSnapToggle = default!;
        [SerializeField] ToggleButton _verticalGridVisibleToggle = default!;

        //[SerializeField] RadioButtonGroup _curveKindRadioGroup = default!;
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
        private static readonly int[] _predefinedHorizontalGridCount = { 1, 2, 3, 4, 6, 8, 12, 16, 24, 32, 48, 64 };

        private GridsManager.CurveKind _currentCurveKind;
        private int _curveFillAmount;
        private bool _curveAutoApplySize;
        private bool _curveAutoApplySpeed;
        private float _bpmStartTime;
        private float _bpmEndTime;
        private float _bpmValue;

        private void Start()
        {
            RegisterNotifications();
            _linearCurveRadio.SetChecked();
        }

        internal void RegisterNotifications()
        {
            // Stage
            {
                _highlightNoteSpeedInput.EditSubmitted += text =>
                {
                    if (float.TryParse(text, out var value))
                        MainSystem.GamePlayManager.HighlightedNoteSpeed = value;
                    else
                        _highlightNoteSpeedInput.SetValueWithoutNotify(MainSystem.GamePlayManager.HighlightedNoteSpeed.ToString("F2"));
                };
                MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                    GamePlayManager.NotificationFlag.HighlightedNoteSpeed,
                    manager => _highlightNoteSpeedInput.SetValueWithoutNotify(MainSystem.GamePlayManager.HighlightedNoteSpeed.ToString("F2")));
                _applySpeedDiffToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.IsApplySpeedDifference = val;
                MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                    GamePlayManager.NotificationFlag.IsApplySpeedDifference,
                    manager => _applySpeedDiffToggle.SetIsCheckedWithoutNotify(manager.IsApplySpeedDifference));
                _filterNoteSpeedToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.IsFilterNoteSpeed = val;
                MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                    GamePlayManager.NotificationFlag.IsFilterNoteSpeed,
                    manager => _filterNoteSpeedToggle.SetIsCheckedWithoutNotify(manager.IsFilterNoteSpeed));

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
                void SyncHorizontal(GridsManager grids)
                {
                    var value = grids.TimeGridSubBeatCount;
                    _horizontalGridCountInput.SetValueWithoutNotify(value.ToString());
                    _horizontalGridCountDecButton.gameObject.SetActive(value > _predefinedHorizontalGridCount[0]);
                    _horizontalGridCountIncButton.gameObject.SetActive(value < _predefinedHorizontalGridCount[^1]);
                }

                void SyncVertical(GridsManager grids)
                {
                    var value = grids.PositionGridCount;
                    _verticalGridCountInput.SetValueWithoutNotify(value.ToString());
                }

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

                _horizontalGridCountDecButton.Clicked += () =>
                {
                    var index = _predefinedHorizontalGridCount.AsSpan().FindLowerBoundIndex(MainSystem.GamePlayManager.Grids.TimeGridSubBeatCount);
                    if (index == 0) return;
                    MainSystem.GamePlayManager.Grids.TimeGridSubBeatCount = _predefinedHorizontalGridCount[index - 1];
                };
                _horizontalGridCountIncButton.Clicked += () =>
                {
                    var index = _predefinedHorizontalGridCount.AsSpan().FindUpperBoundIndex(MainSystem.GamePlayManager.Grids.TimeGridSubBeatCount);
                    if (index >= _predefinedHorizontalGridCount.Length) return;
                    MainSystem.GamePlayManager.Grids.TimeGridSubBeatCount = _predefinedHorizontalGridCount[index];
                };

                _horizontalGridSnapToggle.IsCheckedChanged += val => MainSystem.StageChartEditor.Placer.SnapToTimeGrid = val;
                MainSystem.StageChartEditor.Placer.RegisterNotificationAndInvoke(
                    StageNotePlacer.NotificationFlag.SnapToTimeGrid,
                    placer => _horizontalGridSnapToggle.SetIsCheckedWithoutNotify(placer.SnapToTimeGrid));

                _horizontalGridVisibleToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.Grids.TimeGridVisible = val;
                MainSystem.GamePlayManager.Grids.RegisterNotificationAndInvoke(
                    GridsManager.NotificationFlag.TimeGridVisible,
                    grids => _horizontalGridVisibleToggle.SetIsCheckedWithoutNotify(grids.TimeGridVisible));

                _verticalGridSnapToggle.IsCheckedChanged += val => MainSystem.StageChartEditor.Placer.SnapToPositionGrid = val;
                MainSystem.StageChartEditor.Placer.RegisterNotificationAndInvoke(
                    StageNotePlacer.NotificationFlag.SnapToPositionGrid,
                    placer => _verticalGridSnapToggle.SetIsCheckedWithoutNotify(placer.SnapToPositionGrid));

                _verticalGridVisibleToggle.IsCheckedChanged += val => MainSystem.GamePlayManager.Grids.PositionGridVisible = val;
                MainSystem.GamePlayManager.Grids.RegisterNotificationAndInvoke(
                    GridsManager.NotificationFlag.PositionGridVisible,
                    grids => _verticalGridVisibleToggle.SetIsCheckedWithoutNotify(grids.PositionGridVisible));
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
                        if (MainSystem.GamePlayManager.Grids.IsCurveOn) {
                            _fillCurveButton.IsInteractable = _curveFillAmount > 0;
                        }
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

                MainSystem.StageChartEditor.Selector.SelectedNotesChanged += _OnSelectedNotesChanged;
                _OnSelectedNotesChanged(MainSystem.StageChartEditor.Selector);
                MainSystem.GamePlayManager.Grids.RegisterNotificationAndInvoke(
                    GridsManager.NotificationFlag.IsCurveOnChanged,
                    grids =>
                    {
                        if (grids.IsCurveOn) {
                            _fillCurveButton.IsInteractable = _curveFillAmount > 0;
                        }
                        else {
                            _fillCurveButton.IsInteractable = false;
                        }
                    });

                void _OnSelectedNotesChanged(StageNoteSelector selector)
                {
                    var generatable = selector.SelectedNotes.Length >= 2;
                    _generateCurveButton.IsInteractable = generatable;
                    var appliable = selector.SelectedNotes.Length > 2;
                    _curveApplySizeButton.IsInteractable = appliable;
                    _curveApplySpeedButton.IsInteractable = appliable;
                }
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
                    if (float.TryParse(val, out var fval)) {
                        if (MainSystem.ProjectManager.IsProjectLoaded())
                            _bpmEndTime = Mathf.Min(fval, MainSystem.GamePlayManager.MusicPlayer.ClipLength);
                        else
                            _bpmEndTime = fval;
                    }
                    SyncFloatInput(_bpmEndTimeInput, _bpmEndTime);
                };
                _bpmValueInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var fval))
                        _bpmValue = fval;
                    SyncFloatInput(_bpmValueInput, _bpmValue);
                };
                _bpmFillButton.Clicked += () =>
                {
                    MainSystem.ProjectManager.AssertProjectLoaded();
                    var endTime = Mathf.Min(_bpmEndTime, MainSystem.GamePlayManager.MusicPlayer.ClipLength);
                    MainSystem.StageChartEditor.InsertTempo(new Entities.TempoRange(_bpmValue, _bpmStartTime, _bpmEndTime));
                };

                MainSystem.StageChartEditor.Selector.SelectedNotesChanged += _OnSelectedNotesChanaged;
                _OnSelectedNotesChanaged(MainSystem.StageChartEditor.Selector);

                MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                    GamePlayManager.NotificationFlag.CurrentChart,
                    manager =>
                    {
                        var chartLoaded = manager.IsChartLoaded();
                        _bpmFillButton.IsInteractable = chartLoaded;
                    });

                void _OnSelectedNotesChanaged(StageNoteSelector selector)
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
                }
            }
        }
    }
}