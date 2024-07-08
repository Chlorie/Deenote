using Cysharp.Threading.Tasks;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Localization;
using Deenote.Project;
using Deenote.Project.Models.Datas;
using Deenote.UI.MenuBar;
using Deenote.UI.StatusBar;
using Deenote.UI.Windows;
using UnityEngine;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System.Diagnostics;
#endif

namespace Deenote
{
    public sealed partial class MainSystem : MonoBehaviour
    {
        private static MainSystem _instance;

        [SerializeField] GlobalSettings _globalSettings;

        [Header("UI")]
        [SerializeField] MenuBarController _menuBarController;
        [SerializeField] StatusBarController _statusBarController;
        [SerializeField] PerspectiveViewWindow _perspectiveViewWindow;
        [SerializeField] EditorPropertiesWindow _editorPropertiesWindow;
        [SerializeField] PianoSoundEditWindow _pianoSoundEditWindow;

        [SerializeField] FileExplorerWindow _fileExplorerWindow;
        [SerializeField] ProjectPropertiesWindow _projectPropertiesWindow;
        [SerializeField] MessageBoxWindow _messageBoxWindow;

        [Header("System")]
        [SerializeField] LocalizationSystem _localizationSystem;
        [SerializeField] ProjectManager _projectManager;
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;
        [SerializeField] PianoSoundManager _pianoSoundManager;

        public static GlobalSettings GlobalSettings => _instance._globalSettings;

        public static MenuBarController MenuBar => _instance._menuBarController;
        public static StatusBarController StatusBar => _instance._statusBarController;
        public static PerspectiveViewWindow PerspectiveView => _instance._perspectiveViewWindow;
        public static EditorPropertiesWindow EditorProperties => _instance._editorPropertiesWindow;
        public static PianoSoundEditWindow PianoSoundEdit => _instance._pianoSoundEditWindow;

        public static FileExplorerWindow FileExplorer => _instance._fileExplorerWindow;
        public static ProjectPropertiesWindow ProjectProperties => _instance._projectPropertiesWindow;
        public static MessageBoxWindow MessageBox => _instance._messageBoxWindow;

        public static LocalizationSystem Localization => _instance._localizationSystem;
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

        [RuntimeInitializeOnLoadMethod]
        private static void _QuitConfirm()
        {
            Application.wantsToQuit += () =>
            {
                // TODO:
                return default;
            };
        }

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
            Process.GetCurrentProcess().Kill();
#endif
        }

        public static class Args
        {
            #region Value Clamp

            public const float StageMaxPosition = 2f;

            public static float ClampNoteTime(float time) => Mathf.Clamp(time, 0f, GameStage.MusicLength);

            public static float ClampNotePosition(float position) => Mathf.Clamp(position, -StageMaxPosition, StageMaxPosition);

            public static float ClampNoteSize(float size) => Mathf.Clamp(size, 0.1f, 5f);

            #endregion

            #region NoteCoord <-> World Position

            private const float PositionToXMultiplier = 1.63f;
            private const float TimeToZMultiplier = 10f / 3f;

            /// <summary>
            /// Time from note just appears(alpha=0) to complete display(alpha=1)
            /// </summary>
            public const float StageNoteFadeInTime = 0.5f;

            public static float NoteAppearZ => OffsetTimeToZ(GameStage.StageNoteAheadTime);

            public static float PositionToX(float position) => position * PositionToXMultiplier;

            public static float XToPosition(float x) => x / PositionToXMultiplier;

            /// <param name="offsetTimeToStage">
            /// The actualTime - stageCurrentTime
            /// </param>
            /// <returns></returns>
            public static float OffsetTimeToZ(float offsetTimeToStage) => offsetTimeToStage * TimeToZMultiplier * GameStage.NoteSpeed;

            public static float ZToOffsetTime(float z) => z / TimeToZMultiplier / GameStage.NoteSpeed;

            public static (float X, float Z) NoteCoordToWorldPosition(NoteCoord coord, float currentTime = 0f)
                => (PositionToX(coord.Position), OffsetTimeToZ(coord.Time - currentTime));

            #endregion

            public const float MaxBpm = 1200f;
            public const float MinBeatLineInterval = 60 / 1200f;

            private const float NoteTimeCollisionThreshold = 0.001f;
            private const float NotePositionCollisionThreshold = 0.01f;

            public static bool IsCollided(NoteData left, NoteData right)
            {
                return Mathf.Abs(left.Time - right.Time) <= NoteTimeCollisionThreshold
                    && Mathf.Abs(left.Position - right.Position) <= NotePositionCollisionThreshold;
            }

            #region Colors

            public static Color NoteSelectedColor => new(85f / 255f, 192f / 255f, 1f);

            public static Color NoteCollidedColor => new(1f, 85f / 255f, 85f / 255f);

            public static Color LinkLineColor => new(1f, 233f / 255f, 135f / 255f);

            public static Color SubBeatLineColor => new(42f / 255f, 42 / 255f, 42 / 255f, 0.75f);

            public static Color BeatLineColor => new(0.5f, 0f, 0f, 1f);

            public static Color TempoLineColor => new(0f, 0.5f, 0.5f, 1f);

            public static Color CurveLineColor => new(85f / 255, 192f / 255, 1f);

            #endregion

            public static readonly string[] SupportAudioFileExtensions = new[] { ".mp3", ".wav", };
            public static readonly string[] SupportProjectFileExtensions = new[] { "dnt", ".dsproj", };
            public static readonly string[] SupportChartFileExtensions = new[] { ".json", ".txt" };
        }
    }
}
