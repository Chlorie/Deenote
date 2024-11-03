#nullable enable

using Deenote.UI.ComponentModel;
using Deenote.Utilities;
using System;

namespace Deenote.GameStage
{
    public sealed partial class GridController : SingletonBehavior<GridController>, INotifyPropertyChange<GridController, GridController.NotifyProperty>
    {
        /// <returns>
        /// Note that if given time is earlier than first time grid,
        /// returns the original time
        /// </returns>
        public NoteCoord Quantize(NoteCoord coord, bool snapPosition, bool snapTime)
        {
            float snappedTime = snapTime ? GetNearestTimeGridTime(coord.Time) ?? coord.Time : coord.Time;
            float snappedPos = snapPosition
                ? IsCurveOn && _curveLineData.GetValue(snappedTime) is { } pos
                    ? pos
                    : GetNearestVerticalGridPosition(coord.Position) ?? coord.Position
                : coord.Position;
            return NoteCoord.ClampPosition(snappedTime, snappedPos);
        }

        private void Start()
        {
            // TODO: Temp Start
            _verticalGridGenerationKind = VerticalGridGenerationKind.ByKeyCount;
            VerticalGridCount = 9;
            TimeGridSubBeatCount = 1;

            MainSystem.ProjectManager.RegisterPropertyChangeNotification(
                Project.ProjectManager.NotifyProperty.Tempos,
                projm => UpdateTimeGrids());
            MainSystem.GameStage.RegisterPropertyChangeNotification(
               GameStageController.NotifyProperty.StageNotesUpdated,
               stage =>
               {
                   UpdateTimeGrids();
                   UpdateCurveLine();
               });
            UpdateTimeGrids();
            UpdateCurveLine();
        }

        private void Update()
        {
            DrawTimeGrids();
            DrawVerticalGrids();
            DrawCurve();
        }

        private PropertyChangeNotifier<GridController, NotifyProperty> _propertyChangeNotifier;
        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<GridController> action)
            => _propertyChangeNotifier.AddListener(flag, action);

        public enum NotifyProperty
        {
            TimeGridSubBeatCount,
            VerticalGridCount,
            IsCurveOn,
        }
    }
}