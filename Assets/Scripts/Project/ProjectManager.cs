using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Windows;
using UnityEngine;

namespace Deenote.Project
{
    public sealed partial class ProjectManager : MonoBehaviour
    {
        [Header("Notify")]
        [SerializeField] PropertiesWindow _propertiesWindow;

        private string? _projectSavePath;
        public string CurrentProjectSavePath => _projectSavePath;

        public ProjectModel? CurrentProject { get; private set; }

        private static readonly LocalizableText[] _newProjMsgBtnTxt = {
            LocalizableText.Localized("Message_NewProjectOnOpen_Y"),
            LocalizableText.Localized("Message_NewProjectOnOpen_N"),
        };

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
            var result = await MainSystem.FileExplorer.OpenSelectFileAsync(MainSystem.Args.SupportProjectFileExtensions);
            if (result.IsCancelled)
                return;

            // stage.forceToPlaceNotes = true;
            MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_OpenProject_Loading"));
            var proj = await LoadAsync(result.Path);
            if (proj is null) {
                // TODO: Open failed
                return;
            }

            // TODO:Load AudioClip
            CurrentProject = proj;
            _projectSavePath = result.Path;
            MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_OpenProject_Completed"), 3f).Forget();
            // stage.forceToPlaceNotes = false;
        }

        public async UniTaskVoid SaveProjectAsync()
        {
            SavePlayerPrefs();

            if (CurrentProject is null)
                return;
            // TODO: 好像涉及到谱面的调整，再看看

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

            // TODO: 好像涉及到谱面的调整，再看看2

            _projectSavePath = res.Path;
            MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_SaveProject_Saving"));
            await SaveAsync(CurrentProject, _projectSavePath);
            MainSystem.StatusBar.SetStatusMessageAsync(LocalizableText.Localized("Status_SaveProject_Completed"), 3f).Forget();
        }

        private void SavePlayerPrefs() { }
    }
}