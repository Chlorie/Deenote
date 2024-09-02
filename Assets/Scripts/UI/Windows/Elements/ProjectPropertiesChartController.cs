using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.UI.Windows.Components;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    public sealed class ProjectPropertiesChartController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] TMP_InputField _nameInputField;
        [SerializeField] WindowDropdown _difficultyDropDown;
        [SerializeField] TMP_InputField _levelInputField;
        [SerializeField] TMP_Text _notesCountText;
        [SerializeField] Button _loadButton;
        [SerializeField] Button _importButton;
        [SerializeField] LocalizedText _importText;
        [SerializeField] Button _exportButton;
        [SerializeField] LocalizedText _exportText;
        [SerializeField] Button _removeButton;
        [SerializeField] LocalizedText _removeText;

        private ProjectPropertiesWindow _window;
        private ChartModel __chartModel;
        private bool _isRemoveConfirming;

        public Button LoadButton => _loadButton;

        public ChartModel Chart => __chartModel;

        public void OnCreated(ProjectPropertiesWindow window)
        {
            _window = window;
            _difficultyDropDown.ResetOptions(DifficultyExt.DropdownOptions);
        }

        public void Initialize(ChartModel? chart)
        {
            __chartModel = chart ?? new ChartModel(new ChartData()) {
                Name = "", Difficulty = Difficulty.Hard, Level = "1",
            };

            _nameInputField.text = __chartModel.Name;
            _difficultyDropDown.Dropdown.SetValueWithoutNotify(__chartModel.Difficulty.ToInt32());
            _levelInputField.text = __chartModel.Level;
            _notesCountText.text = $"{__chartModel.Notes.Count} Note{(__chartModel.Notes.Count == 1 ? null : "s")}";

            _importButton.interactable = true;
            _importText.SetLocalizedText("Window_ProjectProperties_ChartImport");
            _exportButton.interactable = true;
            _exportText.SetLocalizedText("Window_ProjectProperties_ChartExport");
            _removeText.SetLocalizedText("Window_ProjectProperties_ChartRemove");
            _isRemoveConfirming = false;
        }

        private void Awake()
        {
            _nameInputField.onEndEdit.AddListener(val => Chart.Name = val);
            _levelInputField.onEndEdit.AddListener(val => Chart.Level = val);
            _difficultyDropDown.Dropdown.onValueChanged.AddListener(val =>
                Chart.Difficulty = DifficultyExt.FromInt32(val));

            _loadButton.onClick.AddListener(() => _window.SelectChartToLoad(this));
            _importButton.onClick.AddListener(ImportChartAsync);
            _exportButton.onClick.AddListener(ExportChartAsync);
            _removeButton.onClick.AddListener(() =>
            {
                if (_isRemoveConfirming) {
                    _window.RemoveChart(this);
                }
                else {
                    _isRemoveConfirming = true;
                    _removeText.SetLocalizedText("Window_ProjectProperties_ChartRemoveConfirm");
                }
            });
        }

        #region UIEvents

        private async UniTaskVoid ImportChartAsync()
        {
            var res = await MainSystem.FileExplorer.OpenSelectFileAsync(MainSystem.Args.SupportChartFileExtensions);
            if (res.IsCancelled)
                return;

            _importText.SetLocalizedText("Window_ProjectProperties_ChartImporting");
            _importButton.interactable = false;

            if (!ChartData.TryLoad(await File.ReadAllTextAsync(res.Path), out var chartData)) {
                _importText.SetLocalizedText("Window_ProjectProperties_ChartImport");
                var arg = Path.GetFileName(res.Path);
                MainSystem.StatusBar.SetStatusMessage(LocalizableText.Localized("Status_ImportChart_Failed"),
                    MemoryMarshal.CreateReadOnlySpan(ref arg, 1));
                _importButton.interactable = true;
                return;
            }

            Chart.Data = chartData;
            _notesCountText.text = $"{__chartModel.Notes.Count} Note{(__chartModel.Notes.Count == 1 ? null : "s")}";
            _importText.SetLocalizedText("Window_ProjectProperties_ChartImported");
            _importButton.interactable = true;
        }

        private async UniTaskVoid ExportChartAsync()
        {
            var res = await MainSystem.FileExplorer.OpenSelectDirectoryAsync();
            if (res.IsCancelled)
                return;

            _exportText.SetLocalizedText("Window_ProjectProperties_ChartExporting");
            _exportButton.interactable = false;

            await File.WriteAllTextAsync(res.Path, Chart.Data.ToJsonString());

            _exportText.SetLocalizedText("Window_ProjectProperties_ChartExported");
            _exportButton.interactable = true;
        }

        #endregion
    }
}