#nullable enable

using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using UnityEngine;

namespace Deenote.Core.Editing
{
    partial class StageNotePlacer
    {
        private Transform? _indicatorPanelTransform;

        private bool _placeSoundNoteByDefault_bf;
        private bool _isIndicatorOn_bf;
        private bool _snapToPositionGrid_bf;
        private bool _snapToTimeGrid_bf;
        private float? _placingNoteSpeed;

        private bool _placeSlideModifier_bf;
        private bool _pasteRememberPosition_bf;

        public bool PlaceSoundNoteByDefault
        {
            get => _placeSoundNoteByDefault_bf;
            set {
                if (Utils.SetField(ref _placeSoundNoteByDefault_bf, value)) {
                    if (value) {
                        _metaPrototype.Sounds.Replace(StageChartEditor._defaultNoteSounds);
                    }
                    else {
                        _metaPrototype.Sounds.Clear();
                    }
                    if (!IsFreezeNotePrototypeProperties()) {
                        foreach (var note in _prototypes) {
                            note.Sounds.Replace(_metaPrototype.Sounds.AsSpan());
                        }
                        foreach (var indicator in _indicators) {
                            indicator.Refresh();
                        }
                    }
                    NotifyFlag(NotificationFlag.PlaceSoundNoteByDefault);
                }
            }
        }

        public bool IsIndicatorOn
        {
            get => _isIndicatorOn_bf;
            set {
                if (Utils.SetField(ref _isIndicatorOn_bf, value)) {
                    RefreshIndicatorVisibility();
                    NotifyFlag(NotificationFlag.IsIndicatorOn);
                }
            }
        }

        public bool SnapToPositionGrid
        {
            get => _snapToPositionGrid_bf;
            set {
                if (Utils.SetField(ref _snapToPositionGrid_bf, value)) {
                    NotifyFlag(NotificationFlag.SnapToPositionGrid);
                }
            }
        }

        public bool SnapToTimeGrid
        {
            get => _snapToTimeGrid_bf;
            set {
                if (Utils.SetField(ref _snapToTimeGrid_bf, value)) {
                    NotifyFlag(NotificationFlag.SnapToTimeGrid);
                }
            }
        }

        public float PlacingNoteSpeed
        {
            get => _placingNoteSpeed ?? _editor._game.HighlightedNoteSpeed;
        }

        private void SetPlacingNoteSpeed(float? value, bool forceUpdateAndNotify)
        {
            var prev = PlacingNoteSpeed;
            if (Utils.SetField(ref _placingNoteSpeed, value) || forceUpdateAndNotify) {
                if (!IsFreezeNotePrototypeProperties()) {
                    var speed = PlacingNoteSpeed;
                    if (speed != prev) {
                        _metaPrototype.Speed = speed;
                        if (_indicators is not null) {
                            foreach (var indicator in _indicators) {
                                indicator.NotePrototype.Speed = speed;
                                indicator.Refresh();
                            }
                        }
                    }
                }
                NotifyFlag(NotificationFlag.PlacingNoteSpeed);
            }
        }

        public bool PlaceSlideModifier
        {
            get => _placeSlideModifier_bf;
            set {
                if (Utils.SetField(ref _placeSlideModifier_bf, value)) {
                    _metaPrototype.Kind = value ? NoteModel.NoteKind.Slide : NoteModel.NoteKind.Click;
                    SwitchPlaceSlideModifier(value);
                }
            }
        }

        public bool PasteRememberPositionModifier
        {
            get => _pasteRememberPosition_bf;
            set {
                if (Utils.SetField(ref _pasteRememberPosition_bf, value)) {

                }
            }
        }

        private void RefreshIndicatorVisibility()
        {
            if (_indicatorPanelTransform == null)
                return;

            if (IsForceShowIndicator() || IsIndicatorOn) {
                _indicatorPanelTransform.gameObject.SetActive(true);
            }
            else {
                _indicatorPanelTransform.gameObject.SetActive(false);
            }
        }

        private void SetIndicatorVisibility(bool visible)
        {
            if (_indicatorPanelTransform != null) {
                _indicatorPanelTransform.gameObject.SetActive(visible);
            }
        }
    }
}