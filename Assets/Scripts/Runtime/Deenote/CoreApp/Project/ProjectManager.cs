#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Entities.Models;
using Deenote.Entities.Storage;
using Deenote.Library;
using Deenote.Library.Components;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Deenote.CoreApp.Project
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

        private void Awake()
        {
            MainSystem.SaveSystem.AutoSaving += AutoSaveHandler;
        }

        private void OnDestroy()
        {
            MainSystem.SaveSystem.AutoSaving -= AutoSaveHandler;
        }

        private void Start()
        {

            // TODO: Fake
            CurrentProject = Fake.GetProject();
        }

        public async UniTask<bool> OpenLoadProjectFileAsync(string filePath)
        {
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

        public UniTask SaveCurrentProjectAsync(CancellationToken cancellationToken = default)
        {
            if (CurrentProject is null)
                return UniTask.CompletedTask;
            return SaveCurrentProjectToAsyncInternal(CurrentProject.ProjectFilePath, cancellationToken);
        }

        public UniTask SaveCurrentProjectToAsync(string targetFilePath, CancellationToken cancellationToken = default)
        {
            if (CurrentProject is null)
                return UniTask.CompletedTask;
            return SaveCurrentProjectToAsyncInternal(targetFilePath, cancellationToken);
        }

        private async UniTask SaveCurrentProjectToAsyncInternal(string targetFilePath, CancellationToken cancellationToken)
        {
            // TODO: 可能要考虑一下多次点击导致的冲突
            Debug.Assert(CurrentProject is not null);
            var proj = CurrentProject!;

            await ProjectIO.SaveAsync(proj, targetFilePath, cancellationToken);
            // HACK should set in entity.csproj
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
        public void AssertProjectLoaded()
#pragma warning disable CS8774
            => Debug.Assert(CurrentProject is not null, "Project not loaded");
#pragma warning restore CS8774

        [MemberNotNullWhen(true, nameof(CurrentProject))]
        public bool IsProjectLoaded() => CurrentProject is not null;

        #endregion

        public enum NotificationFlag
        {
            AutoSave,

            CurrentProject,
            ProjectAudio,
            ProjectMusicName,
            ProjectComposer,
            ProjectChartDesigner,
            ProjectCharts,
            ProjectTempos,
        }
    }
}