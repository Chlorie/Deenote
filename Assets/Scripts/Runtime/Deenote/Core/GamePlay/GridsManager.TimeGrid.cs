#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities;
using Deenote.Library;
using Deenote.Library.Numerics;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    partial class GridsManager
    {
        private const int TimeGridMaxSubBeatCount = 64;
        /// <summary>
        /// If distance from time to grid &lt;= this, treat as equal
        /// </summary>
        private const float TimeGridMinEqualityThreshold = 1e-4f;

        private List<TimeGridLineData> _timeGridLines = new();

        private int _timeGridSubBeatCount_bf;
        private bool _timeGridVisible_bf;

        /// <summary>
        /// Range [0,64]
        /// </summary>
        public int TimeGridSubBeatCount
        {
            get => _timeGridSubBeatCount_bf;
            set {
                value = Mathf.Clamp(value, 0, TimeGridMaxSubBeatCount);
                if (Utils.SetField(ref _timeGridSubBeatCount_bf, value)) {
                    UpdateTimeGrids();
                    NotifyFlag(NotificationFlag.TimeGridSubBeatCountChanged);
                }
            }
        }

        public bool TimeGridVisible
        {
            get => _timeGridVisible_bf;
            set {
                if (Utils.SetField(ref _timeGridVisible_bf, value)) {
                    UpdateTimeGrids();
                    NotifyFlag(NotificationFlag.TimeGridVisible);
                }
            }
        }

        private void SubmitTimeGridRender(PerspectiveLinesRenderer.LineCollector collector)
        {
            _game.AssertStageLoaded();

            if (_timeGridLines.Count == 0)
                return;

            float minx = _game.ConvertNoteCoordPositionToWorldX(-EntityArgs.StageMaxPosition);
            float maxx = _game.ConvertNoteCoordPositionToWorldX(EntityArgs.StageMaxPosition);

            var args = _game.Stage.GridLineArgs;
            foreach (var (time, kind) in _timeGridLines) {
                var (color, width) = kind switch {
                    TimeGridLineKind.SubBeatLine => (args.SubBeatLineColor, args.TimeGridLineWidth),
                    TimeGridLineKind.BeatLine => (args.BeatLineColor, args.TimeGridBeatLineWidth),
                    TimeGridLineKind.TempoLine => (args.TempoLineColor, args.TimeGridTempoLineWidth),
                    _ => throw new System.NotImplementedException(),
                };
                var z = _game.ConvertNoteCoordTimeToWorldZ(time, _editor.Placer.PlacingNoteSpeed);
                collector.AddLine(new Vector2(minx, z), new Vector2(maxx, z), color, width);
            }
        }

        // Note: Distance between tempoLines >= MinBeatLineInterval
        // Distance between beatLines and subBeatLines >= MinBeatLineIntterval / TimeGridSubBeatCount
        private void UpdateTimeGrids()
        {
            var projManager = MainSystem.ProjectManager;
            if (!projManager.IsProjectLoaded())
                return;
            if (!_game.IsStageLoaded())
                return;

            _timeGridLines.Clear();

            if (!TimeGridVisible)
                return;

            var project = projManager.CurrentProject;
            var currentTime = _game.MusicPlayer.Time;
            var tempoIndex = project.GetTempoIndex(currentTime);
            if (tempoIndex < 0)
                tempoIndex = 0;
            float appearTime = currentTime + _game.GetStageNoteAppearAheadTime(_editor.Placer.PlacingNoteSpeed);
            float minSubBeatInterval = Tempo.MinBeatLineInterval / TimeGridSubBeatCount;

            for (; tempoIndex < project.Tempos.Length; tempoIndex++) {
                if (!ProcessTempoAt(tempoIndex))
                    return;
            }

            bool ProcessTempoAt(int tempoIndex)
            {
                Tempo tempo = project.Tempos[tempoIndex];
                float nextTime = project.GetNonOverflowTempoTime(tempoIndex + 1);

                int beatIndex = Mathf.Max(0, tempo.GetBeatIndex(currentTime));

                for (; ; beatIndex++) {
                    float beatTime = tempo.GetBeatTime(beatIndex);
                    if (beatTime > currentTime) {
                        if (beatTime >= nextTime - Tempo.MinBeatLineInterval)
                            return true;
                        if (beatTime >= appearTime)
                            return false;
                        var kind = beatIndex == 0 ? TimeGridLineKind.TempoLine : TimeGridLineKind.BeatLine;
                        _timeGridLines.Add(new TimeGridLineData(beatTime - currentTime, kind));
                    }
                    for (int i = 1; i < TimeGridSubBeatCount; i++) {
                        var subBeatTime = tempo.GetSubBeatTime(beatIndex + (float)i / TimeGridSubBeatCount);
                        if (subBeatTime <= currentTime)
                            continue;
                        if (subBeatTime >= nextTime - minSubBeatInterval)
                            return true;
                        if (subBeatTime >= appearTime)
                            return false;
                        _timeGridLines.Add(new TimeGridLineData(subBeatTime - currentTime, TimeGridLineKind.SubBeatLine));
                    }
                }
            }
        }

        // Note: 与下一个tempo衔接的一拍的子拍线不按照与下一个tempo.StartTime的间距平分，因此
        // get子拍线使用tempo.GetBeatTime(float);即可
        // 若要恢复旧版dnt的方式，UpdateTimeGrid，Get/FloorTo/CeilToNearestTimeGridTime都得改

        /// <returns><see langword="null"/> if given time is earlier than first tempo or later than last beat line</returns>
        public float? GetNearestTimeGridTime(float time)
        {
            if (TimeGridSubBeatCount == 0)
                return null;

            MainSystem.ProjectManager.AssertProjectLoaded();
            var project = MainSystem.ProjectManager.CurrentProject;
            var tempoIndex = project.GetTempoIndex(time);
            if (tempoIndex < 0)
                return null;

            Tempo tempo = project.Tempos[tempoIndex];
            if (tempoIndex == project.Tempos.Length - 1 && tempo.Bpm == 0f)
                return null;

            float nextTempoTime = project.GetNonOverflowTempoTime(tempoIndex + 1);
            int beatIndex = tempo.GetBeatIndex(time);
            float prevBeatTime = tempo.GetBeatTime(beatIndex);

            float prevBeatDelta = time - prevBeatTime;
            int subBeatIndex = Mathf.FloorToInt(prevBeatDelta * TimeGridSubBeatCount / tempo.BeatInterval);

            float prevSubBeatTime = GetPrevSubBeatTime();
            float nextSubBeatTime = GetNextSubBeatTime();

            float GetPrevSubBeatTime()
            {
                float prevSubBeatTime = tempo.GetSubBeatTime(beatIndex + (float)subBeatIndex / TimeGridSubBeatCount);
                if (prevSubBeatTime > nextTempoTime - (Tempo.MinBeatLineInterval / TimeGridSubBeatCount)) {
                    subBeatIndex--;
                    return tempo.GetSubBeatTime(beatIndex + (float)subBeatIndex / TimeGridSubBeatCount);
                }
                return prevSubBeatTime;
            }

            float GetNextSubBeatTime()
            {
                float nextSubBeatTime =
                    tempo.GetSubBeatTime(beatIndex + (float)(subBeatIndex + 1) / TimeGridSubBeatCount);
                if (nextSubBeatTime > nextTempoTime - (Tempo.MinBeatLineInterval / TimeGridSubBeatCount)) {
                    // Here next subBeatTime not rendered, so we use nextTempoTime
                    return nextTempoTime;
                }
                return nextSubBeatTime;
            }

            float prevDelta = time - prevSubBeatTime;
            float nextDelta = nextSubBeatTime - time;
            return prevDelta <= nextDelta ? prevSubBeatTime : nextSubBeatTime;
        }

        // Optimize: Floor/CeilToNext...里有几个处理note与格线过近时的递归调用，疑似可以写成非递归，建议后续看看

        /// <returns><see langword="null"/> if given time is earlier than first tempo</returns>
        public float? FloorToNearestNextTimeGridTime(float time)
        {
            if (TimeGridSubBeatCount == 0)
                return null;

            MainSystem.ProjectManager.AssertProjectLoaded();
            var project = MainSystem.ProjectManager.CurrentProject;
            var tempoIndex = project.GetCeilingTempoIndex(time) - 1;
            if (tempoIndex < 0)
                return null;

            Tempo tempo = project.Tempos[tempoIndex];
            if (tempoIndex == project.Tempos.Length - 1 && tempo.Bpm == 0f)
                return null;

            if (time - tempo.StartTime <= TimeGridMinEqualityThreshold)
                return FloorToNearestNextTimeGridTime(tempo.StartTime);

            Debug.Assert(tempo.GetCeilingBeatIndex(time) > 0);

            float nextTempoTime = project.GetNonOverflowTempoTime(tempoIndex + 1);
            int prevBeatIndex = tempo.GetCeilingBeatIndex(time) - 1;
            float prevBeatTime = tempo.GetBeatTime(prevBeatIndex);

            // Floating-point error handling
            if (Mathf.Approximately(prevBeatTime, time)) {
                prevBeatIndex--;
                prevBeatTime = tempo.GetBeatTime(prevBeatIndex);
            }

            if (time - prevBeatTime <= TimeGridMinEqualityThreshold)
                return FloorToNearestNextTimeGridTime(prevBeatTime);

            float prevBeatDelta = time - prevBeatTime;
            int prevSubBeatIndex = Mathf.CeilToInt(prevBeatDelta * TimeGridSubBeatCount / tempo.BeatInterval) - 1;
            float prevSubBeatTime = tempo.GetSubBeatTime(prevBeatIndex + (float)prevSubBeatIndex / TimeGridSubBeatCount);

            // Floating-point error handling
            if (Mathf.Approximately(prevSubBeatTime, time)) {
                prevSubBeatIndex--;
                prevSubBeatTime = tempo.GetSubBeatTime(prevBeatIndex + (float)prevSubBeatIndex / TimeGridSubBeatCount);
            }

            if (time - prevSubBeatTime <= TimeGridMinEqualityThreshold)
                return FloorToNearestNextTimeGridTime(prevSubBeatTime);

            if (prevSubBeatTime > nextTempoTime - Tempo.MinBeatLineInterval / TimeGridSubBeatCount) {
                // the diff of old and new prevSubBeatIndex should be minSubBeatLineInterval,
                // so here we needn't while, maybe.
                prevSubBeatIndex--;
                prevSubBeatTime = tempo.GetSubBeatTime(prevBeatIndex + (float)prevSubBeatIndex / TimeGridSubBeatCount);
            }

            return prevSubBeatTime;
        }

        /// <returns><see langword="null"/> if given time is later than last line</returns>
        public float? CeilToNearestNextTimeGridTime(float time)
        {
            if (TimeGridSubBeatCount == 0)
                return null;

            MainSystem.ProjectManager.AssertProjectLoaded();
            var project = MainSystem.ProjectManager.CurrentProject;
            var tempoIndex = project.GetTempoIndex(time);
            if (tempoIndex < 0)
                return project.Tempos.Length > 0 ? project.Tempos[0].StartTime : null;

            Tempo tempo = project.Tempos[tempoIndex];
            // Is last tempo and last tempo is Bpm 0, there's no beatline
            if (tempoIndex == project.Tempos.Length - 1 && tempo.Bpm == 0f)
                return null;

            float nextTempoTime = project.GetNonOverflowTempoTime(tempoIndex + 1);
            // time is really near next tempo, we should ignore the next tempo time and ceil to the first subbeatline of next tempo
            if (time >= nextTempoTime - TimeGridMinEqualityThreshold)
                return CeilToNearestNextTimeGridTime(nextTempoTime);

            int prevBeatIndex = tempo.GetBeatIndex(time);
            float nextBeatTime = tempo.GetBeatTime(prevBeatIndex + 1);

            // Floating-point error handling
            if (Mathf.Approximately(nextBeatTime, time)) {
                prevBeatIndex++;
                nextBeatTime = tempo.GetBeatTime(prevBeatIndex + 1);
            }


            // time is really near 
            if (time >= nextBeatTime - TimeGridMinEqualityThreshold)
                return CeilToNearestNextTimeGridTime(nextBeatTime);
            float prevBeatTime = tempo.GetBeatTime(prevBeatIndex);

            float prevBeatDelta = time - prevBeatTime;
            int nextSubBeatIndex = Mathf.FloorToInt(prevBeatDelta * TimeGridSubBeatCount / tempo.BeatInterval) + 1;
            float nextSubBeatTime = tempo.GetSubBeatTime(prevBeatIndex + (float)nextSubBeatIndex / TimeGridSubBeatCount);

            // Floating-point error handling
            if (Mathf.Approximately(nextSubBeatTime, time)) {
                nextSubBeatIndex++;
                nextSubBeatTime = tempo.GetSubBeatTime(prevBeatIndex + (float)nextSubBeatIndex / TimeGridSubBeatCount);
            }

            if (time >= nextSubBeatTime - TimeGridMinEqualityThreshold)
                return CeilToNearestNextTimeGridTime(nextSubBeatTime);

            if (nextSubBeatTime > nextTempoTime - (Tempo.MinBeatLineInterval / TimeGridSubBeatCount)) {
                // `time` is between an unrendered beatTime and nextTempoTime,
                // so the next nearest grid is next Tempo
                return nextTempoTime;
            }

            return nextSubBeatTime;
        }

        private enum TimeGridLineKind
        {
            SubBeatLine,
            BeatLine,
            TempoLine,
        }

        /// <param name="Time">Time offset from currentTime</param>
        /// <param name="Kind"></param>
        private record struct TimeGridLineData(
            float Time,
            TimeGridLineKind Kind);
    }
}