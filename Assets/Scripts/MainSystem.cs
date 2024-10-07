using Cysharp.Threading.Tasks;
using Deenote.ApplicationManaging;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Inputting;
using Deenote.Localization;
using Deenote.Project;
using Deenote.Project.Models.Datas;
using Deenote.UI.MenuBar;
using Deenote.UI.StatusBar;
using Deenote.UI.ToolBar;
using Deenote.UI.Windows;
using Deenote.Utilities;
using Reflex.Attributes;
using UnityEngine;

namespace Deenote
{
    public sealed partial class MainSystem : SingletonBehavior<MainSystem>
    {
        private readonly ResolutionAdjuster _resolutionAdjuster = new();

        [Header("UI")]
        [SerializeField] MenuBarController _menuBarController;
        [SerializeField] ToolBarController _toolBarController;
        [SerializeField] StatusBarController _statusBarController;

        [SerializeField] PerspectiveViewWindow _perspectiveViewWindow;
        [SerializeField] EditorPropertiesWindow _editorPropertiesWindow;
        [SerializeField] PianoSoundEditWindow _pianoSoundEditWindow;
        [SerializeField] PreferencesWindow _preferenceWindow;
        [SerializeField] PropertiesWindow _propertiesWindow;
        [SerializeField] AboutWindow _aboutWindow;

        [SerializeField] FileExplorerWindow _fileExplorerWindow;
        [SerializeField] ProjectPropertiesWindow _projectPropertiesWindow;
        [SerializeField] MessageBoxWindow _messageBoxWindow;

        [Header("System")]
        [SerializeField] InputController _inputController;
        [SerializeField] WindowsManager _windowsManager;

        [SerializeField] ProjectManager _projectManager;
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;
        [SerializeField] PianoSoundManager _pianoSoundManager;

        [Inject] private LocalizationSystem _localizationSystem = null!;

        public static ResolutionAdjuster ResolutionAdjuster => Instance._resolutionAdjuster;

        public static MenuBarController MenuBar => Instance._menuBarController;
        public static ToolBarController ToolBar => Instance._toolBarController;
        public static StatusBarController StatusBar => Instance._statusBarController;

        public static PerspectiveViewWindow PerspectiveView => Instance._perspectiveViewWindow;
        public static EditorPropertiesWindow EditorProperties => Instance._editorPropertiesWindow;
        public static PianoSoundEditWindow PianoSoundEdit => Instance._pianoSoundEditWindow;
        public static PreferencesWindow PreferenceWindow => Instance._preferenceWindow;
        public static PropertiesWindow PropertiesWindow => Instance._propertiesWindow;
        public static AboutWindow AboutWindow => Instance._aboutWindow;

        public static FileExplorerWindow FileExplorer => Instance._fileExplorerWindow;
        public static ProjectPropertiesWindow ProjectProperties => Instance._projectPropertiesWindow;
        public static MessageBoxWindow MessageBox => Instance._messageBoxWindow;

        public static LocalizationSystem Localization => Instance._localizationSystem;
        public static InputController Input => Instance._inputController;
        public static WindowsManager WindowsManager => Instance._windowsManager;

        public static ProjectManager ProjectManager => Instance._projectManager;
        public static GameStageController GameStage => Instance._gameStageController;
        public static EditorController Editor => Instance._editorController;
        public static PianoSoundManager PianoSoundManager => Instance._pianoSoundManager;

