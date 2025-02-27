#nullable enable

using Deenote.Entities;
using Deenote.GamePlay.Audio;
using Deenote.Library.Components;
using Deenote.Project;

namespace Deenote.GamePlay
{
    public sealed partial class GridsManager : FlagNotifiable<GridsManager, GridsManager.NotificationFlag>
    {
        private readonly GamePlayManager _game;

        public GridsManager(GamePlayManager manager)
        {
            _game = manager;
            MainSystem.ProjectManager.RegisterNotification(
                Project.ProjectManager.NotificationFlag.ProjectTempos,
                _OnTemposChanged);
            _game.RegisterNotification(
                GamePlayManager.NotificationFlag.SuddenPlus,
                _OnSuddenPlusChanged);
            _game.MusicPlayer.TimeChanged += _OnStageTimeChanged;
        }

        public void Destroy()
        {
            MainSystem.ProjectManager.UnregisterNotification(
                ProjectManager.NotificationFlag.ProjectTempos,
                _OnTemposChanged);
            _game.UnregisterNotification(
                GamePlayManager.NotificationFlag.SuddenPlus,
                _OnSuddenPlusChanged);
            _game.MusicPlayer.TimeChanged -= _OnStageTimeChanged;
        }

        #region Registrations

        private void _OnTemposChanged(ProjectManager manager) => UpdateTimeGrids();
        private void _OnSuddenPlusChanged(GamePlayManager manager) => UpdatePositionGrids();
        private void _OnStageTimeChanged(GameMusicPlayer.TimeChangedEventArgs args)
        {
            UpdateTimeGrids();
            UpdateCurveLine();
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

        /// <summary>
        /// Call every update
        /// </summary>
        internal void SubmitLinesRender()
        {
            SubmitTimeGridRender();
            SubmitPositionGridsRender();
            SubmitCurveRender();
        }

        public enum NotificationFlag
        {
            TimeGridSubBeatCountChanged,
            PositionGridChanged,
            IsCurveOnChanged,
        }
    }
}