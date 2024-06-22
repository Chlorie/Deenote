using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Localization;
using Deenote.Project;
using Deenote.UI.MenuBar;
using Deenote.UI.StatusBar;
using Deenote.UI.Windows;
using UnityEngine;

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
        [SerializeField] FileExplorerWindow _fileExplorerWindow;
        [SerializeField] ProjectPropertiesWindow _projectPropertiesWindow;
        [SerializeField] MessageBoxWindow _messageBoxWindow;

        [Header("System")]
        [SerializeField] LocalizationSystem _localizationSystem;
        [SerializeField] ProjectManager _projectManager;
        [SerializeField] GameStageController _gameStageController;
        [SerializeField] EditorController _editorController;

        public static GlobalSettings GlobalSettings => _instance._globalSettings;

        public static MenuBarController MenuBar => _instance._menuBarController;
        public static StatusBarController StatusBar => _instance._statusBarController;
        public static PerspectiveViewWindow PerspectiveView => _instance._perspectiveViewWindow;
        public static EditorPropertiesWindow EditorProperties => _instance._editorPropertiesWindow;
        public static FileExplorerWindow FileExplorer => _instance._fileExplorerWindow;
        public static ProjectPropertiesWindow ProjectProperties => _instance._projectPropertiesWindow;
        public static MessageBoxWindow MessageBox => _instance._messageBoxWindow;

        public static LocalizationSystem Localization => _instance._localizationSystem;
        public static ProjectManager ProjectManager => _instance._projectManager;
        public static GameStageController GameStage => _instance._gameStageController;
        public static EditorController Editor => _instance._editorController;

        /// <remarks>
        /// This method invokes before all other custom Awake methods
        /// </remarks>
        private void Awake()
        {
            if (_instance != null)
                throw new System.InvalidOperationException($"Only 1 instance of {nameof(MainSystem)} may exist");
            _instance = this;
        }

        public static class Args
        {
            private const float PositionToXMultiplier = 1.63f;
            private const float TimeToZMultiplier = 10f / 3f;

            public const float StageMaxPosition = 2f;

            /// <summary>
            /// Time from note just appears(alpha=0) to complete display(alpha=1)
            /// </summary>
            public const float StageNoteFadeInTime = 0.5f;

            public const float SideLineX = 2f * PositionToXMultiplier;

            public static float NoteAppearZ => OffsetTimeToZ(GameStage.StageNoteAheadTime);

            public static float PositionToX(float position) => position * PositionToXMultiplier;

            public static float XToPosition(float x) => x / PositionToXMultiplier;

            /// <param name="offsetTime">
            /// Offset time means the actualTime - stageCurrentTime
            /// </param>
            /// <returns></returns>
            public static float OffsetTimeToZ(float offsetTime) => offsetTime * TimeToZMultiplier * GameStage.NoteSpeed;

            public static float ZToOffsetTime(float z) => z / TimeToZMultiplier / GameStage.NoteSpeed;

            public static (float X, float Z) NoteCoordToWorldPosition(NoteCoord coord, float currentTime)
                => (PositionToX(coord.Position), OffsetTimeToZ(coord.Time - currentTime));
        }
    }
}
