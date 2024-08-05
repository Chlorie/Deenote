using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Windows.Elements;
using System.Diagnostics;

namespace Deenote.UI.Windows
{
    partial class ProjectPropertiesWindow
    {
        private UniTaskCompletionSource<ProjectPropertiesChartController?>? _newProjTcs;

        /// <summary>
        /// Open window for creating a new project
        /// </summary>
        /// <returns></returns>
        public async UniTask<Result> OpenNewProjectAsync()
        {
            Window.IsActivated = true;

            _window.SetTitle(LocalizableText.Localized("WindowTitleBar_ProjectProperties_Create"));

            _newProjTcs = new UniTaskCompletionSource<ProjectPropertiesChartController?>();
            var confirmedChart = await _newProjTcs.Task;
            _newProjTcs = null;

            Window.IsActivated = false;
            if (confirmedChart is null)
                return default;

            var proj = new ProjectModel
            {
                MusicName = _musicNameInputField.text,
                Composer = _composerInputField.text,
                ChartDesigner = _chartDesignerInputField.text,
                AudioData = _loadedBytes,
                AudioClip = _loadedClip,
                // TODO: SaveByRefPath?
            };

            int loadChartIndex = -1;
            for (int i = 0; i < _charts.Count; i++)
            {
                var ch = _charts[i];
                proj.Charts.Add(ch.Chart);
                if (confirmedChart == ch)
                    loadChartIndex = i;
            }
            Debug.Assert(loadChartIndex >= 0);
            return new Result(proj, loadChartIndex);
        }

        /// <summary>
        /// Open window for existing project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public async UniTask<Result> OpenLoadProjectAsync(ProjectModel project)
        {
            Window.IsActivated = true;

            _window.SetTitle(LocalizableText.Localized("WindowTitleBar_ProjectProperties"));
            InitializeProject(project);

            _newProjTcs = new UniTaskCompletionSource<ProjectPropertiesChartController?>();
            var confirmedChart = await _newProjTcs.Task;
            _newProjTcs = null;

            Window.IsActivated = false;
            if (confirmedChart is null)
                return default;

            int loadChartIndex = _charts.IndexOf(confirmedChart);
            return new Result(project, loadChartIndex);
        }

        public readonly struct Result
        {
            public ProjectModel Project { get; }
            public int ConfirmedChartIndex { get; }
            public bool IsCancelled => Project is null;

            public Result(ProjectModel project, int chartIndex)
            {
                Project = project;
                ConfirmedChartIndex = chartIndex;
            }
        }
    }
}