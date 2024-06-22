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

        public void SetTaskResult(ProjectPropertiesChartController chartController)
        {
            _newProjTcs.TrySetResult(chartController);
        }

        public async UniTask<ProjectCreationResult> OpenNewProjectAsync()
        {
            gameObject.SetActive(true);

            _window.SetTitle(LocalizableText.Localized("WindowTitleBar_ProjectProperties_Create"));

            _newProjTcs = new UniTaskCompletionSource<MayBeNull<ProjectPropertiesChartController>>();
            var confirmedChart = await _newProjTcs.Task;
            _newProjTcs = null;

            gameObject.SetActive(false);
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
                proj.Charts.Add(ch.BuildChart());
                if (confirmedChart.Value == ch)
                    loadChartIndex = i;
            }
            Debug.Assert(loadChartIndex >= 0);
            return new ProjectCreationResult(proj, loadChartIndex);
        }

        public readonly struct ProjectCreationResult
        {
            private readonly ProjectModel _project;
            private readonly int _chart;

            public ProjectModel Project => _project;

            public int ConfirmedChartIndex => _chart;

            public bool IsCancelled => _project is null;

            public ProjectCreationResult(ProjectModel project, int chartIndex)
            {
                _project = project;
                _chart = chartIndex;
            }
        }
    }
}