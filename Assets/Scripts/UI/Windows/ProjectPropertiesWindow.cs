using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        [SerializeField] Button _addChartButton;

        [Header("Prefabs")]
        [SerializeField] ProjectPropertiesChartController _chartPrefab;

        private PooledObjectListView<ProjectPropertiesChartController> _charts;
        private List<CancellationTokenSource> _cancellationTokenSources = new();

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
            _addChartButton.onClick.AddListener(AddNewChart);

            _window.SetOnIsActivatedChanged(activated =>
            {
                if (!activated) {
                    if (_cancellationTokenSources.Count > 0) {
                        foreach (var cts in _cancellationTokenSources)
                            cts.Cancel();
                        _cancellationTokenSources.Clear();
                    }
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
                _charts[0].Initialize(null);
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
                for (int i = 0; i < _charts.Count; i++) {
                    _charts[i].Initialize(project.Charts[i]);
                }
                _charts.SetSiblingIndicesInOrder();
                SetChartLoadable(true);
            }
        }

        #region UI Events

        private CancellationTokenSource __audioLoadingCts;
        private async UniTaskVoid LoadAudioFileAsync()
        {
            if (false == __audioLoadingCts?.IsCancellationRequested) {
                __audioLoadingCts.Cancel();
                __audioLoadingCts = null;
            }
            
            var res = await MainSystem.FileExplorer.OpenSelectFileAsync(MainSystem.Args.SupportAudioFileExtensions);
            if (res.IsCancelled)
                return;

            SetChartLoadable(false);

            _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad_Loading");
            _audioFileText.TmpText.alignment = TextAlignmentOptions.MidlineGeoAligned;

            AudioClip clip;
            using var fs = File.OpenRead(res.Path);
            var cts = __audioLoadingCts = new CancellationTokenSource();
            try {
                if (!(await AudioUtils.LoadAsync(fs, Path.GetExtension(res.Path), cts.Token)).TryGetValue(out clip)) {
                    _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad_Failed");
                    return;
                }
            } finally {
                cts.Dispose();
                __audioLoadingCts = null;
            }

            _loadedBytes = new byte[fs.Length];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(_loadedBytes);

            _loadedAudioFilePath = res.Path;
            _loadedClip = clip;

            _audioFileText.SetRawText(Path.GetFileName(res.Path));
            _audioFileText.TmpText.alignment = TextAlignmentOptions.MidlineLeft;
            if (string.IsNullOrEmpty(_musicNameInputField.text))
                _musicNameInputField.text = Path.GetFileNameWithoutExtension(res.Path);

            SetChartLoadable(true);
        }

        private void AddNewChart()
        {
            _charts.Add(out var item);
            item.Initialize(null);
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