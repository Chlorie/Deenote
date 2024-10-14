using Deenote.Edit.Operations;
using Deenote.GameStage;
using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
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
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, valueSelector, editingTime: true)
                .WithDoneAction(() =>
                {
                    NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteTime, NotifyProperty.NotePosition);
                    _propertiesWindow.NotifyNoteTimeChanged(SelectedNotes.IsSameForAll(n => n.Data.Time, out var time)
                        ? time
                        : null);
                    _propertiesWindow.NotifyNotePositionChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.Position, out var pos) ? pos : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: false);
                }));
        }

        public void EditSelectedNotesTime(Func<float, float> valueSelector)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, nc => nc with { Time = valueSelector(nc.Time) }, true)
                .WithDoneAction(() =>
                {
                    NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteTime);
                    _propertiesWindow.NotifyNoteTimeChanged(SelectedNotes.IsSameForAll(n => n.Data.Time, out var time)
                        ? time
                        : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: false);
                }));
        }

        public void EditSelectedNotesTime(float newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, nc => nc with { Time = newValue }, true)
                .WithDoneAction(() =>
                {
                    NoteTimeComparer.AssertInOrder(Stage.Chart.Notes);
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteTime);
                    _propertiesWindow.NotifyNoteTimeChanged(SelectedNotes.IsSameForAll(n => n.Data.Time, out var time)
                        ? time
                        : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesPosition(Func<float, float> valueSelector)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, nc => nc with { Position = valueSelector(nc.Position) }, false)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NotePosition);
                    _propertiesWindow.NotifyNotePositionChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.Position, out var pos) ? pos : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesPosition(float newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesCoord(SelectedNotes, nc => nc with { Position = newValue }, false)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NotePosition);
                    _propertiesWindow.NotifyNotePositionChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.Position, out var pos) ? pos : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSize(Func<float, float> valueSelector)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, valueSelector, n => n.Size,
                    (n, v) => n.Size = MainSystem.Args.ClampNoteSize(v))
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteSize);
                    _propertiesWindow.NotifyNoteSizeChanged(SelectedNotes.IsSameForAll(n => n.Data.Size, out var size)
                        ? size
                        : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSize(float newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.Size, (n, v) => n.Size = MainSystem.Args.ClampNoteSize(v))
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteSize);
                    _propertiesWindow.NotifyNoteSizeChanged(SelectedNotes.IsSameForAll(n => n.Data.Size, out var size)
                        ? size
                        : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesShift(float newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.Shift, (n, v) => n.Shift = v)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteShift);
                    _propertiesWindow.NotifyNoteShiftChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.Shift, out var shift) ? shift : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesSpeed(float newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.Speed, (n, v) => n.Speed = v)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteSpeed);
                    _propertiesWindow.NotifyNoteSpeedChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.Speed, out var speed) ? speed : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesDuration(float newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotesDuration(SelectedNotes, newValue)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteDuration);
                    _propertiesWindow.NotifyNoteDurationChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.Duration, out var duration) ? duration : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesVibrate(bool newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.Vibrate, (n, v) => n.Vibrate = v)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteVibrate);
                    _propertiesWindow.NotifyNoteVibrateChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.Vibrate, out var vibrate) ? vibrate : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesIsSwipe(bool newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.IsSwipe, (n, v) => n.IsSwipe = v)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                    _propertiesWindow.NotifyNoteIsSwipeChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.IsSwipe, out var swipe) ? swipe : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesKind(NoteData.NoteKind kind)
        {
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
                            _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                            _propertiesWindow.NotifyNoteIsLinkChanged(true);
                            OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                        })
                        .WithUndoneAction(() =>
                        {
                            _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                            _propertiesWindow.NotifyNoteIsLinkChanged(
                                SelectedNotes.IsSameForAll(n => n.Data.IsSlide, out var slide) ? slide : null);
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
                            _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                            _propertiesWindow.NotifyNoteIsSwipeChanged(
                                SelectedNotes.IsSameForAll(n => n.Data.IsSwipe, out var swipe) ? swipe : null);
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
                        _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                        _propertiesWindow.NotifyNoteIsSwipeChanged(
                            SelectedNotes.IsSameForAll(n => n.Data.IsSwipe, out var swipe) ? swipe : null);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    });

            IUndoableOperation Unlink()
                => Stage.Chart.Notes.UnlinkNotes(SelectedNotes)
                    .WithRedoneAction(() =>
                    {
                        _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                        _propertiesWindow.NotifyNoteIsLinkChanged(false);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    })
                    .WithUndoneAction(() =>
                    {
                        _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                        _propertiesWindow.NotifyNoteIsLinkChanged(
                            SelectedNotes.IsSameForAll(n => n.Data.IsSlide, out var slide) ? slide : null);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    });
        }

        public void EditSelectedNotesWarningType(WarningType newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.WarningType, (n, v) => n.WarningType = v)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteWarningType);
                    _propertiesWindow.NotifyNoteWarningTypeChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.WarningType, out var wType) ? wType : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void EditSelectedNotesEventId(string newValue)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes
                .EditNotes(SelectedNotes, newValue, n => n.EventId, (n, v) => n.EventId = v)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteEventId);
                    _propertiesWindow.NotifyNoteEventIdChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.EventId, out var evId) ? evId : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        #endregion

        public void ApplySelectedNotesWithCurveTransform(GridController.CurveApplyProperty property)
        {
            IUndoableOperation operation = property switch {
                GridController.CurveApplyProperty.Size => Stage.Chart.Notes.EditNotes(SelectedNotes,
                    v => MainSystem.GameStage.Grids.GetCurveTransformedValue(v, GridController.CurveApplyProperty.Size) ?? v,
                    n => n.Size, (n, v) => n.Size = v)
                    .WithDoneAction(() =>
                    {
                        _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteSize);
                        _propertiesWindow.NotifyNoteSizeChanged(SelectedNotes.IsSameForAll(n => n.Data.Size, out var size)
                            ? size
                            : null);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    }),
                GridController.CurveApplyProperty.Speed => Stage.Chart.Notes.EditNotes(SelectedNotes,
                    v => MainSystem.GameStage.Grids.GetCurveTransformedValue(v, GridController.CurveApplyProperty.Speed) ?? v,
                    n => n.Speed, (n, v) => n.Speed = v)
                    .WithDoneAction(() =>
                    {
                        _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteSpeed);
                        _propertiesWindow.NotifyNoteSpeedChanged(
                            SelectedNotes.IsSameForAll(n => n.Data.Speed, out var speed) ? speed : null);
                        OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                    }),
                _ => null,
            };
            _operationHistory.Do(operation);
        }

        public void EditSelectedNoteSounds(ReadOnlySpan<PianoSoundValueData> values)
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotesSounds(SelectedNotes, values)
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteSounds);
                    _propertiesWindow.NotifyNotePianoSoundsChanged(SelectedNotes.IsSameForAll(n => n.Data.Sounds,
                        out var sounds, PianoSoundListDataEqualityComparer.Instance)
                        ? sounds.AsSpan()
                        : null);
                    OnNotesChanged(true, false, noteDataChangedExceptTime: true);
                }));
        }

        public void LinkSelectedNotes()
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes.LinkNotes(SelectedNotes)
                .WithRedoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                    _propertiesWindow.NotifyNoteIsLinkChanged(true);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                    _propertiesWindow.NotifyNoteIsLinkChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.IsSlide, out var slide) ? slide : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void UnlinkSelectedNotes()
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes.UnlinkNotes(SelectedNotes)
                .WithRedoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                    _propertiesWindow.NotifyNoteIsLinkChanged(false);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                })
                .WithUndoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteKind);
                    _propertiesWindow.NotifyNoteIsLinkChanged(
                        SelectedNotes.IsSameForAll(n => n.Data.IsSlide, out var slide) ? slide : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void SoundifySelectedNotes()
        {
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
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteSounds);
                    _propertiesWindow.NotifyNotePianoSoundsChanged(SelectedNotes.IsSameForAll(n => n.Data.Sounds,
                        out var sounds, PianoSoundListDataEqualityComparer.Instance)
                        ? sounds.AsSpan()
                        : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }

        public void DesoundifySelectedNotes()
        {
            if (SelectedNotes.IsEmpty)
                return;

            _operationHistory.Do(Stage.Chart.Notes.EditNotesSounds(SelectedNotes, Array.Empty<PianoSoundValueData>())
                .WithDoneAction(() =>
                {
                    _propertyChangedNotifier.Invoke(this, NotifyProperty.NoteSounds);
                    _propertiesWindow.NotifyNotePianoSoundsChanged(SelectedNotes.IsSameForAll(n => n.Data.Sounds,
                        out var sounds, PianoSoundListDataEqualityComparer.Instance)
                        ? sounds.AsSpan()
                        : null);
                    OnNotesChanged(false, false, noteDataChangedExceptTime: true);
                }));
        }
    }
}