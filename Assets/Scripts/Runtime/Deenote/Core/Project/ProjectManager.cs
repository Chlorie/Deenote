#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Entities.Storage;
using Deenote.Library;
using Deenote.Library.Components;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
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
            set {
                if (Utils.SetField(ref _currentProject_bf, value)) {
                    NotifyFlag(NotificationFlag.CurrentProject);
                }
            }
        }

        private bool _isLoading;
        private bool _isSaving;
        private ResetableCancellationTokenSource _saveCts = new();

        public bool IsLoading
        {
            get => _isLoading;
            set {
                if (Utils.SetField(ref _isLoading, value)) {
                    NotifyFlag(NotificationFlag.IsLoading);
                }
            }
        }

        private void Awake()
        {
            MainSystem.Instance.AutoSaveTrigger.AutoSaving += AutoSaveHandler;
        }

        private void OnDestroy()
        {
            MainSystem.Instance.AutoSaveTrigger.AutoSaving -= AutoSaveHandler;
        }

        private void Start()
        {

            // TODO: Fake
            CurrentProject = Fake.GetProject();
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
            // HACK: should set audioclip in entity.csproj
            proj.AudioClip = clip;

            CurrentProject = proj;
            return true;
        }

        public UniTask SaveCurrentProjectAsync()
        {
            if (!IsProjectLoaded())
                return UniTask.CompletedTask;
            _saveCts.Reset();
            return SaveCurrentProjectToAsyncInternal(CurrentProject.ProjectFilePath, _saveCts.Token);
        }

        public UniTask SaveCurrentProjectToAsync(string targetFilePath)
        {
            if (!IsProjectLoaded())
                return UniTask.CompletedTask;
            _saveCts.Reset();
            return SaveCurrentProjectToAsyncInternal(targetFilePath, _saveCts.Token);
        }

        private async UniTask SaveCurrentProjectToAsyncInternal(string targetFilePath, CancellationToken cancellationToken)
        {
            AssertProjectLoaded();

            _isSaving = true;
            try {
                await ProjectIO.SaveAsync(CurrentProject, targetFilePath, cancellationToken);
            } finally {
                _isSaving = false;
            }
        }

        private async UniTask SaveCurrentProjectChartJsonsAsync(CancellationToken cancellationToken)
        {
            AssertProjectLoaded();

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
                    chart.ToJsonString(), cancellationToken);
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
            AutoSave,

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
    }
}