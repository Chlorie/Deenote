#nullable enable

using Deenote.Edit.Operations;
using Deenote.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Project.Models
{
    partial class ProjectModel
    {
        partial struct TempoListProxy
        {
            public InsertTempoOperation InsertTempo(Tempo tempo, float endTime)
            {
                Debug.Assert(endTime >= tempo.StartTime);

                var tempos = _projectModel._tempos;

                int i = 0;
                for (; i < tempos.Count; i++) {
                    // If |tempo.StartTime - prevTempo.StartTime| <= MinBeatLineInterval,
                    // we set prevTempo = tempo
                    // If |otherInsertTime - nextTempo.StartTime| <= MinBeatLineInterval,
                    // set otherInsertTime = nextTempo.StartTime, means we ignore this insertTime
                    if (tempo.StartTime < tempos[i].StartTime + MainSystem.Args.MinBeatLineInterval)
                        break;
                }
                int removeStartIndex = i;
                for (; i < tempos.Count; i++) {
                    if (endTime < tempos[i].StartTime + MainSystem.Args.MinBeatLineInterval)
                        break;
                }
                int removeEndIndex = i;

                Tempo? insertEndTempo = null;
                Tempo? insertContinuingTempo = null;

                // |  .  .  .  . | . . .|   .   .   .   .  |
                //      ^tempo                ^endTime
                //               ^rmvStartIndex            ^rmvEndIndex
                //                      ^prevTempo         ^nextTempoTime
                //                              ^continuingTempoTime
                if (endTime - tempo.StartTime >= MainSystem.Args.MinBeatLineInterval) {
                    float nextTempoTime = GetNonOverflowTempoTime(removeEndIndex);
                    Tempo prevTempo = removeEndIndex < 1 ? default : tempos[removeEndIndex - 1];
                    float continuingTempoTime = prevTempo.GetBeatTime(prevTempo.GetCeilingBeatIndex(endTime));

                    if (continuingTempoTime <= nextTempoTime - MainSystem.Args.MinBeatLineInterval) {
                        nextTempoTime = continuingTempoTime;
                        insertContinuingTempo = new Tempo(prevTempo.Bpm, continuingTempoTime);
                    }
                    // else if continuingTempo is near to or greater than nextTempo, ignore continuingTempo.
                    else { }

                    if (endTime <= nextTempoTime - MainSystem.Args.MinBeatLineInterval) {
                        insertEndTempo = new Tempo(bpm: 0f, endTime);
                    }
                    // else if endTime is near to Min(nextTempo, continuingTempo), ignore endTime
                }
                // else if given tempo time range is too short, we only adjust startTime

                return new InsertTempoOperation(_projectModel._tempos, removeStartIndex, removeEndIndex, tempo,
                    insertEndTempo, insertContinuingTempo);
            }

            #region Impls

            public sealed class InsertTempoOperation : IUndoableOperation
            {
                private readonly List<Tempo> _tempoList;
                private readonly int _startIndex;
                private readonly int _endIndex;
                private readonly Tempo _insertStartTempo;
                private readonly Tempo? _insertEndTempo;
                private readonly Tempo? _insertContinuingTempo;
                private readonly Tempo[] _removeTempos;

                private Action? _onDone;

                public InsertTempoOperation(List<Tempo> tempoList, int startIndex, int endIndex, Tempo insertStartTempo,
                    Tempo? insertEndTempo, Tempo? insertContinuingTempo)
                {
                    _tempoList = tempoList;
                    _startIndex = startIndex;
                    _endIndex = endIndex;
                    _insertStartTempo = insertStartTempo;
                    _insertEndTempo = insertEndTempo;
                    _insertContinuingTempo = insertContinuingTempo;

                    _removeTempos = _endIndex == _startIndex ? Array.Empty<Tempo>() : new Tempo[_endIndex - _startIndex];
                    for (int i = 0; i < _removeTempos.Length; i++) {
                        _removeTempos[i] = _tempoList[_startIndex + i];
                    }
                }

                public InsertTempoOperation WithDoneAction(Action action)
                {
                    _onDone = action;
                    return this;
                }

                void IUndoableOperation.Redo()
                {
                    if (_insertContinuingTempo.HasValue) {
                        _tempoList.Insert(_endIndex, _insertContinuingTempo.Value);
                    }
                    if (_insertEndTempo.HasValue) {
                        _tempoList.Insert(_endIndex, _insertEndTempo.Value);
                    }
                    _tempoList.RemoveRange(_startIndex.._endIndex);
                    _tempoList.Insert(_startIndex, _insertStartTempo);

                    _onDone?.Invoke();
                }

                void IUndoableOperation.Undo()
                {
                    _tempoList.RemoveAt(_startIndex);
                    _tempoList.InsertRange(_startIndex, _removeTempos);
                    if (_insertEndTempo.HasValue) {
                        if (_insertContinuingTempo.HasValue) {
                            _tempoList.RemoveRange(_endIndex, 2);
                        }
                        else {
                            _tempoList.RemoveAt(_endIndex);
                        }
                    }

                    _onDone?.Invoke();
                }
            }

            #endregion
        }
    }
}