#nullable enable

using Deenote.Core.GameStage;
using Deenote.Entities.Models;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        private void OnStageLoaded_Properties(GameStageSceneLoader loader)
        {
            loader.StageController.IsStageEffectOn = IsStageEffectOn;
            _cacheVisibleRangePercentage = null;
            loader.StageController.VisibleRangePercentage = VisibleRangePercentage;
        }

        private void RegisterConfigurations()
        {
            MainSystem.SaveSystem.SavingConfigurations += configs =>
            {
                configs.Add("stage/highlight_note_speed", HighlightedNoteSpeed);
                configs.Add("stage/apply_speed_diff", IsApplySpeedDifference);
                configs.Add("stage/filter_note_speed", IsFilterNoteSpeed);

                configs.Add("stage/note_speed", NoteFallSpeed);
                configs.Add("stage/show_link_lines", IsShowLinkLines);
                configs.Add("stage/piano_note_distinguish", IsPianoNotesDistinguished);
                configs.Add("stage/effect", IsStageEffectOn);
                configs.Add("stage/sudden_plus", SuddenPlus);
                configs.Add("stage/early_display_slow_notes", EarlyDisplaySlowNotes);
                configs.Add("stage/ignore_note_speed_property", IgnoreNoteSpeed);
                configs.Add("stage/pause_when_lose_focus", PauseWhenLoseFocus);

                configs.Add("stage/music_speed", MusicSpeed);
                configs.Add("stage/hitsound_volume", HitSoundVolume);
                configs.Add("stage/music_volume", MusicVolume);
                configs.Add("stage/piano_volume", PianoVolume);
            };
            MainSystem.SaveSystem.LoadedConfigurations += configs =>
            {
                HighlightedNoteSpeed = configs.GetSingle("stage/highlight_note_speed", 1f);
                IsApplySpeedDifference = configs.GetBoolean("stage/apply_speed_diff", true);
                IsFilterNoteSpeed = configs.GetBoolean("stage/filter_note_speed", false);

                NoteFallSpeed = configs.GetInt32("stage/note_speed", 40);
                IsShowLinkLines = configs.GetBoolean("stage/show_link_lines", true);
                IsPianoNotesDistinguished = configs.GetBoolean("stage/piano_note_distinguish", true);
                IsStageEffectOn = configs.GetBoolean("stage/sudden_plus", true);
                SuddenPlus = configs.GetSingle("stage/sudden_plus", 0f);
                EarlyDisplaySlowNotes = configs.GetBoolean("stage/early_display_slow_notes", false);
                IgnoreNoteSpeed = configs.GetBoolean("stage/ignore_note_speed_property", false);
                PauseWhenLoseFocus = configs.GetBoolean("stage/pause_when_lose_focus", true);

                MusicSpeed = configs.GetInt32("stage/music_speed", 10);
                HitSoundVolume = configs.GetSingle("stage/hitsound_volume", 0f);
                MusicVolume = configs.GetSingle("stage/music_volume", 100f);
                PianoVolume = configs.GetSingle("stage/piano_volume", 0f);
            };
        }

        private float _highlightedNoteSpeed_bf;
        private bool _applySpeedDifference_bf;
        private bool _filterNoteSpeed_bf;

        /// <summary>
        /// If <see cref="IsFilterNoteSpeed"/> is <see langword="true"/>,
        /// a downplayed note will not be selectable on stage.
        /// <br/>
        /// The value is also the default value when place note by editor
        /// </summary>
        public float HighlightedNoteSpeed
        {
            get => _highlightedNoteSpeed_bf;
            set {
                if (Utils.SetField(ref _highlightedNoteSpeed_bf, value)) {
                    if (IsChartLoaded() && IsStageLoaded()) {
                        foreach (var note in NotesManager.OnStageNotes) {
                            note.RefreshColorAlpha();
                        }
                    }
                    NotifyFlag(NotificationFlag.HighlightedNoteSpeed);
                }
            }
        }

        public bool IsApplySpeedDifference
        {
            get => _applySpeedDifference_bf;
            set {
                if (Utils.SetField(ref _applySpeedDifference_bf, value)) {
                    if (IsChartLoaded() && IsStageLoaded()) {
                        NotesManager.RefreshStageActiveNotes();
                    }
                    NotifyFlag(NotificationFlag.IsApplySpeedDifference);
                }
            }
        }

        public bool IsFilterNoteSpeed
        {
            get => _filterNoteSpeed_bf;
            set {
                if (Utils.SetField(ref _filterNoteSpeed_bf, value)) {
                    if (IsChartLoaded() && IsStageLoaded()) {
                        foreach (var note in NotesManager.OnStageNotes) {
                            note.RefreshColorAlpha();
                        }
                    }
                    NotifyFlag(NotificationFlag.IsFilterNoteSpeed);
                }
            }
        }

        public bool IsNoteHighlighted(NoteModel note)
        {
            return !IsFilterNoteSpeed || Mathf.Approximately(note.Speed, HighlightedNoteSpeed);
        }

        #region Stage

        public const int MinNoteSpeed = 5;
        public const int MaxNoteSpeed = 95;

        private int _noteSpeed_bf;
        private bool _showLinkLines_bf;
        private bool _isPianoNotesDistinguished_bf;
        private float _suddenPlus_bf;
        private bool _isStageEffectOn_bf;
        private bool _earlyDisplayLowSpeedNotes_bf;
        private bool _ignoreNoteSpeed_bf;
        private bool _pauseWhenLoseFocus_bf;

        /// <summary>
        /// Range [5, 95], display [0.5, 9.5]
        /// </summary>
        public int NoteFallSpeed
        {
            get => _noteSpeed_bf;
            set {
                value = Mathf.Clamp(value, MinNoteSpeed, MaxNoteSpeed);
                if (Utils.SetField(ref _noteSpeed_bf, value)) {
                    if (IsStageLoaded() && IsChartLoaded()) {
                        NotesManager.RefreshStageActiveNotes();
                    }
                    NotifyFlag(NotificationFlag.NoteSpeed);
                }
            }
        }

        public float ActualNoteFallSpeed => ConvertToActualNoteSpeed(NoteFallSpeed);

        public bool IsShowLinkLines
        {
            get => _showLinkLines_bf;
            set {
                if (Utils.SetField(ref _showLinkLines_bf, value)) {
                    if (IsStageLoaded() && IsChartLoaded()) {
                        foreach (var note in NotesManager.OnStageNotes) {
                            note.RefreshLinkLine();
                        }
                    }
                    NotifyFlag(NotificationFlag.IsShowLinkLines);
                }
            }
        }

        public bool IsPianoNotesDistinguished
        {
            get => _isPianoNotesDistinguished_bf;
            set {
                if (Utils.SetField(ref _isPianoNotesDistinguished_bf, value)) {
                    if (IsStageLoaded() && IsChartLoaded()) {
                        foreach (var note in NotesManager.OnStageNotes) {
                            note.RefreshVisual();
                        }
                    }
                    NotifyFlag(NotificationFlag.DistinguishPianoNotes);
                }
            }
        }

        public bool IsStageEffectOn
        {
            get => _isStageEffectOn_bf;
            set {
                if (Utils.SetField(ref _isStageEffectOn_bf, value)) {
                    if (IsStageLoaded()) {
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
                    if (IsStageLoaded() && IsChartLoaded()) {
                        foreach (var note in NotesManager.OnStageNotes) {
                            note.RefreshColorAlpha();
                        }
                    }
                    NotifyFlag(NotificationFlag.SuddenPlus);
                }
            }
        }

        private float? _cacheVisibleRangePercentage;

        public float VisibleRangePercentage
        {
            get {
                this.AssertStageLoaded();
                if (_cacheVisibleRangePercentage is null) {
                    var x = ConvertNoteCoordPositionToWorldX(0f);
                    var maxZ = ConvertNoteCoordTimeToWorldZ(StageNoteActiveAheadTime);
                    var minZ = ConvertNoteCoordTimeToWorldZ(0f);

                    bool try0, try1;
                    try0 = Stage.TryConvertNotePanelPositionToPerspectiveCameraViewportPoint((x, minZ), out var minVp);
                    try1 = Stage.TryConvertNotePanelPositionToPerspectiveCameraViewportPoint((x, maxZ), out var maxVp);
                    Debug.Assert(try0 && try1);

                    var vp = new Vector2(maxVp.x, Mathf.Lerp(maxVp.y, minVp.y, SuddenPlus));
                    try0 = Stage.TryConvertPerspectiveCameraViewportPointToNotePanelPosition(vp, out var pos);
                    Debug.Assert(try0);

                    _cacheVisibleRangePercentage = Mathf.InverseLerp(minZ, maxZ, pos.Z);
                }
                return _cacheVisibleRangePercentage.GetValueOrDefault();
            }
        }

        /// <remarks>
        /// In DEEMO II, if a low-speed notes is following a high-speed note, 
        /// the slow note will appear only when the fast one appeared.
        /// That means, the slow note will appear from the center of note panel.
        /// <br/>
        /// 
        /// </remarks>
        public bool EarlyDisplaySlowNotes
        {
            get => _earlyDisplayLowSpeedNotes_bf;
            set {
                if (Utils.SetField(ref _earlyDisplayLowSpeedNotes_bf, value)) {
                    if (IsStageLoaded() && IsChartLoaded()) {
                        foreach (var note in NotesManager.OnStageNotes) {
                            note.RefreshColorAlpha();
                        }
                    }
                    NotifyFlag(NotificationFlag.EarlyDisplaySlowNotes);
                }
            }
        }

        public bool IgnoreNoteSpeed
        {
            get => _ignoreNoteSpeed_bf;
            set {
                if (Utils.SetField(ref _ignoreNoteSpeed_bf, value)) {
                    if (IsStageLoaded() && IsChartLoaded()) {
                        NotesManager.RefreshStageActiveNotes();
                    }
                }
            }
        }

        public bool PauseWhenLoseFocus
        {
            get => _pauseWhenLoseFocus_bf;
            set {
                if (Utils.SetField(ref _pauseWhenLoseFocus_bf, value)) {
                    NotifyFlag(NotificationFlag.PauseWhenLoseFocus);
                }
            }
        }

        #endregion

        #region Audio

        public const int MinMusicSpeed = 1;
        public const int MaxMusicSpeed = 30;

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

        private static float ConvertToActualNoteSpeed(int noteSpeed) => noteSpeed / 10f;
        private static float ConvertToActualMusicSpeed(int musicSpeed) => musicSpeed / 10f;
    }
}