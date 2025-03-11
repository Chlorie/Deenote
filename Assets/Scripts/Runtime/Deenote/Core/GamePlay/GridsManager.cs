#nullable enable

using Deenote.Core.Editing;
using Deenote.Core.GamePlay.Audio;
using Deenote.Core.GameStage;
using Deenote.Core.Project;
using Deenote.Entities;
using Deenote.Library.Components;
using System;

namespace Deenote.Core.GamePlay
{
    public sealed partial class GridsManager : FlagNotifiable<GridsManager, GridsManager.NotificationFlag>, IDisposable
    {
        private readonly GamePlayManager _game;
        private readonly StageChartEditor _editor;

        public GridsManager(GamePlayManager manager, StageChartEditor editor)
        {
            _game = manager;
            _editor = editor;
            MainSystem.ProjectManager.RegisterNotification(
                ProjectManager.NotificationFlag.CurrentProject,
                _OnCurrentProjectChanged);
            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.SuddenPlus,
                _OnSuddenPlusChanged);
            _editor.Placer.RegisterNotification(
                StageNotePlacer.NotificationFlag.PlacingNoteSpeed,
                _OnPlacingNoteSpeedChanged);
            _editor.RegisterNotification(
                StageChartEditor.NotificationFlag.ProjectTempo,
                _OnProjectTempoChanged);
            _game.MusicPlayer.TimeChanged += _OnStageTimeChanged;
            _game.StageLoaded += args =>
            {
                args.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
            };

            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Add("stage/grids/pos_grid_count", PositionGridCount);
                configs.Add("stage/grids/pos_grid_visible", PositionGridVisible);
                configs.Add("stage/grids/time_grid_count", TimeGridSubBeatCount);
                configs.Add("stage/grids/time_grid_visible", TimeGridVisible);
            };

            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                PositionGridCount = configs.GetInt32("stage/grids/pos_grid_count", 9);
                PositionGridVisible = configs.GetBoolean("stage/grids/pos_grid_visible", true);
                TimeGridSubBeatCount = configs.GetInt32("stage/grids/time_grid_count", 1);
                TimeGridVisible = configs.GetBoolean("stage/grids/time_grid_visible", true);
            };
        }

        public void Dispose()
        {
            _game.UnregisterNotification(
                GamePlayManager.NotificationFlag.SuddenPlus,
                _OnSuddenPlusChanged);
            _game.MusicPlayer.TimeChanged -= _OnStageTimeChanged;

            if (_game.IsStageLoaded())
                _game.Stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        #region Registrations

        private void _OnCurrentProjectChanged(ProjectManager manager) => UpdateTimeGrids();
        private void _OnSuddenPlusChanged(GamePlayManager manager) => UpdatePositionGrids();
        private void _OnPlacingNoteSpeedChanged(StageNotePlacer _)
        {
            UpdateTimeGrids();
            UpdateCurveLines();
        }
        private void _OnProjectTempoChanged(StageChartEditor _) => UpdateTimeGrids();
        private void _OnStageTimeChanged(GameMusicPlayer.TimeChangedEventArgs args)
        {
            UpdateTimeGrids();
            UpdateCurveLines();
        }
        private void _OnPerspectiveLineCollecting(PerspectiveLinesRenderer.LineCollector collector)
        {
            SubmitTimeGridRender(collector);
            SubmitPositionGridsRender(collector);
            SubmitCurveRender(collector);
        }

        #endregion

        public NoteCoord Quantize(NoteCoord coord, bool snapPosition, bool snapTime)
        {
            float snappedTime = snapTime ? GetNearestTimeGridTime(coord.Time) ?? coord.Time : coord.Time;
            float snappedPosition = snapPosition
                ? GetCurveTransformedPosition(snappedTime) ?? GetNearestPositionGridPosition(coord.Position)
                ?? coord.Position : coord.Position;
            return NoteCoord.ClampPosition(snappedPosition, snappedTime);
        }

        public enum NotificationFlag
        {
            TimeGridSubBeatCountChanged,
            TimeGridVisible,
            PositionGridChanged,
            PositionGridVisible,
            IsCurveOnChanged,
        }
    }
}