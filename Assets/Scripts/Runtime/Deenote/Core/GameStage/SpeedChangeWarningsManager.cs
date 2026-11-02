#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Entities.Comparisons;
using Deenote.Library.Collections.Generic;
using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Deenote.Core.GameStage
{
    /// <summary>
    /// Maintains the warning notes that are currently on stage. Warnings are not combo nodes
    /// and have no hit effect, so their time window is intentionally independent from
    /// <see cref="NotesManager"/>.
    /// </summary>
    public sealed class SpeedChangeWarningsManager
    {
        private readonly GamePlayManager _game;
        private readonly SortedList<GameStageSpeedChangeWarningNoteController> _trackingWarnings;

        private ObjectPool<GameStageSpeedChangeWarningNoteController>? _pool;

        internal SpeedChangeWarningsManager(GamePlayManager game)
        {
            _game = game;
            _trackingWarnings = new(Comparer<GameStageSpeedChangeWarningNoteController>.Create(
                (l, r) => NodeTimeComparer.Instance.Compare(l.Model, r.Model)));
        }

        internal ReadOnlySpan<GameStageSpeedChangeWarningNoteController> OnStageWarnings => _trackingWarnings.AsSpan();

        internal void Initialize(ObjectPool<GameStageSpeedChangeWarningNoteController> pool)
        {
            if (_pool is not null) {
                ClearTrackingWarnings();
                _pool.Dispose();
            }

            _pool = pool;
        }

        internal void ClearTrackingWarnings()
        {
            if (_pool is null)
                return;

            foreach (var warning in _trackingWarnings) {
                _pool.Release(warning);
            }
            _trackingWarnings.Clear();
        }

        internal void RefreshStageActiveWarnings()
        {
            if (_pool is null)
                return;

            ClearTrackingWarnings();

            if (!_game.IsChartLoaded() || !_game.IsStageLoaded())
                return;

            var currentTime = _game.MusicPlayer.Time;
            var activateTime = currentTime +
                _game.GetStageNoteAppearAheadTime(_game.HighlightedNoteSpeed);
            var warnings = _game.CurrentChart.SpeedChangeWarnings;

            foreach (var warning in warnings) {
                if (warning.Time <= currentTime)
                    continue;
                if (warning.Time > activateTime)
                    break;

                var item = _pool.Get();
                item.Initialize(warning);
                _trackingWarnings.Add(item);
            }

            foreach (var warning in _trackingWarnings) {
                warning.RefreshStageDeltaTime();
            }
        }

        internal void ShiftStageActiveWarnings() => RefreshStageActiveWarnings();

        internal void RefreshStageWarningTimeDisplay()
        {
            foreach (var warning in _trackingWarnings) {
                warning.RefreshStageDeltaTime();
            }
        }

        internal void RefreshColoring()
        {
            foreach (var warning in _trackingWarnings) {
                warning.RefreshColoring();
            }
        }
    }
}
