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
using UnityEngine;

namespace Deenote
{
    public sealed partial class MainSystem : MonoBehaviour
    {
        private static MainSystem _instance;

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
        [SerializeField] LocalizationSystem _localizationSystem;
        [SerializeField] UnhandledExceptionHandler _unhandledExceptionHandler;
        [SerializeField] VersionManager _versionManager;
        [SerializeField] InputController _inputController;
        [SerializeField] WindowsManager _windowsManager;

        [SerializeField] ProjectManager _projectManager;
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;
        [SerializeField] PianoSoundManager _pianoSoundManager;

        public static ResolutionAdjuster ResolutionAdjuster => _instance._resolutionAdjuster;

        public static MenuBarController MenuBar => _instance._menuBarController;
        public static ToolBarController ToolBar => _instance._toolBarController;
        public static StatusBarController StatusBar => _instance._statusBarController;

        public static PerspectiveViewWindow PerspectiveView => _instance._perspectiveViewWindow;
        public static EditorPropertiesWindow EditorProperties => _instance._editorPropertiesWindow;
        public static PianoSoundEditWindow PianoSoundEdit => _instance._pianoSoundEditWindow;
        public static PreferencesWindow PreferenceWindow => _instance._preferenceWindow;
        public static PropertiesWindow PropertiesWindow => _instance._propertiesWindow;
        public static AboutWindow AboutWindow => _instance._aboutWindow;

        public static FileExplorerWindow FileExplorer => _instance._fileExplorerWindow;
        public static ProjectPropertiesWindow ProjectProperties => _instance._projectPropertiesWindow;
        public static MessageBoxWindow MessageBox => _instance._messageBoxWindow;

        public static LocalizationSystem Localization => _instance._localizationSystem;
        public static UnhandledExceptionHandler ExceptionHandler => _instance._unhandledExceptionHandler;
        public static VersionManager VersionManager => _instance._versionManager;
        public static InputController Input => _instance._inputController;
        public static WindowsManager WindowsManager => _instance._windowsManager;

        public static ProjectManager ProjectManager => _instance._projectManager;
        public static GameStageController GameStage => _instance._gameStageController;
        public static EditorController Editor => _instance._editorController;
        public static PianoSoundManager PianoSoundManager => _instance._pianoSoundManager;

        /// <remarks>
        /// This method invokes before all other custom Awake methods
        /// </remarks>
        private void Awake()
        {
            if (_instance != null)
                throw new System.InvalidOperationException($"Only 1 instance of {nameof(MainSystem)} may exist");
            _instance = this;
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus) {
                if (GameStage.IsMusicPlaying)
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

        private static readonly LocalizableText[] _quitMessageButtons = new[] {
            LocalizableText.Localized("Message_Quit_Y"),
            LocalizableText.Localized("Message_Quit_N"),
        };

        public static async UniTask<bool> ConfirmQuitAsync()
        {
            var res = await MessageBox.ShowAsync(
                LocalizableText.Localized("Message_Quit_Title"),
                LocalizableText.Localized("Message_Quit_Content"),
                _quitMessageButtons);
            if (res != 0) {
                return false;
            }
            return true;
        }

        public static void QuitApplication()
        {
            // TODO:SavePlayerPrefs
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
             System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
        }

        public static class Args
        {
            public const float NoteSelectionMaxPosition = 4f;

            #region Value Clamp

            public const float StageMaxPosition = 2f;

            public static float ClampNoteTime(float time) => Mathf.Clamp(time, 0f, GameStage.MusicLength);

            public static float ClampNotePosition(float position) => Mathf.Clamp(position, -StageMaxPosition, StageMaxPosition);

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
            public static float OffsetTimeToZ(float offsetTimeToStage) => offsetTimeToStage * GameStage.Args.NoteTimeToZMultiplier * GameStage.NoteSpeed;

            public static float ZToOffsetTime(float z) => z / GameStage.Args.NoteTimeToZMultiplier / GameStage.NoteSpeed;

            public static (float X, float Z) NoteCoordToWorldPosition(NoteCoord coord, float currentTime = 0f)
                => (PositionToX(coord.Position), OffsetTimeToZ(coord.Time - currentTime));

            #endregion

            public const float MaxBpm = 1200f;
            public const float MinBeatLineInterval = 60 / 1200f;

            private const float NoteTimeCollisionThreshold = 0.001f;
            private const float NotePositionCollisionThreshold = 0.01f;

            public static bool IsTimeCollided(NoteData left, NoteData right) => Mathf.Abs(right.Time - left.Time) <= NoteTimeCollisionThreshold;

            public static bool IsPositionCollided(NoteData left, NoteData right) => Mathf.Abs(right.Position - left.Position) <= NotePositionCollisionThreshold;

            public static bool IsCollided(NoteData left, NoteData right) => IsTimeCollided(left, right) && IsPositionCollided(left, right);

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

            public const float GridWidth = 0.002f;
            public const float GridBorderWidth = 0.004f;

            #endregion

            public const string DeenotePreferFileExtension = ".dnt";
            public const ushort DeenoteProjectFileHeader = 0xDEE0;
            public const byte DeenoteProjectFileVersionMark = 1;
            public const string DeenoteCurrentVersion = "1.0";

            public static readonly string[] SupportAudioFileExtensions = new[] { ".mp3", ".wav", };
            public static readonly string[] SupportProjectFileExtensions = new[] { DeenotePreferFileExtension, ".dsproj", };
            public static readonly string[] SupportChartFileExtensions = new[] { ".json", ".txt" };
        }
    }
}
