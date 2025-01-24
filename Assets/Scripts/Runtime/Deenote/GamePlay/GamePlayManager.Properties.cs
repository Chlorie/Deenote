#nullable enable

using Deenote.GamePlay.Stage;
using Deenote.Library;
using UnityEngine;

namespace Deenote.GamePlay
{
    partial class GamePlayManager
    {
        private void OnStageLoaded_Properties(GameStageSceneLoader loader)
        {
            loader.StageController.IsStageEffectOn = IsStageEffectOn;
            _cacheVisibleRangePercentage = null;
            loader.StageController.VisibleRangePercentage = VisibleRangePercentage;
        }

        #region Stage

        private int _noteSpeed_bf;
        private bool _showLinkLines_bf;
        private bool _isPianoNotesDistinguished_bf;
        private float _suddenPlus_bf;
        private bool _isStageEffectOn_bf;

        /// <summary>
        /// Range [1, 99], display [0.1, 9.9]
        /// </summary>
        public int NoteSpeed
        {
            get => _noteSpeed_bf;
            set {
                value = Mathf.Clamp(value, MinNoteSpeed, MaxNoteSpeed);
                if (Utils.SetField(ref _noteSpeed_bf, value)) {
                    if (CurrentChart is not null && Stage is not null) {
                        UpdateActiveNotes();
                    }
                    NotifyFlag(NotificationFlag.NoteSpeed);
                }
            }
        }

        public float ActualNoteSpeed => ConvertToActualNoteSpeed(NoteSpeed);

        public bool IsShowLinkLines
        {
            get => _showLinkLines_bf;
            set {
                if (Utils.SetField(ref _showLinkLines_bf, value)) {
                    RefreshNotesTimeState(); // This updates link line
                    NotifyFlag(NotificationFlag.IsShowLinkLines);
                }
            }
        }

        public bool IsPianoNotesDistinguished
        {
            get => _isPianoNotesDistinguished_bf;
            set {
                if (Utils.SetField(ref _isPianoNotesDistinguished_bf, value)) {
                    RefreshNotesVisual();
                    NotifyFlag(NotificationFlag.DistinguishPianoNotes);
                }
            }
        }

        public bool IsStageEffectOn
        {
            get => _isStageEffectOn_bf;
            set {
                if (Utils.SetField(ref _isStageEffectOn_bf, value)) {
                    if (Stage is not null) {
                        Stage.IsStageEffectOn = value;
                    }
                    NotifyFlag(NotificationFlag.StageEffectOn);
                }
            }
        }

        /// <summary>
        /// Range [0, 1]
        /// </summary>
        public float SuddenPlus
        {
            get => _suddenPlus_bf;
            set {
                value = Mathf.Clamp(value, 0f, 1f);
                if (Utils.SetField(ref _suddenPlus_bf, value)) {
                    _cacheVisibleRangePercentage = null;
                    RefreshNotesTimeState();
                    NotifyFlag(NotificationFlag.SuddenPlus);
                }
            }
        }

        private float? _cacheVisibleRangePercentage;

        public float VisibleRangePercentage
        {
            get {
                this.AssertStageLoaded();
                _cacheVisibleRangePercentage ??= this.StagePerspectiveCamera.SuddenPlusRangeToVisibleRangePercentage(SuddenPlus);
                return _cacheVisibleRangePercentage.GetValueOrDefault();
            }
        }

        #endregion

        #region Audio

        // The field is required as we may restore it when manual-play mode off
        private int _musicSpeed_bf;

        /// <summary>
        /// Range [1, 30], representing [0.1, 3.0]
        /// </summary>
        public int MusicSpeed
        {
            get => _musicSpeed_bf;
            set {
                value = Mathf.Clamp(value, MinMusicSpeed, MaxMusicSpeed);
                if (Utils.SetField(ref _musicSpeed_bf, value)) {
                    var actualVal = ConvertToActualMusicSpeed(value);
                    MusicPlayer.Pitch = actualVal;
                    PianoSoundPlayer.Speed = actualVal;
                    NotifyFlag(NotificationFlag.MusicSpeed);
                }
            }
        }

        public float ActualMusicSpeed => ConvertToActualMusicSpeed(MusicSpeed);

        /// <summary>
        /// Range [0,1]
        /// </summary>
        public float HitSoundVolume
        {
            get => HitSoundPlayer.Volume;
            set {
                value = Mathf.Clamp(value, 0f, 1f);
                if (HitSoundPlayer.Volume != value) {
                    HitSoundPlayer.Volume = value;
                    NotifyFlag(NotificationFlag.HitSoundVolume);
                }
            }
        }

        /// <summary>
        /// Range [0,1]
        /// </summary>
        public float MusicVolume
        {
            get => MusicPlayer.Volume;
            set {
                value = Mathf.Clamp(value, 0f, 1f);
                if (MusicPlayer.Volume != value) {
                    MusicPlayer.Volume = value;
                    NotifyFlag(NotificationFlag.MusicVolume);
                }
            }
        }

        /// <summary>
        /// Range [0,1]
        /// </summary>
        public float PianoVolume
        {
            get => PianoSoundPlayer.Volume;
            set {
                value = Mathf.Clamp(value, 0f, 1f);
                if (PianoSoundPlayer.Volume != value) {
                    PianoSoundPlayer.Volume = value;
                    NotifyFlag(NotificationFlag.PianoVolume);
                }
            }
        }

        #endregion
    }
}