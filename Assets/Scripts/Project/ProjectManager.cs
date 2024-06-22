using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using UnityEngine;

namespace Deenote.Project
{
    public sealed partial class ProjectManager : MonoBehaviour
    {
        private string _projectSavePath;

        public ProjectModel CurrentProject { get; private set; }


        private static readonly LocalizableText[] _newProjMsgBtnTxt = new[] {
            LocalizableText.Localized("Message_NewProjectOnOpen_Y"),
            LocalizableText.Localized("Message_NewProjectOnOpen_N"),
        };
        private static readonly string[] _supportProjFileExts = new[] { ".dsproj", ".dnt" };

        private void Awake()
        {
            CurrentProject = Fake.Project;
        }

        public async UniTaskVoid NewProjectAsync()
        {
            var result = await MainSystem.ProjectProperties.OpenNewProjectAsync();
            if (result.IsCancelled)
                return;

            if (CurrentProject is not null) {
                var clicked = await MainSystem.MessageBox.ShowAsync(
                    LocalizableText.Localized("Message_NewProjectOnOpen_Title"),
                    LocalizableText.Localized("Message_NewProjectOnOpen_Content"),
                    _newProjMsgBtnTxt);
                if (clicked != 0)
                    return;
            }

            CurrentProject = result.Project;
            MainSystem.GameStage.LoadChart(result.Project, result.ConfirmedChartIndex);
            MainSystem.PerspectiveView.gameObject.SetActive(true);
            MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_NewProject_Completed"), 3f).Forget();
        }

        public async UniTaskVoid OpenProjectAsync()
        {
            var result = await MainSystem.FileExplorer.OpenSelectFileAsync(_supportProjFileExts);
            if (result.IsCancelled)
                return;

            // stage.forceToPlaceNotes = true;
            MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_OpenProject_Loading"));
            var proj = await LoadAsync(result.Path);
            if (!proj.HasValue) {
                // TODO: Open failed
                return;
            }

            CurrentProject = proj.Value;
            _projectSavePath = result.Path;
            MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_OpenProject_Completed"), 3f).Forget();
            // stage.forceToPlaceNotes = false;
        }

        public async UniTaskVoid SaveProjectAsync()
        {
            SavePlayerPrefs();

            if (CurrentProject is null)
                return;
            // TODO: �����漰������ĵ������ٿ���

            if (_projectSavePath is null) {
                await SaveAsInternalAsync();
                return;
            }
            else {
                MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_SaveProject_Saving"));
                await SaveAsync(CurrentProject, _projectSavePath);
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

            var res = await MainSystem.FileExplorer.OpenSelectDirectoryAsync();
            if (res.IsCancelled)
                return;

            // TODO: �����漰������ĵ������ٿ���2

            _projectSavePath = res.Path;
            MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_SaveProject_Saving"));
            await SaveAsync(CurrentProject, _projectSavePath);
            MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_SaveProject_Completed"), 3f).Forget();
        }


        private void SavePlayerPrefs() { }
    }
}