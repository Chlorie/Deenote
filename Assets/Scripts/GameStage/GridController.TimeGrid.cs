using Deenote.Project.Models;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GridController
    {
        [Header("Time Grid")]
        [SerializeField] Transform _timeGridParentTransform;
        private PooledObjectListView<LineRenderer> _timeGrids;

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
                _editorPropertiesWindow.NotifyTimeGridSubBeatCountChanged(__timeGridSubBeatCount);
            }
        }

        private ProjectModel.TempoListProxy CurrentProjectTempos => MainSystem.ProjectManager.CurrentProject.Tempos;

        // Note: Distance between tempoLines >= MinBeatLineInterval
        // Distance between beatLines and subBeatLines >= MinBeatLineIntterval / TimeGridSubBeatCount
        public void UpdateTimeGrids()
        {
            if (TimeGridSubBeatCount == 0) {
                _timeGrids.Clear();
                return;
            }

            using var grids = _timeGrids.Resetting();

            int iTempo = CurrentProjectTempos.GetTempoIndex(MainSystem.GameStage.CurrentMusicTime);
            if (iTempo < 0) {
                iTempo = 0;
            }

            float currentTime = MainSystem.GameStage.CurrentMusicTime;
            float appearTime = currentTime + MainSystem.GameStage.StageNoteAheadTime;
            float minSubBeatLineInterval = MainSystem.Args.MinBeatLineInterval / TimeGridSubBeatCount;

            for (; iTempo < CurrentProjectTempos.Count; iTempo++) {
                Tempo tempo = CurrentProjectTempos[iTempo];
                float nextTime = CurrentProjectTempos.GetNonOverflowTempoTime(iTempo + 1);

                // If bpm is 0, draw the tempo line only
                if (tempo.Bpm == 0f) {
                    if (tempo.StartTime > currentTime) {
                        if (tempo.StartTime >= appearTime)
                            return;
                        if (tempo.StartTime > nextTime - MainSystem.Args.MinBeatLineInterval)
                            goto RenderNextTempo;
                        grids.Add(out var line);
                        SetTimeGridKind(line, TimeGridKind.TempoLine);
                        SetTimeGridTime(line, tempo.StartTime - currentTime);
                    }
                    goto RenderNextTempo;
                }

                int beatIndex = Mathf.Max(0, tempo.GetBeatIndex(MainSystem.GameStage.CurrentMusicTime));

                for (; ; beatIndex++) {
                    float beatTime = tempo.GetBeatTime(beatIndex);
                    if (beatTime > currentTime) {
                        if (beatTime >= appearTime)
                            return;
                        if (beatTime > nextTime - minSubBeatLineInterval)
                            goto RenderNextTempo;
                        grids.Add(out var line);
                        SetTimeGridKind(line, beatIndex == 0 ? TimeGridKind.TempoLine : TimeGridKind.BeatLine);
                        SetTimeGridTime(line, beatTime - currentTime);
                    }

                    for (int i = 1; i < TimeGridSubBeatCount; i++) {
                        var subBeatTime = tempo.GetSubBeatTime(beatIndex + (float)i / TimeGridSubBeatCount);
                        if (subBeatTime > currentTime) {
                            if (subBeatTime >= appearTime)
                                return;
                            if (subBeatTime > nextTime - minSubBeatLineInterval)
                                goto RenderNextTempo;
                            grids.Add(out var line);
                            SetTimeGridKind(line, TimeGridKind.SubBeatLine);
                            SetTimeGridTime(line, subBeatTime - currentTime);
                        }
                    }
                }
            RenderNextTempo:
                continue;
            }

            if (MainSystem.GameStage.MusicLength < appearTime && CurrentProjectTempos[^1].Bpm != 0f) {
                grids.Add(out var lastLine);
                SetTimeGridKind(lastLine, TimeGridKind.TempoLine);
                SetTimeGridTime(lastLine, MainSystem.GameStage.MusicLength - currentTime);
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
                float nextSubBeatTime = tempo.GetSubBeatTime(beatIndex + (float)(subBeatIndex + 1) / TimeGridSubBeatCount);
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
            float prevSubBeatTime = tempo.GetSubBeatTime(prevBeatIndex + (float)prevSubBeatIndex / TimeGridSubBeatCount);

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
            float nextSubBeatTime = tempo.GetSubBeatTime(prevBeatIndex + (float)nextSubBeatIndex / TimeGridSubBeatCount);

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

        #region Pool & Unity

        private void SetTimeGridKind(LineRenderer line, TimeGridKind lineKind)
        {
            line.SetSolidColor(lineKind switch {
                TimeGridKind.SubBeatLine => MainSystem.Args.SubBeatLineColor,
                TimeGridKind.BeatLine => MainSystem.Args.BeatLineColor,
                TimeGridKind.TempoLine => MainSystem.Args.TempoLineColor,
                _ => Color.white,
            });
        }

        private void SetTimeGridTime(LineRenderer line, float timeOffsetToStage)
        {
            var z = MainSystem.Args.OffsetTimeToZ(timeOffsetToStage);
            var x = MainSystem.Args.PositionToX(MainSystem.Args.StageMaxPosition);
            line.SetPosition(0, _stage.NormalizeGridPosition(new Vector3(-x, 0, z)));
            line.SetPosition(1, _stage.NormalizeGridPosition(new Vector3(x, 0, z)));

            float alpha = Mathf.Clamp01((_stage.StageNoteAheadTime - timeOffsetToStage) / MainSystem.Args.ZToOffsetTime(_stage.Args.NoteFadeInZRange));
            line.SetGradientColor(alpha, alpha);
        }

        private void AwakeTimeGrid()
        {
            _timeGrids = new PooledObjectListView<LineRenderer>(UnityUtils.CreateObjectPool(() =>
            {
                var line = Instantiate(_linePrefab, _timeGridParentTransform);
                line.sortingOrder = -14;
                line.widthMultiplier = MainSystem.Args.GridWidth;
                line.positionCount = 2;
                return line;
            }));
        }

        #endregion

        private enum TimeGridKind
        {
            SubBeatLine,
            BeatLine,
            TempoLine,
        }
    }
}
