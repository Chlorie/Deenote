#nullable enable

using Deenote.Audio;
using Deenote.Core.Configurations;
using Deenote.Core.Inputting;
using Deenote.Editing;
using Deenote.GamePlay;
using Deenote.GamePlay.Stage;
using Deenote.Library.Components;
using Deenote.Project;
using System;
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
        [SerializeField] KeyBindingManager _keyBindingManager = default!;

        public static ConfigSerializer Configuration { get; private set; }=default!;
        public static GlobalSettings GlobalSettings { get; private set; } = default!;

        public static PianoSoundSource PianoSoundSource => Instance._pianoSoundSource;

        public static ProjectManager ProjectManager => Instance._projectManager;
        public static GamePlayManager GamePlayManager => Instance._gamePlayManager;
        public static StageChartEditor StageChartEditor => Instance._stageChartEditor;
        public static KeyBindingManager KeyBindingManager => Instance._keyBindingManager;

        protected override void Awake()
        {
            base.Awake();

            Configuration = new(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "config.json");
            GlobalSettings = new();

            StageChartEditor.OnInstantiate(ProjectManager, GamePlayManager);
        }

        private void Start()
        {
            Configuration.Load();
            _ = GameStageSceneLoader.LoadAsync("DeemoStage");
        }

        private void OnApplicationFocus(bool focus)
        {
            // TODO: make this configurable
            //if (!focus && GameStage.IsMusicPlaying) {
            //    GameStage.PauseStage();
            //}
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