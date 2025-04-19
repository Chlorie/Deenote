#nullable enable

using Deenote.Audio;
using Deenote.Core;
using Deenote.Core.Editing;
using Deenote.Core.GamePlay;
using Deenote.Core.GameStage;
using Deenote.Core.Project;
using Deenote.Library.Components;
using System.Collections.Immutable;
using UnityEngine;

namespace Deenote
{
    public sealed partial class MainSystem : SingletonBehaviour<MainSystem>
    {
        [Header("System")]
        [SerializeField] PianoSoundSource _pianoSoundSource = default!;
        [Header("Manager")]
        [SerializeField] ProjectManager _projectManager = default!;
        [SerializeField] GamePlayManager _gamePlayManager = default!;
        [SerializeField] StageChartEditor _stageChartEditor = default!;

        private UnhandledExceptionHandler _unhandledExceptionHandler = default!;

        public static SaveSystem SaveSystem { get; private set; } = default!;
        public static GlobalSettings GlobalSettings { get; private set; } = default!;

        public static PianoSoundSource PianoSoundSource => Instance._pianoSoundSource;

        public static ProjectManager ProjectManager => Instance._projectManager;
        public static GamePlayManager GamePlayManager => Instance._gamePlayManager;
        public static StageChartEditor StageChartEditor => Instance._stageChartEditor;

        protected override void Awake()
        {
            base.Awake();

            _unhandledExceptionHandler = new();

            SaveSystem = new();

            GlobalSettings = new();

            StageChartEditor.OnInstantiate(ProjectManager, GamePlayManager);
        }

        private void Start()
        {
            SaveSystem.LoadConfigurations();
            _ = GameStageSceneLoader.LoadAsync("DeemoStage");
        }

        public static partial class Args
        {
            public const string DeenotePreferFileExtension = ".dnt";
            public const string DeenotePreferChartExtension = ".json";

            public static readonly ImmutableArray<string> SupportLoadProjectFileExtensions
                = ImmutableArray.Create(DeenotePreferFileExtension, ".dsproj");
            public static readonly ImmutableArray<string> SupportLoadAudioFileExtensions
                = ImmutableArray.Create(".mp3", ".wav");
            public static readonly ImmutableArray<string> SupportLoadChartFileExtensions
                = ImmutableArray.Create(".json", ".txt");
        }
    }
}