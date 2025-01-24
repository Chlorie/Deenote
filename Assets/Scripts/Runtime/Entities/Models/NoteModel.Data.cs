#nullable enable

using CommunityToolkit.Diagnostics;
using Deenote.Library.Collections;
using System;
using System.Diagnostics;
using UnityEngine.Pool;

namespace Deenote.Entities.Models
{
    partial class NoteModel
    {
        internal NoteKind _kind;

        public float Position { get => _position; set => _position = value; }
        public float Time { get => _time; set => _time = value; }
        public float Size { get => _size; set => _size = value; }
        /// <remarks>
        /// The value is not guaranteed to be 0 if <see cref="IsHold"/> is <see langword="false"/>,
        /// use <see cref="GetActualDuration"/> to get the actual duration
        /// </remarks>
        public float Duration { get => _duration; set => _duration = value; }

        public NoteKind Kind { get => _kind; set => _kind = value; }
        public float Speed { get => _speed; set => _speed = value; }
        public ReadOnlySpan<PianoSoundValueModel> Sounds => _sounds.AsSpan();

        public float Shift { get => _shift; set => _shift = value; }
        public string EventId { get => _eventId; set => _eventId = value; }
        public WarningType WarningType { get => _warningType; set => _warningType = value; }
#pragma warning disable CS0618
        public bool Vibrate { get => _vibrate; set => _vibrate = value; }
#pragma warning restore CS0618

        internal NoteModel? _prevLink;
        internal NoteModel? _nextLink;

        public NoteModel? PrevLink
        {
            get {
                Debug.Assert(_prevLink is null || IsSlide);
                return _prevLink;
            }
        }

        public NoteModel? NextLink
        {
            get {
                Debug.Assert(_nextLink is null || IsSlide);
                return _nextLink;
            }
        }

        public bool IsSlide => Kind is NoteKind.Slide;

        public bool IsSwipe => Kind is NoteKind.Swipe;

        public bool IsHold => Kind is not NoteKind.Swipe && Duration > 0f;

        public bool HasSounds => _sounds.Count > 0;

        public float EndTime => Time + GetActualDuration();

        public NoteCoord PositionCoord
        {
            get => new(Position, Time);
            set => (_position, _time) = (value.Position, value.Time);
        }

        /// <summary>
        /// Get the actual duration, which ensures note is a hold note if return value &gt; 0
        /// </summary>
        /// <returns>The actual hold duration, 0 if the note is not a hold note</returns>
        public float GetActualDuration()
        {
            return IsHold ? Duration : 0f;
        }

        public enum NoteKind { Click, Slide, Swipe, }

        /// <summary>
        /// Set <paramref name="note"/> to non-slide, and 
        /// link its original prev note to original next note
        /// </summary>
        public void UnlinkWithoutCutChain(bool keepNoteKind = false)
        {
            var prevLink = _prevLink;
            var nextLink = _nextLink;

            if (!keepNoteKind)
                _kind = NoteKind.Click;
            if (prevLink is not null)
                prevLink._nextLink = nextLink;
            if (nextLink is not null)
                nextLink._prevLink = prevLink;
        }

        /// <summary>
        /// Insert <paramref name="note"/> into another link after <paramref name="prevLink"/>,
        /// this method auto calls <see cref="UnlinkWithoutCutLinkChain(NoteData)"/> first
        /// </summary>
        public void InsertAsLinkAfter(NoteModel other)
        {
            if (IsSlide)
                UnlinkWithoutCutChain();

            _kind = NoteKind.Slide;
            other._kind = NoteKind.Slide;
            _prevLink = other;
            _nextLink = other._nextLink;
            if (_nextLink is not null)
                _nextLink._prevLink = this;
            other._nextLink = this;
        }

        /// <summary>
        /// Insert <paramref name="note"/> into another link before <paramref name="nextLink"/>,
        /// this method auto calls <see cref="UnlinkWithoutCutLinkChain(NoteData)"/> first
        /// </summary>
        public void InsertAsLinkBefore(NoteModel other)
        {
            if (IsSlide)
                UnlinkWithoutCutChain();

            _kind = NoteKind.Slide;
            other._kind = NoteKind.Slide;
            _nextLink = other;
            _prevLink = other._prevLink;
            if (_prevLink is not null)
                _prevLink._nextLink = this;
            other._prevLink = this;
        }

        public static void CloneLinkDatas(ReadOnlySpan<NoteModel> from, ReadOnlySpan<NoteModel> to)
        {
            Guard.HasSizeEqualTo(to, from.Length);

            using var dp_slideLookup = DictionaryPool<NoteModel, NoteModel>.Get(out var slideLookup);

            for (int i = 0; i < from.Length; i++) {
                var fromNote = from[i];
                var toNote = to[i];

                toNote.UnlinkWithoutCutChain(true);
                if (fromNote.IsSlide) {
                    slideLookup.Add(fromNote, toNote);

                    // GetPrevLink in from
                    var prevLink = fromNote.PrevLink;
                    NoteModel copiedPrev = default!;
                    while (prevLink is not null && !slideLookup.TryGetValue(prevLink, out copiedPrev)) {
                        prevLink = prevLink.PrevLink;
                    }

                    if (prevLink is not null) {
                        Debug.Assert(copiedPrev is not null);
                        toNote.InsertAsLinkAfter(copiedPrev!);
                    }
                }
            }
        }
    }
}