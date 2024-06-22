using Deenote.Project.Models;
using Deenote.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    partial class GridController
    {
        [Header("Time Grid")]
        private ObjectPool<LineRenderer> _timeGridPool;
        private List<LineRenderer> _timeGrids;

        [SerializeField] private int _timeGridSubBeatCount = 2;
        private int _currentOnLineTempo;

        private List<Tempo> CurrentProjectTempos => MainSystem.ProjectManager.CurrentProject.Tempos;

        public void UpdateTimeGrids()
        {
            if (_timeGrids.Count > 0) {
                // TODO: Optimize
                foreach (var grid in _timeGrids) {
                    _timeGridPool.Release(grid);
                }
                _timeGrids.Clear();
            }

            int iTempo = GetOnLineTempoIndex(CurrentProjectTempos);
            _currentOnLineTempo = iTempo;

            if (iTempo < 0) {
                iTempo = 0;
            }

            for (; iTempo < CurrentProjectTempos.Count; iTempo++) {
                if (!RenderTempo(iTempo))
                    break;
            }

            static int GetOnLineTempoIndex(List<Tempo> tempos)
            {
                int i = 0;
                for (; i < tempos.Count; i++) {
                    var tempo = tempos[i];
                    if (tempo.StartTime >= MainSystem.GameStage.CurrentMusicTime) {
                        break;
                    }
                }
                return i - 1;
            }

            /// <return>false if rendering reached StageNoteAheadTime</return>
            bool RenderTempo(int tempoIndex)
            {
                float currentTime = MainSystem.GameStage.CurrentMusicTime;

                Tempo tempo = CurrentProjectTempos[tempoIndex];
                int beatIndex = Mathf.Max(0, tempo.GetBeatIndex(MainSystem.GameStage.CurrentMusicTime));
                float beatIntervalTime = 60 / tempo.Bpm;

                // When flag set false, all grids of this tempo are rendered;
                for (bool flag = true; flag; beatIndex++) {
                    float beatTime = tempo.StartTime + beatIndex * beatIntervalTime;

                    if (beatTime > currentTime) {
                        if (beatTime > currentTime + MainSystem.GameStage.StageNoteAheadTime)
                            return false;

                        _timeGrids.Add(GetTimeGrid(beatTime - currentTime, beatIndex == 0 ? TimeGridKind.TempoLine : TimeGridKind.BeatLine));
                    }

                    float nextBeatTime = tempo.StartTime + (beatIndex + 1) * beatIntervalTime;
                    float nextTempoTime = tempoIndex + 1 < CurrentProjectTempos.Count ? CurrentProjectTempos[tempoIndex + 1].StartTime : MainSystem.GameStage.MusicLength;
                    if (nextTempoTime < nextBeatTime) {
                        nextBeatTime = nextTempoTime;
                        flag = false;
                    }
                    for (int i = 1; i < _timeGridSubBeatCount; i++) {
                        var subBeatTime = Mathf.Lerp(beatTime, nextBeatTime, (float)i / _timeGridSubBeatCount);
                        if (subBeatTime > currentTime) {
                            if (subBeatTime > currentTime + MainSystem.GameStage.StageNoteAheadTime)
                                return false;

                            _timeGrids.Add(GetTimeGrid(subBeatTime - currentTime, TimeGridKind.SubBeatLine));
                        }
                    }
                }
                return true;
            }
        }

        /// <returns><see langword="null"/> if given time is earlier than first tempo</returns>
        public float? GetNearestTimeGridTime(float time)
        {
            int iTempo = GetTempoIndex(time);
            if (iTempo < 0) {
                return null;
            }

            Tempo tempo = CurrentProjectTempos[iTempo];
            int index = tempo.GetBeatIndex(time);
            float prevTime = tempo.GetBeatTime(index);

            float nextTempoTime = iTempo + 1 < CurrentProjectTempos.Count ? CurrentProjectTempos[iTempo + 1].StartTime : MainSystem.GameStage.MusicLength;
            float nextTime = Mathf.Min(tempo.GetBeatTime(index + 1), nextTempoTime);

            float prevDelta = time - prevTime;
            float nextDelta = nextTime - time;
            return prevDelta <= nextDelta ? prevTime : nextTime;
        }

        public float? FloorToNearestTimeGridTime(float time)
        {
            int iTempo = GetTempoIndex(time);
            if (iTempo < 0)
                return null;

            Tempo tempo = CurrentProjectTempos[iTempo];
            int index = tempo.GetBeatIndex(time);
            float prevTime = tempo.GetBeatTime(index);
            return prevTime;
        }

        public float? CeilToNearestTimeGridTime(float time)
        {
            int iTempo = GetTempoIndex(time);
            if (iTempo < 0)
                return null;

            Tempo tempo = CurrentProjectTempos[iTempo];
            int index = tempo.GetBeatIndex(time);
            float nextTempoTime = iTempo + 1 < CurrentProjectTempos.Count ? CurrentProjectTempos[iTempo+1].StartTime:MainSystem.GameStage.MusicLength;
            float nextTime = Mathf.Min(tempo.GetBeatTime(index + 1), nextTempoTime);
            return nextTime;
        }

        private int GetTempoIndex(float time)
        {
            int iTempo;
            for (iTempo = 0; iTempo < CurrentProjectTempos.Count; iTempo++) {
                if (CurrentProjectTempos[iTempo].StartTime > time)
                    break;
            }
            return iTempo - 1;
        }

        #region Pool & Unity

        /// <param name="stageTime">ActualTime - CurrentTime</param>
        private LineRenderer GetTimeGrid(float stageTime, TimeGridKind lineKind)
        {
            var line = _timeGridPool.Get();
            line.widthMultiplier = 0.035f;
            line.positionCount = 2;
            line.SetSolidColor(lineKind switch {
                TimeGridKind.SubBeatLine => new Color(42f / 255f, 42 / 255f, 42 / 255f, 0.75f),
                TimeGridKind.BeatLine => new Color(0.5f, 0f, 0f, 1f),
                TimeGridKind.TempoLine => new Color(0f, 0.5f, 0.5f, 1f),
                _ => Color.white,
            });

            SetTimeGridTime(line, stageTime);
            return line;
        }

        private void SetTimeGridTime(LineRenderer line, float timeOffsetToCurrent)
        {
            var z = MainSystem.Args.OffsetTimeToZ(timeOffsetToCurrent);
            var x = MainSystem.Args.SideLineX;
            line.SetPosition(0, new Vector3(-x, 0, z));
            line.SetPosition(1, new Vector3(x, 0, z));
        }

        private void AwakeTimeGrid()
        {
            _timeGridPool = UnityUtils.CreateObjectPool(_linePrefab, _lineParentTransform);
            _timeGrids = new();
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