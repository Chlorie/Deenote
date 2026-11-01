#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities;
using UnityEngine;

namespace Deenote.Core.Editing
{
    partial class StageNotePlacer
    {
        private SpeedChangeWarningIndicatorController? _speedChangeWarningIndicator;

        private void InitializeSpeedChangeWarningIndicator(GameStageController stage)
        {
            var prefab = stage.Args.SpeedChangeWarningIndicatorPrefab;
            if (_speedChangeWarningIndicator is not null) {
                Object.Destroy(_speedChangeWarningIndicator.gameObject);
            }

            _speedChangeWarningIndicator = Object.Instantiate(prefab, stage.NoteIndicatorPanelTransform);
            _speedChangeWarningIndicator.OnInstantiate(this);
            _speedChangeWarningIndicator.gameObject.SetActive(false);
        }

        private void MoveSpeedChangeWarningIndicatorTo(float time)
        {
            _speedChangeWarningIndicator?.MoveTo(time);
        }

        private float GetWarningTime(NoteCoord coord)
            => _editor._game.Grids.Quantize(coord, false, SnapToTimeGrid).Time;

        private StateFlag BeginPlaceSpeedChangeWarning(NoteCoord coord, Vector2 mousePosition)
        {
            MoveSpeedChangeWarningIndicatorTo(GetWarningTime(coord));
            return StateFlag.PlacingSpeedChangeWarningNote;
        }

        private void UpdatePlaceSpeedChangeWarning(NoteCoord coord, Vector2 mousePosition)
        {
            MoveSpeedChangeWarningIndicatorTo(GetWarningTime(coord));
        }

        private StateFlag EndPlaceSpeedChangeWarning(NoteCoord coord, Vector2 mousePosition)
        {
            if (IsSpeedChangeWarningMode && _speedChangeWarningIndicator is not null) {
                var warning = _speedChangeWarningIndicator.CreateModel();
                _editor.AddSpeedChangeWarning(warning);
            }

            return IsSpeedChangeWarningMode
                ? StateFlag.IdlePlacingSpeedChangeWarningNote
                : StateFlag.Idle;
        }
    }
}
