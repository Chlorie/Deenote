using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using System.Diagnostics;

namespace Deenote.UI.Windows
{
    partial class ProjectPropertiesWindow
    {
        private UniTaskCompletionSource<MayBeNull<ProjectPropertiesChartController>> _newProjTcs;

        /// <summary>
        /// Open window for creating a new project
        /// </summary>
        /// <returns></returns>
        public async UniTask<Result> OpenNewProjectAsync()
        {
            Window.IsActivated = true;

            _window.SetTitle(LocalizableText.Localized("WindowTitleBar_ProjectProperties_Create"));

            _newProjTcs = new UniTaskCompletionSource<MayBeNull<ProjectPropertiesChartController>>();
            var confirmedChart = await _newProjTcs.Task;
            _newProjTcs = null;

            Window.IsActivated = false;
            if (!confirmedChart.HasValue)
                return default;

            var proj = new ProjectModel {
                MusicName = _musicNameInputField.text,
                Composer = _composerInputField.text,
                ChartDesigner = _chartDesignerInputField.text,
                AudioData = _loadedBytes,
                AudioClip = _loadedClip,
                // TODO: SaveByRefPath?
            };

            int loadChartIndex = -1;
            for (int i = 0; i < _charts.Count; i++) {
                var ch = _charts[i];
                proj.Charts.Add(ch.Chart);
                if (confirmedChart.Value == ch)
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

            _newProjTcs = new UniTaskCompletionSource<MayBeNull<ProjectPropertiesChartController>>();
            var confirmedChart = await _newProjTcs.Task;
            _newProjTcs = null;

            Window.IsActivated = false;
            if (!confirmedChart.HasValue)
                return default;

            int loadChartIndex = _charts.IndexOf(confirmedChart.Value);
            return new Result(project, loadChartIndex);
        }

        public readonly struct Result
        {
            private readonly ProjectModel _project;
            private readonly int _chart;

            public ProjectModel Project => _project;

            public int ConfirmedChartIndex => _chart;

            public bool IsCancelled => _project is null;

            public Result(ProjectModel project, int chartIndex)
            {
                _project = project;
                _chart = chartIndex;
            }
        }
    }
}