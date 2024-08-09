using Deenote.Settings;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        [Header("Args")]
        [SerializeField] GameStageArgs _args;
        public GameStageArgs Args => _args;

        [Header("Settings")]
        [SerializeField] private bool __isStageEffectOn;
        public bool IsStageEffectOn
        {
            get => __isStageEffectOn;
            set {
                if (__isStageEffectOn == value)
                    return;
                __isStageEffectOn = value;
                if (__isStageEffectOn is false)
                    StopStageEffect();
                MainSystem.PreferenceWindow.NotifyIsStageEffectOnChanged(__isStageEffectOn);
            }
        }

        [SerializeField, Range(1, 19)] private int __noteSpeed;
        /// <summary>
        /// Range [1, 19], display [0.5, 9.5]
        /// </summary>
        public int NoteSpeed
        {
            get => __noteSpeed;
            set {
                value = Mathf.Clamp(value, 1, 19);

                if (__noteSpeed == value)
                    return;
                __noteSpeed = value;
                UpdateStageNotes();
                ForceUpdateNotesDisplay();
                _editorPropertiesWindow.NotifyNoteSpeedChanged(__noteSpeed);
            }
        }

        [SerializeField, Range(1, 30)] private int __musicSpeed;
        /// <summary>
        /// Range [1, 30], display [0.1, 3.0]
        /// </summary>
        public int MusicSpeed
        {
            get => __musicSpeed;
            set {
                value = Mathf.Clamp(value, 1, 30);

                if (__musicSpeed == value)
                    return;
                __musicSpeed = value;
                _musicSource.pitch = value / 10f;
                _editorPropertiesWindow.NotifyMusicSpeedChanged(__musicSpeed);
            }
        }

        [SerializeField, Range(0, 100)] private int __effectVolume;
        /// <summary>
        /// Range [0, 100]
        /// </summary>
        public int EffectVolume
        {
            get => __effectVolume;
            set {
                value = Mathf.Clamp(value, 0, 100);

                if (__effectVolume == value)
                    return;
                __effectVolume = value;
                _editorPropertiesWindow.NotifyEffectVolumeChanged(__effectVolume);
            }
        }

        [SerializeField, Range(0, 100)] private int __musicVolume;
        /// <summary>
        /// Range [0, 100]
        /// </summary>
        public int MusicVolume
        {
            get => __musicVolume;
            set {
                value = Mathf.Clamp(value, 0, 100);

                if (__musicVolume == value)
                    return;
                __musicVolume = value;
                _musicSource.volume = __musicVolume / 100f;
                _editorPropertiesWindow.NotifyMusicVolumeChanged(__musicVolume);
            }
        }

        [SerializeField, Range(0, 100)] private int __pianoVolume;
        /// <summary>
        /// Range [0, 100]
        /// </summary>
        public int PianoVolume
        {
            get => __pianoVolume;
            set {
                value = Mathf.Clamp(value, 0, 100);

                if (__pianoVolume == value)
                    return;
                __pianoVolume = value;
                _editorPropertiesWindow.NotifyPianoVolumeChanged(__pianoVolume);
            }
        }

        [SerializeField, Range(0f, 100f)] private int __suddenPlusRange;
        /// <summary>
        /// Range [0, 100], means percent
        /// </summary>
        public int SuddenPlusRange
        {
            get => __suddenPlusRange;
            set {
                value = Mathf.Clamp(value, 0, 100);

                if (__suddenPlusRange == value)
                    return;
                __suddenPlusRange = value;
                UpdateStageNotes();
                ForceUpdateNotesDisplay();
                _gridController.UpdateVerticalGrids();
                _editorPropertiesWindow.NotifySuddenPlusRangeChanged(__suddenPlusRange);
            }
        }

        private bool __isPianoNotesDistinguished;
        public bool IsPianoNotesDistinguished
        {
            get => __isPianoNotesDistinguished;
            set {
                if (__isPianoNotesDistinguished == value)
                    return;
                __isPianoNotesDistinguished = value;
                ForceUpdateStageNotes(false, true);
                MainSystem.PreferenceWindow.NotifyIsPianoNoteDistinguished(__isPianoNotesDistinguished);
            }
        }
    }
}