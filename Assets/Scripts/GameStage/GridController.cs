using Deenote.UI.Windows;
using UnityEngine;

namespace Deenote.GameStage
{
    public sealed partial class GridController : MonoBehaviour
    {
        [SerializeField] GameStageController _stage;

        [Header("Notify")]
        [SerializeField] EditorPropertiesWindow _editorPropertiesWindow;

        [Header("Prefabs")]
        [SerializeField] LineRenderer _linePrefab;

        /// <returns>
        /// Note that if given time is earlier than first time grid,
        /// returns the original time
        /// </returns>
        public NoteCoord Quantize(NoteCoord coord, bool snapPosition, bool snapTime)
        {
            float snappedTime = snapTime ? GetNearestTimeGridTime(coord.Time) ?? coord.Time : coord.Time;
            float snappedPos = (snapPosition, IsCurveOn) switch {
                (true, true) => _curveLineData.GetPosition(snappedTime) ?? GetNearestVerticalGridPosition(coord.Position) ?? coord.Position,
                (true, false) => GetNearestVerticalGridPosition(coord.Position) ?? coord.Position,
                (false, _) => coord.Position,
            };
            return new(snappedPos, snappedTime);
        }

        private void Awake()
        {
            AwakeTimeGrid();
            AwakeVerticalGrid();
            AwakeCurve();
        }

        private void Start()
        {
            _verticalGridGenerationKind = VerticalGridGenerationKind.ByKeyCount;
            VerticalGridCount = 9;
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
    }
}
