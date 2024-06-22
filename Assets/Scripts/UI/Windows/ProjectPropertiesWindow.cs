using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class ProjectPropertiesWindow : MonoBehaviour
    {
        [SerializeField] Window _window;

        [Header("UI")]
        [SerializeField] Button _audioFileButton;
        [SerializeField] LocalizedText _audioFileText;
        [SerializeField] TMP_InputField _musicNameInputField;
        [SerializeField] TMP_InputField _composerInputField;
        [SerializeField] TMP_InputField _chartDesignerInputField;
        [SerializeField] Transform _chartsParentTransform;
        [SerializeField] ProjectPropertiesChartController _mainChartController;

        [Header("Prefabs")]
        [SerializeField] ProjectPropertiesChartController _chartPrefab;

        private ObjectPool<ProjectPropertiesChartController> _chartPool;
        /// <summary>
        /// Includes <see cref="_mainChartController"/>
        /// </summary>
        private List<ProjectPropertiesChartController> _charts;

        private static readonly string[] _supportMusicExtensions = { ".mp3", ".wav" };

        private string _selectedMainAudioPath;
        private byte[] _loadedBytes;
        private AudioClip _loadedClip;

        private void Awake()
        {
            _chartPool = UnityUtils.CreateObjectPool(_chartPrefab, _chartsParentTransform);
            _charts = new() { _mainChartController };

            _audioFileButton.onClick.AddListener(LoadAudioFileAsync);
            _mainChartController.InitializeMain(() => _newProjTcs.TrySetResult(_mainChartController), AddChart);
        }

        private void OnEnable()
        {
            _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad");
            _musicNameInputField.text = "";
            _composerInputField.text = "";
            _chartDesignerInputField.text = "";
            _audioFileText.TmpText.alignment = TextAlignmentOptions.CenterGeoAligned;
            SetChartLoadable(false);
        }

        private void OnDisable()
        {
            _newProjTcs?.TrySetResult(null);
        }

        private void SetChartLoadable(bool value)
        {
            if (_mainChartController.LoadButton.interactable == value)
                return;

            _mainChartController.LoadButton.interactable = value;
            foreach (var chart in _charts) {
                chart.LoadButton.interactable = value;
            }
        }

        #region UI Events

        private async UniTaskVoid LoadAudioFileAsync()
        {
            _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad_Loading");
            var res = await MainSystem.FileExplorer.OpenSelectFileAsync(_supportMusicExtensions);
            if (res.IsCancelled)
                return;

            SetChartLoadable(false);

            var bytes = await File.ReadAllBytesAsync(res.Path);
            var clip = AudioUtils.LoadFromBuffer(bytes, Path.GetExtension(res.Path));

            if (!clip.HasValue) {
                _audioFileText.SetLocalizedText("Window_ProjectProperties_AudioFileLoad_Failed");
                return;
            }

            _loadedBytes = bytes;
            _loadedClip = clip.Value;
            _selectedMainAudioPath = res.Path;
            _audioFileText.SetRawText(Path.GetFileName(res.Path));
            if (string.IsNullOrEmpty(_musicNameInputField.text))
                _musicNameInputField.text = Path.GetFileNameWithoutExtension(res.Path);

            SetChartLoadable(true);
        }

        private void AddChart()
        {
            var chartFile = _chartPool.Get();
            chartFile.Initialize(
                cht => _newProjTcs.TrySetResult(cht),
                cht =>
                {
                    _chartPool.Release(cht);
                    _charts.Remove(cht);
                });
            chartFile.transform.SetAsLastSibling();
            _charts.Add(chartFile);
        }

        #endregion
    }
}