        private void OnApplicationFocus(bool focus)
        {
            // TODO: make this configurable
            if (!focus && GameStage.IsMusicPlaying) {
                GameStage.PauseStage();
            }
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        private static void _QuitConfirm()
        {
            Application.wantsToQuit += () => ConfirmQuitAsync().GetAwaiter().GetResult();
        }
#endif

        private static readonly LocalizableText[] _quitMessageButtons = {
            LocalizableText.Localized("Message_Quit_Y"), LocalizableText.Localized("Message_Quit_N"),
        };

        public static async UniTask<bool> ConfirmQuitAsync()
        {
            var res = await MessageBox.ShowAsync(
                LocalizableText.Localized("Message_Quit_Title"),
                LocalizableText.Localized("Message_Quit_Content"),
                _quitMessageButtons);
            return res == 0;
        }

        public static void QuitApplication()
        {
            // TODO: SaveConfig
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public static class Args
        {
            public const float NoteSelectionMaxPosition = 4f;

            #region Value Clamp

            public const float StageMaxPosition = 2f;

            public static float ClampNoteTime(float time) => Mathf.Clamp(time, 0f, GameStage.MusicLength);

            public static float ClampNotePosition(float position) =>
                Mathf.Clamp(position, -StageMaxPosition, StageMaxPosition);

            public static float ClampNoteSize(float size) => Mathf.Clamp(size, 0.1f, 5f);

            #endregion

            #region NoteCoord <-> World Position

            private const float PositionToXMultiplier = 1.63f;

            // private const float TimeToZMultiplier = 10f / 3f;
            public static float NoteAppearZ => OffsetTimeToZ(GameStage.StageNoteAheadTime);

            public static float PositionToX(float position) => position * PositionToXMultiplier;

            public static float XToPosition(float x) => x / PositionToXMultiplier;

            /// <param name="offsetTimeToStage">
            /// The actualTime - stageCurrentTime
            /// </param>
            /// <returns></returns>
            public static float OffsetTimeToZ(float offsetTimeToStage) =>
                offsetTimeToStage * GameStage.Args.NoteTimeToZMultiplier * GameStage.NoteSpeed;

            public static float ZToOffsetTime(float z) =>
                z / GameStage.Args.NoteTimeToZMultiplier / GameStage.NoteSpeed;

            public static (float X, float Z) NoteCoordToWorldPosition(NoteCoord coord, float currentTime = 0f)
                => (PositionToX(coord.Position), OffsetTimeToZ(coord.Time - currentTime));

            public static float TimeToHoldScaleY(float time)
                => time * GameStage.Args.HoldSpritePrefab.NotePanelZToYMultiplier * GameStage.NoteSpeed;

            #endregion

            #region NoteTimeAdjusting

            // NOTE: This change is for speed property of note, currently has no use

            // TODO：NoteModel基于这个值排序，
            public static float NoteActualAppearTime(float time, float noteSpeed) => time - GameStage.StageNoteAheadTime / noteSpeed;

            // 这个时间主要用于显示
            // 形象地说，Note在出现之前以1速下落，出现之后以NoteSpeed速度下落，在判定线以下以1速下落
            // 基于这个PseudoTime排列，Note能够以其在界面上的位置排序
            public static float NoteTimeToPseudoTime(float time, float noteSpeed)
            {
                var currentTime = GameStage.CurrentMusicTime;
                if (time <= currentTime)
                    return time;
                else if (time < currentTime + GameStage.StageNoteAheadTime)
                    return (time - currentTime) * noteSpeed;
                else
                    return (time - currentTime) - GameStage.StageNoteAheadTime / noteSpeed;
            }

            #endregion

            public const float MaxBpm = 1200f;
            public const float MinBeatLineInterval = 60 / 1200f;

            private const float NoteTimeCollisionThreshold = 0.001f;
            private const float NotePositionCollisionThreshold = 0.01f;

            public static bool IsTimeCollided(NoteData left, NoteData right) =>
                Mathf.Abs(right.Time - left.Time) <= NoteTimeCollisionThreshold;

            public static bool IsPositionCollided(NoteData left, NoteData right) =>
                Mathf.Abs(right.Position - left.Position) <= NotePositionCollisionThreshold;

            public static bool IsCollided(NoteData left, NoteData right) =>
                IsTimeCollided(left, right) && IsPositionCollided(left, right);

            #region Colors

            public static Color NoteSelectedColor => new(85f / 255f, 192f / 255f, 1f);

            public static Color NoteCollidedColor => new(1f, 85f / 255f, 85f / 255f);

            public static Color LinkLineColor => new(1f, 233f / 255f, 135f / 255f);

            public static Color SubBeatLineColor => new(42f / 255f, 42 / 255f, 42 / 255f, 0.75f);

            public static Color BeatLineColor => new(0.5f, 0f, 0f, 1f);

            public static Color TempoLineColor => new(0f, 0.5f, 0.5f, 1f);

            public static Color CurveLineColor => new(85f / 255, 192f / 255, 1f);

            #endregion

            #region Grids

            public const float GridWidth = 2f;
            public const float GridBorderWidth = 4f;

            #endregion

            public const string DeenotePreferFileExtension = ".dnt";
            public const ushort DeenoteProjectFileHeader = 0xDEE0;
            public const byte DeenoteProjectFileVersionMark = 1;

            public static readonly string[] SupportAudioFileExtensions = { ".mp3", ".wav", };
            public static readonly string[] SupportProjectFileExtensions = { DeenotePreferFileExtension, ".dsproj", };
            public static readonly string[] SupportChartFileExtensions = { ".json", ".txt" };
        }
    }
}