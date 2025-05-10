#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Entities.Storage;
using Deenote.Library;
using Deenote.Library.Components;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deenote.Core.Project
{
    public sealed partial class ProjectManager : FlagNotifiableMonoBehaviour<ProjectManager, ProjectManager.NotificationFlag>
    {
        private ProjectModel? _currentProject_bf;
        public ProjectModel? CurrentProject
        {
            get => _currentProject_bf;
        }

        private AudioClip? _audioClip;
        public AudioClip? AudioClip => _audioClip;

        private bool _isLoading_bf;
        private bool _isSaving_bf;
        private ResetableCancellationTokenSource _saveCts = new();
        private ResetableCancellationTokenSource _saveChartsCts = new();

        public bool IsLoading
        {
            get => _isLoading_bf;
            private set {
                if (Utils.SetField(ref _isLoading_bf, value)) {
                    NotifyFlag(NotificationFlag.IsLoading);
                }
            }
        }

        public bool IsSaving
        {
            get => _isSaving_bf;
            private set {
                if (Utils.SetField(ref _isSaving_bf, value)) {
                    NotifyFlag(NotificationFlag.IsSaving);
                }
            }
        }

        private void Awake()
        {
            RegisterAutoSaveConfigurations();
        }

#if  UNITY_EDITOR
        private async void Start()
        {
            var (proj, clip) = await Fake.GetProject();
            SetCurrentProject(proj, clip);
        }
#endif

        public void SetCurrentProject(ProjectModel project, AudioClip audio)
        {
            if (Utils.SetField(ref _currentProject_bf, project)) {
                _audioClip = audio;
                if (project is not null)
                    project.AudioLength = audio.length;
                NotifyFlag(NotificationFlag.CurrentProject);
            }
        }

        public async UniTask<bool> OpenLoadProjectFileAsync(string filePath)
        {
            using var loadingScope = new LoadingScope(this);

            var proj = await ProjectIO.LoadAsync(filePath);
            if (proj is null)
                return false;

            using var ms = new MemoryStream(proj.AudioFileData);
            var clip = await AudioUtils.TryLoadAsync(ms, Path.GetExtension(proj.AudioFileRelativePath));
            if (clip is null)
                return false;

            SetCurrentProject(proj, clip);
            return true;
        }

        public void UnloadCurrentProject()
        {
            SetCurrentProject(null!, null!);
        }

        public async UniTask SaveCurrentProjectAsync()
        {
            ValidateProject();

            _saveCts.Reset();
            await SaveCurrentProjectToAsyncInternal(CurrentProject.ProjectFilePath, _saveCts.Token);
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.Project));
        }

        public async UniTask SaveCurrentProjectToAsync(string targetFilePath)
        {
            ValidateProject();

            _saveCts.Reset();
            await SaveCurrentProjectToAsyncInternal(targetFilePath, _saveCts.Token);
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.Project));
        }

        private async UniTask SaveCurrentProjectToAsyncInternal(string targetFilePath, CancellationToken cancellationToken)
        {
            using var scope = new SavingScope(this);

            AssertProjectLoaded();
            await ProjectIO.SaveAsync(CurrentProject, targetFilePath, cancellationToken);
        }

        public async UniTask SaveCurrentProjectChartJsonsAsync()
        {
            ValidateProject();
            await SaveCurrentProjectChartJsonsToAsyncInternal(Path.GetDirectoryName(CurrentProject.ProjectFilePath));
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.ChartJsons));
        }

        public async UniTask SaveCurrentProjectChartJsonsToAsync(string targetDirectory)
        {
            ValidateProject();
            await SaveCurrentProjectChartJsonsToAsyncInternal(targetDirectory);
            ProjectSaved?.Invoke(new ProjectSaveEventArgs(ProjectSaveContents.ChartJsons));
        }

        private async UniTask SaveCurrentProjectChartJsonsToAsyncInternal(string targetDirectory)
        {
            AssertProjectLoaded();

            _saveChartsCts.Reset();

            var time = DateTime.Now;
            string dir = Path.Combine(Path.GetDirectoryName(CurrentProject.ProjectFilePath), AutoSaveJsonDirName);
            string filename = Path.GetFileNameWithoutExtension(CurrentProject.ProjectFilePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var tasks = new Task[CurrentProject.Charts.Count];
            for (int i = 0; i < CurrentProject.Charts.Count; i++) {
                ChartModel? chart = CurrentProject.Charts[i];
                var chartname = string.IsNullOrEmpty(chart.Name) ? chart.Difficulty.ToLowerCaseString() : chart.Name;

                tasks[i] = File.WriteAllTextAsync(
                    Path.Combine(dir, $"{filename}.{chartname}.{time:yyMMddHHmmss}.json"),
                    chart.ToJsonString(), _saveChartsCts.Token);
            }

            await Task.WhenAll(tasks);
        }

        #region Validation

        [MemberNotNull(nameof(CurrentProject))]
        private void ValidateProject()
        {
            if (CurrentProject is null)
                throw new InvalidOperationException("No project loaded.");
        }

        /// <summary>
        /// The method do <c>UnityEngine.Debug.Assert()</c>, and could make IDE
        /// provide a better nullable diagnostic in context
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        [MemberNotNull(nameof(CurrentProject))]
        public void AssertProjectLoaded(string message = "Project not loaded")
#pragma warning disable CS8774
            => Debug.Assert(CurrentProject is not null, message);
#pragma warning restore CS8774

        [MemberNotNullWhen(true, nameof(CurrentProject))]
        public bool IsProjectLoaded() => CurrentProject is not null;

        #endregion

        public enum NotificationFlag
        {
            IsLoading,
            IsSaving,

            AutoSave,
            AutoSaveInterval,

            CurrentProject,
            ProjectAudio,
            ProjectMusicName,
            ProjectComposer,
            ProjectChartDesigner,
            ProjectCharts,
        }

        private readonly struct LoadingScope : IDisposable
        {
            private readonly ProjectManager _self;

            public LoadingScope(ProjectManager self)
            {
                _self = self;
                _self.IsLoading = true;
            }

            public void Dispose()
            {
                _self.IsLoading = false;
            }
        }

        private readonly struct SavingScope : IDisposable
        {
            private readonly ProjectManager _self;

            public SavingScope(ProjectManager self)
            {
                _self = self;
                _self.IsSaving = true;
            }

            public void Dispose()
            {
                _self.IsSaving = false;
            }
        }
    }
}