#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using Deenote.Library.Components;
using Deenote.Localization;
using Deenote.UI.Dialogs.Elements;
using Deenote.UI.Views.Elements;
using Deenote.UIFramework;
using Deenote.UIFramework.Controls;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class ProjectInfoNavigationPageView : MonoBehaviour
    {
        [Header("Project")]
        [SerializeField] RectTransform _projectInfoGroup = default!;
        [SerializeField] Button _audioButton = default!;
        [SerializeField] TextBox _musicNameInput = default!;
        [SerializeField] TextBox _composerInput = default!;
        [SerializeField] TextBox _chartDesignerInput = default!;
        [SerializeField] Collapsable _chartsCollapsable = default!;
        [SerializeField] Button _addChartButton = default!;
        [SerializeField] Button _loadChartButton = default!;
        [Header("Chart")]
        [SerializeField] RectTransform _chartInfoGroup = default!;
        [SerializeField] TextBox _chartNameInput = default!;
        [SerializeField] Dropdown _chartDifficultyDropdown = default!;
        [SerializeField] TextBox _chartLevelInput = default!;
        [SerializeField] TextBox _chartSpeedInput = default!;
        [SerializeField] TextBox _chartRemapMinVolumeInput = default!;
        [SerializeField] TextBox _chartRemapMaxVolumeInput = default!;

        [Header("Prefabs")]
        [SerializeField] ProjectInfoChartListItem _chartListItemPrefab = default!;
        private PooledObjectListView<ProjectInfoChartListItem> _chartItems = default!;

        private ResetableCancellationTokenSource _rcts = default!;

        #region MessageBoxArgs

        private static readonly MessageBoxArgs _loadAudioFailedMsgBoxArgs = new(
            LocalizableText.Localized("LoadAudio_MsgBox_Title"),
            LocalizableText.Localized("LoadAudioFailed_MsgBox_Content"),
            LocalizableText.Localized("LoadAudioFailed_MsgBox_Y"));

        private static readonly MessageBoxArgs _loadChartFailedMsgBoxArgs = new(
            LocalizableText.Localized("LoadChart_MsgBox_Title"),
            LocalizableText.Localized("LoadChartFailed_MsgBox_Content"),
            LocalizableText.Localized("LoadChartFailed_MsgBox_Y"));

        #endregion

        #region LocalizedTextKey

        private const string SelectAudioFileExplorerTitleKey = "SelectAudio_FileExplorer_Title";
        private const string SelectChartFileExplorerTitleKey = "SelectChart_FileExplorer_Title";
        private const string ChartLoadedStatusKey = "LoadChart_Status_Loaded";
        private const string LoadAudioLoadingStatusKey = "LoadAudio_Status_Loading";
        private const string LoadAudioLoadedStatusKey = "LoadAudio_Status_Loaded";

        #endregion

        private void Awake()
        {
            _chartItems = new(UnityUtils.CreateObjectPool(_chartListItemPrefab, _chartsCollapsable.Content,
                item => item.OnInstantiate(this), defaultCapacity: 0));
            _rcts = new();

            #region Project
            {
                _audioButton.Clicked += UniTask.Action(async () =>
                {
                    AssertProjectLoaded();
                    _rcts.CancelAndReset();

                    while (true) {
                        var res = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                            LocalizableText.Localized(SelectAudioFileExplorerTitleKey),
                            MainSystem.Args.SupportLoadAudioFileExtensions);
                        if (res.IsCancelled)
                            return;

                        var fileName = Path.GetFileName(res.Path);
                        MainWindow.StatusBar.SetLocalizedStatusMessage(LoadAudioLoadingStatusKey, fileName);

                        using var fs = File.OpenRead(res.Path);
                        var clip = await AudioUtils.TryLoadAsync(fs, Path.GetExtension(res.Path), _rcts.Token);
                        if (clip is null) {
                            MainWindow.StatusBar.SetReadyStatusMessage();

                            _rcts.Token.ThrowIfCancellationRequested();
                            var btn = await MainWindow.DialogManager.OpenMessageBoxAsync(_loadAudioFailedMsgBoxArgs);
                            if (btn != 0)
                                return;
                            continue; // Re-select file
                        }

                        var bytes = new byte[fs.Length];
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Read(bytes);
                        MainSystem.ProjectManager.EditProjectAudio(res.Path, bytes, clip);

                        MainWindow.StatusBar.SetLocalizedStatusMessage(LoadAudioLoadedStatusKey, fileName, 3f);

                        break;
                    }
                });
                _musicNameInput.EditSubmitted += MainSystem.ProjectManager.EditProjectMusicName;
                _composerInput.EditSubmitted += MainSystem.ProjectManager.EditProjectComposer;
                _chartDesignerInput.EditSubmitted += MainSystem.ProjectManager.EditProjectChartDesigner;
                _addChartButton.Clicked += () =>
                {
                    var newChart = new ChartModel(new()) {
                        Difficulty = Difficulty.Hard,
                        Level = "10",
                    };
                    MainSystem.ProjectManager.AddProjectChart(newChart);
                    LoadChartModelToStage(newChart);
                };
                _loadChartButton.Clicked += UniTask.Action(async () =>
                {
                    var res = await MainWindow.DialogManager.OpenFileExplorerSelectFileAsync(
                        LocalizableText.Localized(SelectChartFileExplorerTitleKey),
                        MainSystem.Args.SupportLoadChartFileExtensions);
                    if (res.IsCancelled)
                        return;

                    if (!ChartModel.TryParse(await File.ReadAllTextAsync(res.Path), out var chart)) {
                        await MainWindow.DialogManager.OpenMessageBoxAsync(_loadChartFailedMsgBoxArgs);
                        return;
                    }

                    chart.Difficulty = Difficulty.Hard;
                    chart.Level = "10";

                    MainSystem.ProjectManager.AddProjectChart(chart);
                    LoadChartModelToStage(chart);
                });

                var manager = MainSystem.ProjectManager;
                manager.RegisterNotificationAndInvoke(ProjectManager.NotificationFlag.CurrentProject,
                    manager =>
                    {
                        if (!manager.IsProjectLoaded()) {
                            _projectInfoGroup.gameObject.SetActive(false);
                        }
                        else {
                            var proj = manager.CurrentProject;
                            _projectInfoGroup.gameObject.SetActive(true);
                            SetAudio(proj.AudioFileRelativePath);
                            SetName(proj.MusicName);
                            SetComposer(proj.Composer);
                            SetCharter(proj.ChartDesigner);
                            SetCharts(proj.Charts);
                        }
                    });
                manager.RegisterNotification(ProjectManager.NotificationFlag.ProjectAudio,
                    manager =>
                    {
                        if (manager.IsProjectLoaded() && manager.CurrentProject is var proj)
                            SetAudio(proj.AudioFileRelativePath);
                    });
                manager.RegisterNotification(ProjectManager.NotificationFlag.ProjectMusicName,
                    manager =>
                    {
                        if (manager.IsProjectLoaded() && manager.CurrentProject is var proj)
                            SetName(proj.MusicName);
                    });
                manager.RegisterNotification(ProjectManager.NotificationFlag.ProjectComposer,
                    manager =>
                    {
                        if (manager.IsProjectLoaded() && manager.CurrentProject is var proj)
                            SetComposer(proj.Composer);
                    });
                manager.RegisterNotification(ProjectManager.NotificationFlag.ProjectChartDesigner,
                    manager =>
                    {
                        if (manager.IsProjectLoaded() && manager.CurrentProject is var proj)
                            SetCharter(proj.ChartDesigner);
                    });
                manager.RegisterNotification(ProjectManager.NotificationFlag.ProjectCharts,
                    manager =>
                    {
                        if (manager.IsProjectLoaded() && manager.CurrentProject is var proj)
                            SetCharts(manager.CurrentProject.Charts);
                    });

                void SetAudio(string audioPath) => _audioButton.Text.SetRawText(Path.GetFileName(audioPath));
                void SetName(string name) => _musicNameInput.SetValueWithoutNotify(name);
                void SetComposer(string composer) => _composerInput.SetValueWithoutNotify(composer);
                void SetCharter(string charter) => _chartDesignerInput.SetValueWithoutNotify(charter);
                void SetCharts(List<ChartModel> charts)
                {
                    using (var resetter = _chartItems.Resetting(charts.Count)) {
                        foreach (var chart in charts) {
                            resetter.Add(out var item);
                            item.Initialize(chart);
                        }
                    }
                    _chartItems.SetSiblingIndicesInOrder();
                }
            }
            #endregion

            #region Chart
            {
                _chartNameInput.EditSubmitted += MainSystem.GamePlayManager.EditChartName;
                _chartDifficultyDropdown.ResetOptions(DifficultyExt.DropdownOptions.AsSpan());
                _chartDifficultyDropdown.SelectedIndexChanged += val =>
                {
                    var diff = DifficultyExt.FromDropdownIndex(val);
                    MainSystem.GamePlayManager.EditChartDifficulty(diff);
                    _chartNameInput.SetPlaceHolderText(LocalizableText.Raw(diff.ToDisplayString()));
                };
                _chartLevelInput.EditSubmitted += MainSystem.GamePlayManager.EditChartLevel;
                _chartSpeedInput.EditSubmitted += val =>
                {
                    if (float.TryParse(val, out var speed))
                        MainSystem.GamePlayManager.EditChartSpeed(speed);
                    else
                        _chartSpeedInput.SetValueWithoutNotify(MainSystem.GamePlayManager.CurrentChart?.Speed.ToString("F3"));
                };
                _chartRemapMinVolumeInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var vol))
                        MainSystem.GamePlayManager.EditChartRemapMinVolume(vol);
                    else
                        _chartRemapMinVolumeInput.SetValueWithoutNotify(MainSystem.GamePlayManager.CurrentChart?.RemapMinVolume.ToString());
                };
                _chartRemapMaxVolumeInput.EditSubmitted += val =>
                {
                    if (int.TryParse(val, out var vol))
                        MainSystem.GamePlayManager.EditChartRemaMaxVolume(vol);
                    else
                        _chartRemapMaxVolumeInput.SetValueWithoutNotify(MainSystem.GamePlayManager.CurrentChart?.RemapMaxVolume.ToString());
                };

                var game = MainSystem.GamePlayManager;
                game.RegisterNotificationAndInvoke(GamePlayManager.NotificationFlag.CurrentChart,
                    stage =>
                    {
                        if (stage.CurrentChart is not { } cht) {
                            _chartInfoGroup.gameObject.SetActive(false);
                        }
                        else {
                            _chartInfoGroup.gameObject.SetActive(true);
                            SetName(cht.Name);
                            SetDifficulty(cht.Difficulty);
                            SetLevel(cht.Level);
                            SetSpeed(cht.Speed);
                            SetRemapMinVolume(cht.RemapMinVolume);
                            SetRemapMaxVolume(cht.RemapMaxVolume);
                        }
                    });
                game.RegisterNotification(GamePlayManager.NotificationFlag.ChartName,
                    stage =>
                    {
                        if (stage.CurrentChart is { } chart) {
                            SetName(chart.Name);
                            RefreshChartListUI();
                        }
                    });
                game.RegisterNotification(GamePlayManager.NotificationFlag.ChartDifficulty,
                    stage =>
                    {
                        if (stage.CurrentChart is { } chart) {
                            SetDifficulty(chart.Difficulty);
                            RefreshChartListUI();
                        }
                    });
                game.RegisterNotification(GamePlayManager.NotificationFlag.ChartLevel,
                    stage =>
                    {
                        if (stage.CurrentChart is { } chart) {
                            SetLevel(chart.Level);
                            RefreshChartListUI();
                        }
                    });
                game.RegisterNotification(GamePlayManager.NotificationFlag.ChartSpeed,
                    stage =>
                    {
                        if (stage.CurrentChart is { } chart) {
                            SetSpeed(chart.Speed);
                        }
                    });
                game.RegisterNotification(GamePlayManager.NotificationFlag.ChartRemapMinVolume,
                    stage =>
                    {
                        if (stage.CurrentChart is { } chart) {
                            SetRemapMinVolume(chart.RemapMinVolume);
                        }
                    });
                game.RegisterNotification(GamePlayManager.NotificationFlag.ChartRemapMaxVolume,
                    stage =>
                    {
                        if (stage.CurrentChart is { } chart) {
                            SetRemapMaxVolume(chart.RemapMaxVolume);
                        }
                    });

                void SetName(string name) => _chartNameInput.SetValueWithoutNotify(name);
                void SetDifficulty(Difficulty difficulty)
                {
                    _chartNameInput.SetPlaceHolderText(LocalizableText.Raw(difficulty.ToDisplayString()));
                    _chartDifficultyDropdown.SetValueWithoutNotify(difficulty.ToDropdownIndex());
                }
                void SetLevel(string level) => _chartLevelInput.SetValueWithoutNotify(level);
                void SetSpeed(float speed) => _chartSpeedInput.SetValueWithoutNotify(speed.ToString("F3"));
                void SetRemapMinVolume(int vol) => _chartRemapMinVolumeInput.SetValueWithoutNotify(vol.ToString());
                void SetRemapMaxVolume(int vol) => _chartRemapMaxVolumeInput.SetValueWithoutNotify(vol.ToString());
            }
            #endregion
        }

        private void RefreshChartListUI()
        {
            foreach (var item in _chartItems) {
                item.RefreshUI();
            }
        }

        #region Chart List Item Callbacks

        internal void LoadChartToStage(ProjectInfoChartListItem item)
            => LoadChartModelToStage(item.ChartModel);

        internal void RemoveChart(ProjectInfoChartListItem item)
        {
            MainSystem.ProjectManager.AssertProjectLoaded();

            int findIndex = _chartItems.IndexOf(item);
            Debug.Assert(findIndex >= 0, $"Try to remove a {nameof(ProjectInfoChartListItem)} that is not in the list");
            _chartItems.RemoveAt(findIndex);
            Debug.Assert(ReferenceEquals(item.ChartModel, MainSystem.ProjectManager.CurrentProject.Charts[findIndex]),
                "Chart in ProjectInfo page and in current project not match");
            MainSystem.ProjectManager.RemoveProjectChartAt(findIndex);

            if (_chartItems.Count == 0) {
                MainSystem.ProjectManager.AddProjectChart(new ChartModel());
            }
        }

        #endregion

        private void LoadChartModelToStage(ChartModel chart)
        {
            MainSystem.GamePlayManager.LoadChartInCurrentProject(chart);
            MainWindow.StatusBar.SetLocalizedStatusMessage(ChartLoadedStatusKey);
        }

        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        private void AssertProjectLoaded() => Debug.Assert(MainSystem.ProjectManager.CurrentProject is not null);
    }
}