using Cysharp.Threading.Tasks;
using Deenote.Edit;
using Deenote.GameStage;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Windows;
using System.IO;
using UnityEngine;

namespace Deenote.Project
{
    public sealed partial class ProjectManager : MonoBehaviour
    {
        [Header("Notify")]
        [SerializeField] PropertiesWindow _propertiesWindow;
        [SerializeField] PerspectiveViewWindow _perspectiveViewWindow;
        [SerializeField] EditorController _editor;
        [SerializeField] GameStageController _stage;

        private string _currentProjectFileName;
        /// <summary>
        /// Is <see langword="null"/> if current project hasn't been saved.
        /// </summary>
        public string CurrentProjectSaveDirectory { get; private set; }

        public ProjectModel CurrentProject { get; private set; }

        private static readonly LocalizableText[] _newProjMsgBtnTxt = new[] {
            LocalizableText.Localized("Message_NewProjectOnOpen_Y"),
            LocalizableText.Localized("Message_NewProjectOnOpen_N"),
        };
        private static readonly LocalizableText[] _openProjMsgBtnTxt = new[] {
            LocalizableText.Localized("Message_OpenProjectOnOpen_Y"),
            LocalizableText.Localized("Message_OpenProjectOnOpen_N"),
        };
        private static readonly LocalizableText[] _openProjFailMsgBtnTxt = new[] {
            LocalizableText.Localized("Message_OpenProjectFailed_Y"),
            LocalizableText.Localized("Message_OpenProjectFailed_N"),
        };

        //private void Start()
        //{
        //    // TODO: Fake
        //    CurrentProject = Fake.Project;
        //    OnProjectChanged(0);
        //}

        private void Update()
        {
            UpdateAutoSave();
        }

        public async UniTaskVoid CreateNewProjectAsync()
        {
            if (CurrentProject is not null) {
                var clicked = await MainSystem.MessageBox.ShowAsync(
                    LocalizableText.Localized("Message_NewProjectOnOpen_Title"),
                    LocalizableText.Localized("Message_NewProjectOnOpen_Content"),
                    _newProjMsgBtnTxt);
                if (clicked != 0)
                    return;
            }

            var result = await MainSystem.ProjectProperties.OpenNewProjectAsync();
            if (result.IsCancelled)
                return;

            CurrentProjectSaveDirectory = null;
            CurrentProject = result.Project;
            OnProjectChanged(result.ConfirmedChartIndex);
            MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_NewProject_Completed"), 3f).Forget();
        }

        public async UniTaskVoid OpenProjectAsync()
        {
            if (CurrentProject is not null) {
                var clicked = await MainSystem.MessageBox.ShowAsync(
                 LocalizableText.Localized("Message_OpenProjectOnOpen_Title"),
                 LocalizableText.Localized("Message_OpenProjectOnOpen_Content"),
                 _openProjMsgBtnTxt);
                if (clicked != 0)
                    return;
            }

        SelectFile:
            var result = await MainSystem.FileExplorer.OpenSelectFileAsync(MainSystem.Args.SupportProjectFileExtensions);
            if (result.IsCancelled)
                return;

            MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_OpenProject_Loading"));
            var proj = await LoadAsync(result.Path);
            if (!proj.HasValue) {
                var clicked = await MainSystem.MessageBox.ShowAsync(
                    LocalizableText.Localized("Message_OpenProjectFailed_Title"),
                    LocalizableText.Localized("Message_OpenProjectFailed_Content"),
                    _openProjFailMsgBtnTxt);
                if (clicked != 0)
                    return;
                goto SelectFile;
            }

            var res = await MainSystem.ProjectProperties.OpenLoadProjectAsync(proj.Value);
            if (res.IsCancelled) {
                return;
            }

            CurrentProjectSaveDirectory = Path.GetDirectoryName(result.Path);
            _currentProjectFileName = Path.GetFileName(result.Path);
            CurrentProject = res.Project;
            OnProjectChanged(res.ConfirmedChartIndex);
            MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_OpenProject_Completed"), 3f).Forget();
        }

        public async UniTaskVoid SaveProjectAsync()
        {
            SavePlayerPrefs();

            if (CurrentProject is null) {
                Debug.Assert(false, "Save nothing");
                return;
            }

            if (CurrentProjectSaveDirectory is null) {
                await SaveAsInternalAsync();
                return;
            }
            else {
                MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_SaveProject_Saving"));
                await SaveAsync(CurrentProject, Path.Combine(CurrentProjectSaveDirectory, _currentProjectFileName));
                MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_SaveProject_Completed"), 3f).Forget();
            }
        }

        public async UniTaskVoid SaveAsAsync()
        {
            await SaveAsInternalAsync();
        }

        private async UniTask SaveAsInternalAsync()
        {
            if (CurrentProject is null)
                return;

            var res = await MainSystem.FileExplorer.OpenInputFileAsync(MainSystem.Args.DeenotePreferFileExtension);
            if (res.IsCancelled)
                return;

            if (CurrentProjectSaveDirectory is null && CurrentProject.SaveAsRefPath) {
                CurrentProjectSaveDirectory = Path.GetDirectoryName(res.Path);
                CurrentProject.AudioFileRelativePath = Path.GetRelativePath(CurrentProjectSaveDirectory, CurrentProject.AudioFileRelativePath);
                _currentProjectFileName = Path.GetFileName(res.Path);
            }
            else {
                CurrentProjectSaveDirectory = Path.GetDirectoryName(res.Path);
                _currentProjectFileName = Path.GetFileName(res.Path);
            }

            MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_SaveProject_Saving"));
            await SaveAsync(CurrentProject, res.Path);
            MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_SaveProject_Completed"), 3f).Forget();
        }


        private void SavePlayerPrefs() { }

        private void OnProjectChanged(int selectedChartIndex)
        {
            _editor.NotifyProjectChanged();
            _propertiesWindow.NotifyProjectChanged(CurrentProject);

            _stage.LoadChart(CurrentProject, selectedChartIndex);
            _perspectiveViewWindow.Window.IsActivated = true;
        }
    }
}