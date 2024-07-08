using Cysharp.Threading.Tasks;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    public sealed class ProjectPropertiesChartController : MonoBehaviour
    {

        [Header("UI")]
        [SerializeField] TMP_InputField _nameInputField;
        [SerializeField] TMP_InputField _levelInputField;
        [SerializeField] TMP_Dropdown _difficultyDropDown;
        [SerializeField] Button _loadButton;
        [SerializeField] Button _importButton;
        [SerializeField] Button _exportButton;
        [SerializeField] Button _removeButton;
        [SerializeField] TMP_Text _removeText;

        private ProjectPropertiesWindow _window;

        private bool _isMainChart;

        private ChartModel __chartModel;

        public string FilePath;

        public Button LoadButton => _loadButton;

        public ChartModel Chart => __chartModel;

        public void OnCreated(ProjectPropertiesWindow window)
        {
            _window = window;
        }

        public void Initialize(ChartModel chart, bool isMainChart)
        {
            __chartModel = chart ?? new(new()) {
                Name = "",
                Difficulty = Difficulty.Hard,
                Level = "",
            };

            _nameInputField.text = __chartModel.Name;
            _levelInputField.text = __chartModel.Level;
            _difficultyDropDown.value = __chartModel.Difficulty.ToInt32();
            _removeText.text = isMainChart ? "+" : "-";

            _isMainChart = isMainChart;
        }

        private void Awake()
        {
            _nameInputField.onSubmit.AddListener(val => Chart.Name = val);
            _levelInputField.onSubmit.AddListener(val => Chart.Level = val);
            _difficultyDropDown.onValueChanged.AddListener(val => Chart.Difficulty = DifficultyExt.FromInt32(val));

            _loadButton.onClick.AddListener(() => _window.SelectChartToLoad(this));
            _importButton.onClick.AddListener(ImportChartAsync);
            _exportButton.onClick.AddListener(ExportChartAsync);
            _removeButton.onClick.AddListener(() =>
            {
                if (_isMainChart)
                    _window.AddNewChart();
                else
                    _window.RemoveChart(this);
            });
        }

        #region UIEvents

        private async UniTaskVoid ImportChartAsync()
        {
            var res = await MainSystem.FileExplorer.OpenSelectFileAsync(MainSystem.Args.SupportChartFileExtensions);
            if (res.IsCancelled)
                return;

            // TODO: Loading message

            if (!ChartData.TryLoad(await File.ReadAllTextAsync(res.Path), out var chartData)) {
                // TODO: Load chart failed
                return;
            }

            Chart.Data = chartData;
        }

        private async UniTaskVoid ExportChartAsync()
        {
            var res = await MainSystem.FileExplorer.OpenSelectDirectoryAsync();
            if (res.IsCancelled)
                return;

            // TODO: Exporting message
            await File.WriteAllTextAsync(res.Path, Chart.Data.ToJsonString());
            // TODO: Exported message
        }

        #endregion
    }
}