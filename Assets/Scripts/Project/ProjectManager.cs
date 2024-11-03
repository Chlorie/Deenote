#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Project.Models;
using Deenote.Utilities;
using System.IO;
using UnityEngine;

namespace Deenote.Project
{
    public sealed partial class ProjectManager : MonoBehaviour
    {
        private ProjectModel? __currentProject;
        /// <summary>
        /// Maybe null on loading project
        /// </summary>
        public ProjectModel CurrentProject
        {
            get => __currentProject!;
            set {
                if (__currentProject == value)
                    return;
                __currentProject = value;
                _propertyChangeNotifier.Invoke(this, NotifyProperty.CurrentProject);
            }
        }

        private void Start()
        {
            // TODO: Fake
            CurrentProject = Fake.GetProject();
        }

        private void Update()
        {
            Update_AutoSave();
        }

        /// <returns>Is project opened</returns>
        public async UniTask<bool> OpenLoadProjectFileAsync(string filePath)
        {
            ProjectModel? proj = await LoadAsync(filePath);
            if (proj is null)
                return false;

            if (proj.SaveAsRefPath) {
                throw new System.NotImplementedException();
            }
            else {
                using var ms = new MemoryStream(proj.AudioFileData);
                var clip = await AudioUtils.LoadAsync(ms, Path.GetExtension(proj.AudioFileRelativePath));
                if (clip is null)
                    return false;
                ProjectModel.InitializationHelper.SetAudioClip(proj, clip);
            }

            CurrentProject = proj;
            return true;
        }

        public UniTask SaveCurrentProjectAsync()
            => SaveCurrentProjectToAsync(CurrentProject.ProjectFilePath);

        public async UniTask SaveCurrentProjectToAsync(string targetFilePath)
        {
            if (CurrentProject is null)
                return;

            // TODO: Handle AudioFileRelativePath ?

            //if (CurrentProjectSaveDirectory is null && CurrentProject.SaveAsRefPath) {
            //    CurrentProject.AudioFileRelativePath = Path.GetRelativePath(
            //        CurrentProjectSaveDirectory, CurrentProject.AudioFileRelativePath);
            //}
            //else {
            //}

            await SaveAsync(CurrentProject, targetFilePath);
            ProjectModel.InitializationHelper.SetProjectFilePath(CurrentProject, targetFilePath);
        }
    }
}