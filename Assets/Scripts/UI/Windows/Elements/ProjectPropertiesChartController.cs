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
        private static readonly string[] _supportChartExtensions = { ".json", ".txt" };

        [Header("UI")]
        [SerializeField] TMP_InputField _nameInputField;
        [SerializeField] TMP_InputField _levelInputField;
        [SerializeField] TMP_Dropdown _difficultyDropDown;
        [SerializeField] Button _loadButton;
        [SerializeField] Button _importButton;
        [SerializeField] Button _exportButton;
        [SerializeField] Button _removeButton;
        [SerializeField] TMP_Text _removeText;

        private UnityAction<ProjectPropertiesChartController> _onLoad;
        private UnityAction<ProjectPropertiesChartController> _onRemove;

        private ChartData _chartData;

        public string FilePath;

        public Button LoadButton => _loadButton;

        public void Initialize(UnityAction<ProjectPropertiesChartController> onLoad, UnityAction<ProjectPropertiesChartController> onRemove)
        {
            _onLoad = onLoad;
            _onRemove = onRemove;

            _nameInputField.text = "";
            _levelInputField.text = "";
            //_difficultyDropDown
            _removeText.text = "-";

            _loadButton.onClick.AddListener(() => _onLoad(this));
            _removeButton.onClick.AddListener(() => _onRemove(this));
        }

        public void InitializeMain(UnityAction onLoad, UnityAction onAdd)
        {
            _nameInputField.text = "";
            _levelInputField.text = "";
            //_difficultyDropDown
            _removeText.text = "+";
            _loadButton.onClick.AddListener(onLoad);
            _removeButton.onClick.AddListener(onAdd);
        }

        public ChartModel BuildChart()
        {
            var chart = new ChartModel(_chartData ?? new()) {
                Name = _nameInputField.text,
                Difficulty = DifficultyExt.FromInt32(_difficultyDropDown.value),
                Level = _levelInputField.text,
            };
            return chart;
        }

        private void Awake()
        {
            _importButton.onClick.AddListener(ImportChartAsync);
            _exportButton.onClick.AddListener(ExportChartAsync);
        }

        #region UIEvents

        private async UniTaskVoid ImportChartAsync()
        {
            var res = await MainSystem.FileExplorer.OpenSelectFileAsync(_supportChartExtensions);
            if (res.IsCancelled)
                return;

            // TODO: Loading message

            if (!ChartData.TryLoad(await File.ReadAllTextAsync(res.Path), out var chartData)) {
                // TODO: Load chart failed
                return;
            }

            _chartData = chartData;
        }

        private async UniTaskVoid ExportChartAsync()
        {
            if (_chartData is null)
                return;

            var res = await MainSystem.FileExplorer.OpenSelectDirectoryAsync();
            if (res.IsCancelled)
                return;

            // TODO: Exporting message
            await File.WriteAllTextAsync(res.Path, _chartData.ToJsonString());
            // TODO: Exported message
        }

        #endregion

        private void OnDisable()
        {
            _onRemove = null;
            _removeButton.onClick.RemoveAllListeners();
        }
    }
}