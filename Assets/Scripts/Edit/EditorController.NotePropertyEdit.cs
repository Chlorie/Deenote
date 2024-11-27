#nullable enable

using Deenote.Edit.Operations;
using Deenote.GameStage;
using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using System.Runtime.CompilerServices;
using UnityEngine.Pool;

namespace Deenote.Edit
{
    partial class EditorController
    {
        private static readonly PianoSoundValueData[] _defaultNoteSounds =
            new[] { new PianoSoundValueData(0f, 0f, 72, 0) };

        #region Simple Properties

        public void EditSelectedNotesPositionCoord(Func<NoteCoord, NoteCoord> valueSelector)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, valueSelector, editingTime: true)
                .WithDoneAction(() =>
                {
                    NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteTime, NotifyProperty.NotePosition);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: false);
                }));
        }

        public void EditSelectedNotesTime(Func<float, float> valueSelector)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, nc => nc with { Time = valueSelector(nc.Time) }, true)
                .WithDoneAction(() =>
                {
                    NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteTime);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: false);
                }));
        }

        public void EditSelectedNotesTime(float newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, nc => nc with { Time = newValue }, true)
                .WithDoneAction(() =>
                {
                    NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteTime);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesPosition(Func<float, float> valueSelector)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, nc => nc with { Position = valueSelector(nc.Position) }, false)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NotePosition);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesPosition(float newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, nc => nc with { Position = newValue }, false)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NotePosition);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSize(Func<float, float> valueSelector)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, valueSelector, n => n.Size,
                    (n, v) => n.Size = MainSystem.Args.ClampNoteSize(v))
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteSize);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSize(float newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.Size, (n, v) => n.Size = MainSystem.Args.ClampNoteSize(v))
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteSize);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesShift(float newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.Shift, (n, v) => n.Shift = v)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteShift);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSpeed(float newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.Speed, (n, v) => n.Speed = v)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteSpeed);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesDuration(float newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesDuration(SelectedNotes, newValue)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteDuration);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesVibrate(bool newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.Vibrate, (n, v) => n.Vibrate = v)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteVibrate);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesIsSwipe(bool newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.IsSwipe, (n, v) => n.IsSwipe = v)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesKind(NoteData.NoteKind kind)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            // TODO: Maybe create new operation that cound combines 2 in NoteModelListProxy later
            // 因为有一部分操作是在入口方法进行的，有没有可能导致second执行出错？比如缓存的noteIndex不对
            switch (kind) {
                case NoteData.NoteKind.Click: {
                    var first = SetSwipeFalse();
                    var second = Unlink();
                    _operationHistory.Do(new CombinedOperation(first, second));
                    return;
                }
                case NoteData.NoteKind.Slide: {
                    var first = SetSwipeFalse();
                    var second = Stage.Chart.Notes.LinkNotes(SelectedNotes)
                        .WithRedoneAction(() =>
                        {
                            _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                            OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                        })
                        .WithUndoneAction(() =>
                        {
                            _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                            OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                        });
                    _operationHistory.Do(new CombinedOperation(first, second));
                    break;
                }
                case NoteData.NoteKind.Swipe: {
                    var first = Unlink();
                    var second = Stage.Chart.Notes
                        .EditNotes(SelectedNotes, true, n => n.IsSwipe, (n, v) => n.IsSwipe = v)
                        .WithDoneAction(() =>
                        {
                            _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                            OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                        });
                    _operationHistory.Do(new CombinedOperation(first, second));
                    break;
                }
                default:
                    break;
            }

            IUndoableOperation SetSwipeFalse()
                => Stage.Chart.Notes
                    .EditNotes(SelectedNotes, false, n => n.IsSwipe, (n, v) => n.IsSwipe = v)
                    .WithDoneAction(() =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    });

            IUndoableOperation Unlink()
                => Stage.Chart.Notes.UnlinkNotes(SelectedNotes)
                    .WithRedoneAction(() =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    })
                    .WithUndoneAction(() =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    });
        }

        public void EditSelectedNotesWarningType(WarningType newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.WarningType, (n, v) => n.WarningType = v)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteWarningType);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesEventId(string newValue)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.EventId, (n, v) => n.EventId = v)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteEventId);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        #endregion

        #region Link & Sound

        public void LinkSelectedNotes()
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes.LinkNotes(SelectedNotes)
                .WithRedoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void UnlinkSelectedNotes()
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes.UnlinkNotes(SelectedNotes)
                .WithRedoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteKind);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void SoundifySelectedNotes()
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;
            using var _en = ListPool<NoteModel>.Get(out var editNotes);

            foreach (var n in SelectedNotes) {
                if (!n.Data.HasSound)
                    editNotes.Add(n);
            }

            _operationHistory.Do(Stage.Chart.Notes.EditNotesSounds(editNotes.AsSpan(), _defaultNoteSounds)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteSounds);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void DesoundifySelectedNotes()
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotesSounds(SelectedNotes, Array.Empty<PianoSoundValueData>())
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteSounds);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        #endregion

        public void ApplySelectedNotesWithCurveTransform(GridController.CurveApplyProperty property)
        {
            if (Stage.Chart is null)
                return;

            IUndoableOperation operation = property switch {
                GridController.CurveApplyProperty.Size => Stage.Chart.Notes.EditNotes(SelectedNotes,
                    v => MainSystem.GameStage.Grids.GetCurveTransformedValue(v, GridController.CurveApplyProperty.Size) ?? v,
                    n => n.Size, (n, v) => n.Size = v)
                    .WithDoneAction(() =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteSize);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    }),
                GridController.CurveApplyProperty.Speed => Stage.Chart.Notes.EditNotes(SelectedNotes,
                    v => MainSystem.GameStage.Grids.GetCurveTransformedValue(v, GridController.CurveApplyProperty.Speed) ?? v,
                    n => n.Speed, (n, v) => n.Speed = v)
                    .WithDoneAction(() =>
                    {
                        _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteSpeed);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    }),
                _ => throw new SwitchExpressionException(property),
            };
            _operationHistory.Do(operation);
        }

        public void EditSelectedNoteSounds(ReadOnlySpan<PianoSoundValueData> values)
        {
            if (Stage.Chart is null)
                return;
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotesSounds(SelectedNotes, values)
                .WithDoneAction(() =>
                {
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.NoteSounds);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void InsertTempo(Tempo tempo, float endTime)
        {
            _operationHistory.Do(MainSystem.ProjectManager.DoInsertTempoOperation(tempo, endTime));
        }
    }
}