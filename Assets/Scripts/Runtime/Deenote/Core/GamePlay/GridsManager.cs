#nullable enable

using Deenote;
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

        public GridsManager(GamePlayManager manager)
        {
            _game = manager;
            MainSystem.ProjectManager.RegisterNotification(
                ProjectManager.NotificationFlag.ProjectTempos,
                _OnTemposChanged);
            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.SuddenPlus,
                _OnSuddenPlusChanged);
            _game.MusicPlayer.TimeChanged += _OnStageTimeChanged;
            _game.StageLoaded += args =>
            {
                args.Stage.PerspectiveLinesRenderer.LineCollecting += _OnPerspectiveLineCollecting;
            };
        }

        public void Dispose()
        {
            MainSystem.ProjectManager.UnregisterNotification(
                ProjectManager.NotificationFlag.ProjectTempos,
                _OnTemposChanged);
            _game.UnregisterNotification(
                GamePlayManager.NotificationFlag.SuddenPlus,
                _OnSuddenPlusChanged);
            _game.MusicPlayer.TimeChanged -= _OnStageTimeChanged;

            if (_game.IsStageLoaded())
                _game.Stage.PerspectiveLinesRenderer.LineCollecting -= _OnPerspectiveLineCollecting;
        }

        #region Registrations

        private void _OnTemposChanged(ProjectManager manager) => UpdateTimeGrids();
        private void _OnSuddenPlusChanged(GamePlayManager manager) => UpdatePositionGrids();
        private void _OnStageTimeChanged(GameMusicPlayer.TimeChangedEventArgs args)
        {
            UpdateTimeGrids();
            UpdateCurveLine();
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
            PositionGridChanged,
            IsCurveOnChanged,
        }
    }
}