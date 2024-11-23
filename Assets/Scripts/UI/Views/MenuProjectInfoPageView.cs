#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.UI.ComponentModel;
using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.IO;
using TMPro;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class MenuProjectInfoPageView : MonoBehaviour
    {
        [SerializeField] GameObject _noProjectLoadedTextGameObject = default!;
        [SerializeField] Collapsable _projectInfoCollapsable = default!;
        [SerializeField] KVButtonProperty _projectAudioProperty = default!;
        [SerializeField] KVInputProperty _projectMusicNameProperty = default!;
        [SerializeField] KVInputProperty _projectComposerProperty = default!;
        [SerializeField] KVInputProperty _projectChartDesignerProperty = default!;

        [SerializeField] Collapsable _projectCharts = default!;
        [SerializeField] Button _projectAddChartButton = default!;
        [SerializeField] Button _projectLoadChartButton = default!;
        [SerializeField] Transform _projectChartListParentTransform = default!;

        [SerializeField] Collapsable _chartInfoCollapsable = default!;
        [SerializeField] KVInputProperty _chartNameProperty = default!;
        [SerializeField] KVDropdownProperty _chartDifficultyProperty = default!;
        [SerializeField] KVInputProperty _chartLevelProperty = default!;
        [SerializeField] KVInputProperty _chartSpeedProperty = default!;
        [SerializeField] KVRangeInputProperty _chartRemapVolumeProperty = default!;

        [Header("Prefabs")]
        [SerializeField] ChartListItem _chartListItemPrefab;
        private PooledObjectListView<ChartListItem> _charts;

        private static readonly MessageBoxArgs _loadAudioFailedMsgBoxArgs = new(
            LocalizableText.Localized("LoadAudio_MsgBox_Title"),
            LocalizableText.Localized("LoadAudioFailed_MsgBox_Content"),
            LocalizableText.Localized("LoadAudioFailed_MsgBox_Y"));

        private static readonly MessageBoxArgs _loadChartFailedMsgBoxArgs = new(
            LocalizableText.Localized("LoadChart_MsgBox_Title"),
            LocalizableText.Localized("LoadChartFailed_MsgBox_Content"),
            LocalizableText.Localized("LoadChartFailed_MsgBox_Y"));

        private void Awake()
        {
            _charts = new PooledObjectListView<ChartListItem>(
                UnityUtils.CreateObjectPool(() =>
                {
                    var item = Instantiate(_chartListItemPrefab, _projectChartListParentTransform);
                    item.Parent = this;
                    return item;
                }));
        }

        private void Start()
        {
            // Project properties
            {
                _projectAudioProperty.Button.OnClick.AddListener(async UniTaskVoid () =>
                {
                    using DisposableGuard dg = new();
                    while (true) {
                        var res = await MainSystem.FileExplorerDialog.OpenSelectFileAsync(
                            LocalizableText.Localized("SelectAudio_FileExplorer_Title"),
                            MainSystem.Args.SupportLoadAudioFileExtensions);
                        if (res.IsCancelled)
                            return;

                        var fs = dg.Set(File.OpenRead(res.Path));
                        var clip = await AudioUtils.LoadAsync(fs, Path.GetExtension(res.Path));
                        if (clip is null) {
                            var btn = await MainSystem.MessageBoxDialog.OpenAsync(_loadAudioFailedMsgBoxArgs);
                            if (btn != 0)
                                return;
                            // Reselect file
                            continue;
                        }
                        var bytes = new byte[fs.Length];
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Read(bytes);
                        MainSystem.ProjectManager.EditProjectAudio(res.Path, bytes, clip);

                        break;
                    }
                });
                _projectMusicNameProperty.InputField.OnEndEdit.AddListener(MainSystem.ProjectManager.EditProjectMusicName);
                _projectComposerProperty.InputField.OnEndEdit.AddListener(MainSystem.ProjectManager.EditProjectComposer);
                _projectChartDesignerProperty.InputField.OnEndEdit.AddListener(MainSystem.ProjectManager.EditProjectChartDesigner);

                // TODO: 如果是在加载中第一次打开这个页面，currentproject 是null
                MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                    Project.ProjectManager.NotifyProperty.Audio,
                    proj => NotifyAudioFileChanged(proj.CurrentProject));
                MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                    Project.ProjectManager.NotifyProperty.MusicName,
                    proj => _projectMusicNameProperty.InputField.SetValueWithoutNotify(proj.CurrentProject.MusicName));
                MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                    Project.ProjectManager.NotifyProperty.Composer,
                    proj => _projectComposerProperty.InputField.SetValueWithoutNotify(proj.CurrentProject.Composer));
                MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                    Project.ProjectManager.NotifyProperty.ChartDesigner,
                    proj => _projectChartDesignerProperty.InputField.SetValueWithoutNotify(proj.CurrentProject.ChartDesigner));
            }

            // Project Charts
            {
                // Add Chart
                _projectAddChartButton.OnClick.AddListener(() =>
                {
                    _charts.Add(out var item);
                    var newChart = new ChartModel(new()) {
                        Difficulty = Difficulty.Hard,
                        Level = "10",
                    };
                    MainSystem.ProjectManager.AddProjectChart(newChart);
                    LoadChartToStage(newChart);
                });

                // LoadChart
                _projectLoadChartButton.OnClick.AddListener(async UniTaskVoid () =>
                {
                    var res = await MainSystem.FileExplorerDialog.OpenSelectFileAsync(
                        LocalizableText.Localized("SelectChart_FileExplorer_Title"),
                        MainSystem.Args.SupportLoadChartFileExtensions);
                    if (res.IsCancelled)
                        return;

                    if (!ChartData.TryLoad(File.ReadAllText(res.Path), out var chartData)) {
                        await MainSystem.MessageBoxDialog.OpenAsync(_loadChartFailedMsgBoxArgs);
                        return;
                    }

                    var newChart = new ChartModel(chartData) {
                        Difficulty = Difficulty.Hard,
                        Level = "10",
                    };
                    MainSystem.ProjectManager.AddProjectChart(newChart);
                    LoadChartToStage(newChart);
                    MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("LoadChart_Status_Loaded"));
                });

                // Notify chart list changed
                MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                    Project.ProjectManager.NotifyProperty.ChartList,
                    projm => ReloadCharts(projm.CurrentProject));
            }

            MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                Project.ProjectManager.NotifyProperty.CurrentProject,
                projm =>
                {
                    var proj = projm.CurrentProject;
                    if (proj is null) {
                        _noProjectLoadedTextGameObject.SetActive(true);
                        _projectInfoCollapsable.gameObject.SetActive(false);
                    }
                    else {
                        _noProjectLoadedTextGameObject.SetActive(false);
                        _projectInfoCollapsable.gameObject.SetActive(true);
                        NotifyAudioFileChanged(proj);
                        _projectMusicNameProperty.InputField.SetValueWithoutNotify(proj.MusicName);
                        _projectComposerProperty.InputField.SetValueWithoutNotify(proj.Composer);
                        _projectChartDesignerProperty.InputField.SetValueWithoutNotify(proj.ChartDesigner);
                        ReloadCharts(proj);
                    }
                });

            // Chart properties
            {
                _chartNameProperty.InputField.OnEndEdit.AddListener(MainSystem.GameStage.EditChartName);
                _chartDifficultyProperty.Dropdown.ResetOptions(DifficultyExt.DropdownOptions.AsSpan());
                _chartDifficultyProperty.Dropdown.OnValueChanged.AddListener(
                    val => MainSystem.GameStage.EditChartDifficulty(DifficultyExt.FromInt32(val)));
                _chartLevelProperty.InputField.OnEndEdit.AddListener(MainSystem.GameStage.EditChartLevel);
                _chartSpeedProperty.InputField.OnEndEdit.AddListener(val =>
                {
                    if (float.TryParse(val, out var fval))
                        MainSystem.GameStage.EditChartSpeed(fval);
                    else
                        _chartSpeedProperty.InputField.SetValueWithoutNotify(MainSystem.GameStage.Chart.Data.Speed.ToString("F3"));
                });
                _chartRemapVolumeProperty.LowerInputField.OnEndEdit.AddListener(val =>
                {
                    if (int.TryParse(val, out var ival))
                        MainSystem.GameStage.EditChartRemapVMin(ival);
                    else
                        _chartRemapVolumeProperty.LowerInputField.SetValueWithoutNotify(MainSystem.GameStage.Chart.Data.RemapMinVelocity.ToString());
                });
                _chartRemapVolumeProperty.UpperInputField.OnEndEdit.AddListener(val =>
                {
                    if (int.TryParse(val, out var ival))
                        MainSystem.GameStage.EditChartRemapVMax(ival);
                    else
                        _chartRemapVolumeProperty.UpperInputField.SetValueWithoutNotify(MainSystem.GameStage.Chart.Data.RemapMaxVelocity.ToString());
                });

                MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GameStageController.NotifyProperty.ChartName,
                    stage =>
                    {
                        _chartNameProperty.InputField.SetValueWithoutNotify(stage.Chart.Name ?? "");
                        RefreshCharts();
                    });
                MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GameStageController.NotifyProperty.ChartDifficulty,
                    stage =>
                    {
                        var difficulty = stage.Chart.Difficulty;
                        _chartDifficultyProperty.Dropdown.SetValueWithoutNotify(difficulty.ToInt32());
                        _chartNameProperty.InputField.PlaceHolderText = difficulty.ToDisplayString();
                        RefreshCharts();
                    });
                MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GameStageController.NotifyProperty.ChartLevel,
                    stage =>
                    {
                        _chartLevelProperty.InputField.SetValueWithoutNotify(stage.Chart.Level);
                        RefreshCharts();
                    });
                MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GameStageController.NotifyProperty.ChartSpeed,
                    stage => _chartSpeedProperty.InputField.SetValueWithoutNotify(stage.Chart.Data.Speed.ToString("F3")));
                MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GameStageController.NotifyProperty.ChartRemapMinVolume,
                    stage => _chartRemapVolumeProperty.LowerInputField.SetValueWithoutNotify(stage.Chart.Data.RemapMinVelocity.ToString()));
                MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GameStageController.NotifyProperty.ChartRemapMaxVolume,
                    stage => _chartRemapVolumeProperty.UpperInputField.SetValueWithoutNotify(stage.Chart.Data.RemapMaxVelocity.ToString()));

                MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                    GameStage.GameStageController.NotifyProperty.CurrentChart,
                    stage =>
                    {
                        if (stage.Chart is null) {
                            _chartInfoCollapsable.gameObject.SetActive(false);
                        }
                        else {
                            _chartInfoCollapsable.gameObject.SetActive(true);
                            var chart = stage.Chart;
                            _chartNameProperty.InputField.PlaceHolderText = chart.Difficulty.ToDisplayString();
                            _chartNameProperty.InputField.SetValueWithoutNotify(chart.Name);
                            _chartDifficultyProperty.Dropdown.SetValueWithoutNotify(chart.Difficulty.ToInt32());
                            _chartLevelProperty.InputField.SetValueWithoutNotify(chart.Level);
                            _chartRemapVolumeProperty.LowerInputField.SetValueWithoutNotify(chart.Data.RemapMinVelocity.ToString());
                            _chartRemapVolumeProperty.UpperInputField.SetValueWithoutNotify(chart.Data.RemapMaxVelocity.ToString());
                        }
                    });
            }

            void NotifyAudioFileChanged(ProjectModel proj)
            {
                // TODO: proj为null时报错
                _projectAudioProperty.Button.Text.SetRawText(Path.GetFileName(proj.AudioFileRelativePath));
                _projectAudioProperty.Button.Text.Text.horizontalAlignment = HorizontalAlignmentOptions.Left;
            }

            void ReloadCharts(ProjectModel proj)
            {
                using (var resettingCharts = _charts.Resetting(proj.Charts.Count)) {
                    foreach (var chart in proj.Charts) {
                        resettingCharts.Add(out var item);
                        item.Initialize(chart);
                    }
                }
                _charts.SetSiblingIndicesInOrder();
            }
        }

        /// <summary>
        /// Update display datas of chart items
        /// </summary>
        private void RefreshCharts()
        {
            foreach (var chart in _charts) {
                chart.Refresh();
            }
        }

        internal void LoadChartToStage(ChartModel chart)
        {
            MainSystem.GameStage.LoadChartInCurrentProject(chart);
        }

        internal void RemoveChartListItem(ChartListItem item)
        {
            int findIndex = _charts.IndexOf(item);
            Debug.Assert(findIndex >= 0, $"Try to remove a {nameof(ChartListItem)} that is not in the list");
            _charts.RemoveAt(findIndex);
            Debug.Assert(ReferenceEquals(item.Chart, MainSystem.ProjectManager.CurrentProject.Charts[findIndex]),
                "Chart in ProjectInfo page and in current project not match");
            MainSystem.ProjectManager.RemoveProjectChartAt(findIndex);
        }
    }
}