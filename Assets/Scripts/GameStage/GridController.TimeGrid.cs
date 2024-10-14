using System.Collections.Generic;
using Deenote.Project.Models;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GridController
    {
        private List<(float, TimeGridKind)> _timeGridData = new();

        [Header("Time Grid")]
        [SerializeField, Range(0, 64)]
        private int __timeGridSubBeatCount = 2;

        public int TimeGridSubBeatCount
        {
            get => __timeGridSubBeatCount;
            set {
                value = Mathf.Clamp(value, 0, 64);
                if (__timeGridSubBeatCount == value)
                    return;
                __timeGridSubBeatCount = value;
                UpdateTimeGrids();
                _propertyChangedNotifier.Invoke(this, NotifyProperty.TimeGridSubBeatCount);
                _editorPropertiesWindow.NotifyTimeGridSubBeatCountChanged(__timeGridSubBeatCount);
            }
        }

        private ProjectModel.TempoListProxy CurrentProjectTempos => MainSystem.ProjectManager.CurrentProject.Tempos;

        private void DrawTimeGrids()
        {
            var lineRenderer = PerspectiveLinesRenderer.Instance;
            float x = MainSystem.Args.PositionToX(MainSystem.Args.StageMaxPosition);
            foreach (var (z, kind) in _timeGridData) {
                var color = kind switch {
                    TimeGridKind.SubBeatLine => MainSystem.Args.SubBeatLineColor,
                    TimeGridKind.BeatLine => MainSystem.Args.BeatLineColor,
                    TimeGridKind.TempoLine => MainSystem.Args.TempoLineColor,
                    _ => Color.white,
                };
                lineRenderer.AddLine(new Vector3(-x, z), new Vector3(x, z), color, MainSystem.Args.GridWidth);
            }
        }

        // Note: Distance between tempoLines >= MinBeatLineInterval
        // Distance between beatLines and subBeatLines >= MinBeatLineIntterval / TimeGridSubBeatCount
        public void UpdateTimeGrids()
        {
            _timeGridData.Clear();

            int iTempo = CurrentProjectTempos.GetTempoIndex(MainSystem.GameStage.CurrentMusicTime);
            if (iTempo < 0) iTempo = 0;

            float currentTime = MainSystem.GameStage.CurrentMusicTime;
            float appearTime = currentTime + MainSystem.GameStage.StageNoteAheadTime;
            float minSubBeatLineInterval = MainSystem.Args.MinBeatLineInterval / TimeGridSubBeatCount;

            bool ProcessTempoAt(int tempoIdx)
            {
                Tempo tempo = CurrentProjectTempos[tempoIdx];
                float nextTime = CurrentProjectTempos.GetNonOverflowTempoTime(tempoIdx + 1);

                // If bpm is 0, just draw the tempo line
                if (tempo.Bpm == 0f) {
                    if (tempo.StartTime <= currentTime) return true;
                    if (tempo.StartTime >= appearTime) return false;
                    float z = MainSystem.Args.OffsetTimeToZ(tempo.StartTime - currentTime);
                    _timeGridData.Add((z, TimeGridKind.TempoLine));
                    return true;
                }

                int beatIndex = Mathf.Max(0, tempo.GetBeatIndex(MainSystem.GameStage.CurrentMusicTime));

                for (;; beatIndex++) {
                    float beatTime = tempo.GetBeatTime(beatIndex);
                    if (beatTime > currentTime) {
                        if (beatTime >= appearTime) return false;
                        float z = MainSystem.Args.OffsetTimeToZ(beatTime - currentTime);
                        var kind = beatIndex == 0 ? TimeGridKind.TempoLine : TimeGridKind.BeatLine;
                        _timeGridData.Add((z, kind));
                    }

                    for (int i = 1; i < TimeGridSubBeatCount; i++) {
                        var subBeatTime = tempo.GetSubBeatTime(beatIndex + (float)i / TimeGridSubBeatCount);
                        if (subBeatTime <= currentTime) continue;
                        if (subBeatTime >= appearTime) return false;
                        if (subBeatTime >= nextTime - minSubBeatLineInterval) return true;
                        float z = MainSystem.Args.OffsetTimeToZ(subBeatTime - currentTime);
                        _timeGridData.Add((z, TimeGridKind.SubBeatLine));
                    }
                }
            }

            for (; iTempo < CurrentProjectTempos.Count; iTempo++)
                if (!ProcessTempoAt(iTempo))
                    return;
        }

        // Note: 与下一个tempo衔接的一拍的子拍线不按照与下一个tempo.StartTime的间距平分，因此
        // get子拍线使用tempo.GetBeatTime(float);即可
        // 若要恢复旧版dnt的方式，UpdateTimeGrid，Get/FloorTo/CeilToNearestTimeGridTime都得改

        /// <returns><see langword="null"/> if given time is earlier than first tempo or later than last beat line</returns>
        public float? GetNearestTimeGridTime(float time)
        {
            if (TimeGridSubBeatCount == 0)
                return null;

            int tempoIndex = CurrentProjectTempos.GetTempoIndex(time);
            if (tempoIndex < 0) {
                return null;
            }

            Tempo tempo = CurrentProjectTempos[tempoIndex];
            if (tempoIndex == CurrentProjectTempos.Count - 1 && tempo.Bpm == 0f)
                return null;

            float nextTempoTime = CurrentProjectTempos.GetNonOverflowTempoTime(tempoIndex + 1);
            int beatIndex = tempo.GetBeatIndex(time);
            float prevBeatTime = tempo.GetBeatTime(beatIndex);

            float prevBeatDelta = time - prevBeatTime;
            int subBeatIndex = Mathf.FloorToInt(prevBeatDelta * TimeGridSubBeatCount / tempo.BeatInterval);

            float prevSubBeatTime = GetPrevSubBeatTime();
            float nextSubBeatTime = GetNextSubBeatTime();

            float GetPrevSubBeatTime()
            {
                float prevSubBeatTime = tempo.GetSubBeatTime(beatIndex + (float)subBeatIndex / TimeGridSubBeatCount);
                if (prevSubBeatTime > nextTempoTime - (MainSystem.Args.MinBeatLineInterval / TimeGridSubBeatCount)) {
                    subBeatIndex--;
                    return tempo.GetSubBeatTime(beatIndex + (float)subBeatIndex / TimeGridSubBeatCount);
                }
                return prevSubBeatTime;
            }

            float GetNextSubBeatTime()
            {
                float nextSubBeatTime =
                    tempo.GetSubBeatTime(beatIndex + (float)(subBeatIndex + 1) / TimeGridSubBeatCount);
                if (nextSubBeatTime > nextTempoTime - (MainSystem.Args.MinBeatLineInterval / TimeGridSubBeatCount)) {
                    // Here next subBeatTime not rendered, so we use nextTempoTime
                    return nextTempoTime;
                }
                return nextSubBeatTime;
            }

            float prevDelta = time - prevSubBeatTime;
            float nextDelta = nextSubBeatTime - time;
            return prevDelta <= nextDelta ? prevSubBeatTime : nextSubBeatTime;
        }

        /// <returns><see langword="null"/> if given time is earlier than first tempo</returns>
        public float? FloorToNextNearestTimeGridTime(float time)
        {
            if (TimeGridSubBeatCount == 0)
                return null;

            int tempoIndex = CurrentProjectTempos.GetCeilingTempoIndex(time) - 1;
            if (tempoIndex < 0)
                return null;

            Tempo tempo = CurrentProjectTempos[tempoIndex];
            if (tempoIndex == CurrentProjectTempos.Count - 1 && tempo.Bpm == 0f)
                return null;

            float nextTempoTime = CurrentProjectTempos.GetNonOverflowTempoTime(tempoIndex + 1);
            int prevBeatIndex = tempo.GetCeilingBeatIndex(time) - 1;
            float prevBeatTime = tempo.GetBeatTime(prevBeatIndex);

            float prevBeatDelta = time - prevBeatTime;
            int prevSubBeatIndex = Mathf.CeilToInt(prevBeatDelta * TimeGridSubBeatCount / tempo.BeatInterval) - 1;
            float prevSubBeatTime =
                tempo.GetSubBeatTime(prevBeatIndex + (float)prevSubBeatIndex / TimeGridSubBeatCount);

            if (prevSubBeatTime > nextTempoTime - (float)(MainSystem.Args.MinBeatLineInterval / TimeGridSubBeatCount)) {
                // the diff of old and new prevSubBeatIndex should be minSubBeatLineInterval,
                // so here we needn't while, maybe.
                prevSubBeatIndex--;
                return tempo.GetSubBeatTime(prevBeatIndex + (float)prevSubBeatIndex / TimeGridSubBeatCount);
            }

            return prevSubBeatTime;
        }

        /// <returns><see langword="null"/> if given time is later than last line</returns>
        public float? CeilToNextNearestTimeGridTime(float time)
        {
            if (TimeGridSubBeatCount == 0)
                return null;

            int tempoIndex = CurrentProjectTempos.GetTempoIndex(time);
            if (tempoIndex < 0)
                return CurrentProjectTempos.Count > 0 ? CurrentProjectTempos[0].StartTime : null;

            Tempo tempo = CurrentProjectTempos[tempoIndex];
            if (tempoIndex == CurrentProjectTempos.Count - 1 && tempo.Bpm == 0f)
                return null;

            float nextTempoTime = CurrentProjectTempos.GetNonOverflowTempoTime(tempoIndex + 1);
            int prevBeatIndex = tempo.GetBeatIndex(time);
            float prevBeatTime = tempo.GetBeatTime(prevBeatIndex);

            float prevBeatDelta = time - prevBeatTime;
            int nextSubBeatIndex = Mathf.FloorToInt(prevBeatDelta * TimeGridSubBeatCount / tempo.BeatInterval) + 1;
            float nextSubBeatTime =
                tempo.GetSubBeatTime(prevBeatIndex + (float)nextSubBeatIndex / TimeGridSubBeatCount);

            if (nextSubBeatTime > nextTempoTime - (MainSystem.Args.MinBeatLineInterval / TimeGridSubBeatCount)) {
                // `time` is between an unrendered beatTime and nextTempoTime,
                // so the next nearest grid is next Tempo
                return nextTempoTime;
            }

            return nextSubBeatTime;

            //if (tempoIndex + 1 < CurrentProjectTempos.Count) {
            //    nextSubBeatTime = Mathf.Min(nextSubBeatTime, CurrentProjectTempos.GetNonOverflowTempoTime(tempoIndex + 1));
            //}
            //else {
            //    Debug.Assert(MainSystem.GameStage.MusicLength == CurrentProjectTempos.GetNonOverflowTempoTime(tempoIndex + 1));
            //    // If Min(nextSubBeat, nextBeat, nextTempo) is nextTempo, and nextTempo is end of Audio
            //    // we dont want to move, so return null
            //    if (MainSystem.GameStage.MusicLength < nextSubBeatTime)
            //        return null;
            //}

            //return nextSubBeatTime;
        }

        private enum TimeGridKind
        {
            SubBeatLine,
            BeatLine,
            TempoLine,
        }
    }
}