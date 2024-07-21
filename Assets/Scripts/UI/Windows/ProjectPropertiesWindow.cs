using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class ProjectPropertiesWindow : MonoBehaviour
    {
        [SerializeField] Window _window;
        public Window Window => _window;

        [Header("UI")]
        [SerializeField] Button _audioFileButton;
        [SerializeField] LocalizedText _audioFileText;
        [SerializeField] TMP_InputField _musicNameInputField;
        [SerializeField] TMP_InputField _composerInputField;
        [SerializeField] TMP_InputField _chartDesignerInputField;
        [SerializeField] Transform _chartsParentTransform;

        [Header("Prefabs")]
        [SerializeField] ProjectPropertiesChartController _chartPrefab;

        private PooledObjectListView<ProjectPropertiesChartController> _charts;

        private byte[] _loadedBytes;
        private string _loadedAudioFilePath;
        private AudioClip _loadedClip;

        private void Awake()
        {
            _charts = new PooledObjectListView<ProjectPropertiesChartController>(UnityUtils.CreateObjectPool(() =>
            {
                var item = Instantiate(_chartPrefab, _chartsParentTransform);
                item.OnCreated(this);
                return item;
            }, 1));

            _audioFileButton.onClick.AddListener(LoadAudioFileAsync);

            _window.SetOnIsActivatedChanged(activated =>
            {
                if (!activated) {
                    _newProjTcs?.TrySetResult(null);
                }
            });
        }

        private void SetChartLoadable(bool value)
        {
            if (_charts[0].LoadButton.interactable == value)
                return;

            foreach (var chart in _charts) {
                chart.LoadButton.interactable = value;
            }
        }

        private void InitializeProject(ProjectModel project)
        {
            if (project is null) {
                _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad");
                _musicNameInputField.text = "";
                _composerInputField.text = "";
                _chartDesignerInputField.text = "";
                _audioFileText.TmpText.alignment = TextAlignmentOptions.CenterGeoAligned;

                _charts.SetCount(1);
                _charts[0].Initialize(null, true);
                SetChartLoadable(false);
            }
            else {
                if (project.SaveAsRefPath) {
                    _audioFileText.SetRawText(Path.GetFileName(project.AudioFileRelativePath));
                    _audioFileText.TmpText.alignment = TextAlignmentOptions.BaselineLeft;
                }
                else {
                    _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad_Embeded");
                    _audioFileText.TmpText.alignment = TextAlignmentOptions.CenterGeoAligned;
                }
                _musicNameInputField.text = project.MusicName;
                _composerInputField.text = project.Composer;
                _chartDesignerInputField.text = project.ChartDesigner;

                _charts.SetCount(project.Charts.Count);
                _charts[0].Initialize(project.Charts[0], true);
                for (int i = 1; i < _charts.Count; i++) {
                    _charts[i].Initialize(project.Charts[i], true);
                }
                _charts.SetSiblingIndicesInOrder();
                SetChartLoadable(true);
            }
        }

        #region UI Events

        private async UniTaskVoid LoadAudioFileAsync()
        {
            var res = await MainSystem.FileExplorer.OpenSelectFileAsync(MainSystem.Args.SupportAudioFileExtensions);
            if (res.IsCancelled)
                return;

            SetChartLoadable(false);

            _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad_Loading");
            _audioFileText.TmpText.alignment = TextAlignmentOptions.CenterGeoAligned;
            using var fs = File.OpenRead(res.Path);
            if (!AudioUtils.TryLoad(fs, Path.GetExtension(res.Path), out var clip)) {
                _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad_Failed");
                return;
            }

            _loadedBytes = new byte[fs.Length];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(_loadedBytes);

            _loadedAudioFilePath = res.Path;
            _loadedClip = clip;
            _audioFileText.SetRawText(Path.GetFileName(res.Path));
            _audioFileText.TmpText.alignment = TextAlignmentOptions.BaselineLeft;
            if (string.IsNullOrEmpty(_musicNameInputField.text))
                _musicNameInputField.text = Path.GetFileNameWithoutExtension(res.Path);

            SetChartLoadable(true);
        }

        public void AddNewChart()
        {
            _charts.Add(out var item);
            item.Initialize(null, false);
            item.transform.SetAsLastSibling();
        }

        public void SelectChartToLoad(ProjectPropertiesChartController chart)
        {
            _newProjTcs.TrySetResult(chart);
        }

        public void RemoveChart(ProjectPropertiesChartController chart)
        {
            _charts.Remove(chart);
        }

        #endregion
    }
}