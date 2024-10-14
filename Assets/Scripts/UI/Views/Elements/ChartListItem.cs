using Deenote.Project.Models;
using Deenote.UI.Controls;
using TMPro;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class ChartListItem : MonoBehaviour
    {
        [SerializeField] Button _infoButton = default!;
        [SerializeField] UnityEngine.UI.Image _difficultyImage = default!;
        [SerializeField] TMP_Text _levelText = default!;

        [SerializeField] Button _deleteButton = default!;

        private ChartModel _chart = default!;

        public MenuProjectInfoPageView Parent { get; internal set; } = default!;
        public ChartModel Chart => _chart;

        private void Awake()
        {
            _infoButton.OnClick.AddListener(() => MainSystem.GameStage.LoadChartInCurrentProject(_chart));
            _deleteButton.OnClick.AddListener(() => Parent.RemoveChartListItem(this));
        }

        public void Initialize(ChartModel chart)
        {
            _chart = chart;
            _difficultyImage.sprite = chart.Difficulty switch {
                Difficulty.Easy => MainSystem.Args.GameStageViewArgs.EasyDifficultyIconSprite,
                Difficulty.Normal => MainSystem.Args.GameStageViewArgs.NormalDifficultyIconSprite,
                Difficulty.Hard => MainSystem.Args.GameStageViewArgs.HardDifficultyIconSprite,
                Difficulty.Extra => MainSystem.Args.GameStageViewArgs.ExtraDifficultyIconSprite,
                _ => null,
            };
            _levelText.color = _infoButton.Text.Text.color = chart.Difficulty switch {
                Difficulty.Easy => MainSystem.Args.GameStageViewArgs.EasyLevelTextColor,
                Difficulty.Normal => MainSystem.Args.GameStageViewArgs.NormalLevelTextColor,
                Difficulty.Hard => MainSystem.Args.GameStageViewArgs.HardLevelTextColor,
                Difficulty.Extra => MainSystem.Args.GameStageViewArgs.ExtraLevelTextColor,
                _ => Color.white,
            };
            _levelText.text = chart.Level;
        }
    }
}