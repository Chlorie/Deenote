using Deenote.UI.ComponentModel;
using Deenote.UI.Windows;
using Deenote.Utilities;
using System;
using UnityEngine;

namespace Deenote.GameStage
{
    public sealed partial class GridController : SingletonBehavior<GridController>, INotifyPropertyChange<GridController, GridController.NotifyProperty>
    {
        [Header("Notify")]
        [SerializeField] EditorPropertiesWindow _editorPropertiesWindow;

        /// <returns>
        /// Note that if given time is earlier than first time grid,
        /// returns the original time
        /// </returns>
        public NoteCoord Quantize(NoteCoord coord, bool snapPosition, bool snapTime)
        {
            float snappedTime = snapTime ? GetNearestTimeGridTime(coord.Time) ?? coord.Time : coord.Time;
            float snappedPos = (snapPosition, IsCurveOn) switch {
                (true, true) => _curveLineData.GetValue(snappedTime) ??
                                GetNearestVerticalGridPosition(coord.Position) ?? coord.Position,
                (true, false) => GetNearestVerticalGridPosition(coord.Position) ?? coord.Position,
                (false, _) => coord.Position,
            };
            return NoteCoord.ClampPosition(snappedTime, snappedPos);
        }

        private void Start()
        {
            // TODO: Temp Start
            _verticalGridGenerationKind = VerticalGridGenerationKind.ByKeyCount;
            VerticalGridCount = 9;
            TimeGridSubBeatCount = 1;
        }

        private void Update()
        {
            DrawTimeGrids();
            DrawVerticalGrids();
            DrawCurve();
        }

        #region Notify

        public void NotifyGameStageProgressChanged()
        {
            UpdateTimeGrids();
            UpdateCurveLine();
        }

        public void NotifyCurrentProjectTemposChanged()
        {
            UpdateTimeGrids();
        }

        #endregion

        private PropertyChangeNotifier<GridController, NotifyProperty> _propertyChangedNotifier;
        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<GridController> action)
            => _propertyChangedNotifier.AddListener(flag, action);

        public enum NotifyProperty
        {
            TimeGridSubBeatCount,
            VerticalGridCount,
            IsCurveOn,
        }
    }
}