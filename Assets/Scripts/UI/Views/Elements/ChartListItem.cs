using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Project.Models;
using Deenote.UI.Controls;
using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Deenote.UI.Views.Elements
{
    public sealed class ChartListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] UnityEngine.UI.Button _infoButton = default!;
        [SerializeField] UnityEngine.UI.Image _difficultyImage = default!;
        [SerializeField] TMP_Text _nameText = default!;
        [SerializeField] TMP_Text _levelText = default!;

        [SerializeField] Button _deleteButton = default!;

        private ChartModel _chart = default!;

        public MenuProjectInfoPageView Parent { get; internal set; } = default!;
        public ChartModel Chart => _chart;

        private void Awake()
        {
            _infoButton.onClick.AddListener(() => Parent.LoadChartToStage(_chart));
            _deleteButton.OnClick.AddListener(() => Parent.RemoveChartListItem(this));
        }

        public void Initialize(ChartModel chart)
        {
            _chart = chart;
            Refresh();
        }

        // TODO: Bind to button in unity editor
        public async UniTask ExportFileAsync()
        {
            var res = await MainSystem.FileExplorerDialog.OpenSelectDirectoryAsync(LocalizableText.Localized("ExportChart_FileExplorer_Title"));
            if (res.IsCancelled)
                return;

            var chart = _chart.Data.Clone();
            MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("ExportChart_Status_Exporting"));
            var filename = Path.GetFileNameWithoutExtension(MainSystem.ProjectManager.CurrentProject.ProjectFilePath);
            await File.WriteAllTextAsync(
                Path.Combine(res.Path, $"{filename}.{_chart.Name ?? _chart.Difficulty.ToLowerCaseString()}.json"),
                chart.ToJsonString());
            MainSystem.StatusBarView.SetStatusMessage(LocalizableText.Localized("ExportChart_Status_Exported"));
        }

        internal void Refresh()
        {
            _difficultyImage.sprite = _chart.Difficulty switch {
                Difficulty.Easy => MainSystem.Args.GameStageViewArgs.EasyDifficultyIconSprite,
                Difficulty.Normal => MainSystem.Args.GameStageViewArgs.NormalDifficultyIconSprite,
                Difficulty.Hard => MainSystem.Args.GameStageViewArgs.HardDifficultyIconSprite,
                Difficulty.Extra => MainSystem.Args.GameStageViewArgs.ExtraDifficultyIconSprite,
                _ => null,
            };
            _levelText.color = _nameText.color = _chart.Difficulty switch {
                Difficulty.Easy => MainSystem.Args.GameStageViewArgs.EasyLevelTextColor,
                Difficulty.Normal => MainSystem.Args.GameStageViewArgs.NormalLevelTextColor,
                Difficulty.Hard => MainSystem.Args.GameStageViewArgs.HardLevelTextColor,
                Difficulty.Extra => MainSystem.Args.GameStageViewArgs.ExtraLevelTextColor,
                _ => Color.white,
            };
            if (_chart.Name is not null) {
                _nameText.text = _chart.Name;
                _nameText.fontStyle &= ~FontStyles.Italic;
            }
            else {
                _nameText.text = _chart.Difficulty.ToDisplayString();
                _nameText.fontStyle |= FontStyles.Italic;
            }
            Span<char> chars = stackalloc char[_chart.Level.Length + 3];
            "Lv ".AsSpan().CopyTo(chars);
            _chart.Level.AsSpan().CopyTo(chars[3..]);
            _levelText.text = chars.ToString();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _deleteButton.gameObject.SetActive(true);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _deleteButton.gameObject.SetActive(false);
        }
    }
}