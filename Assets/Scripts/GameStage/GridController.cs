using UnityEngine;

namespace Deenote.GameStage
{
    public sealed partial class GridController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] Transform _lineParentTransform;
        [SerializeField] LineRenderer _linePrefab;

        /// <returns>
        /// Note that if given time is earlier than first time grid,
        /// returns the original time
        /// </returns>
        public NoteCoord Quantize(NoteCoord coord, bool snapPosition, bool snapTime)
        {
            float snappedPos = snapPosition ? GetNearestVerticalGridPosition(coord.Position) : coord.Position;
            float snappedTime = snapTime ? GetNearestTimeGridTime(coord.Time) ?? coord.Time : coord.Time;
            return new(snappedPos, snappedTime);
        }

        private void Awake()
        {
            AwakeTimeGrid();
            AwakeVerticalGrid();
        }

        private void Start()
        {
            SetVerticalGridEqualInterval(9);
        }
    }
}
