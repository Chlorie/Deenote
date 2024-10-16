using Deenote.Project.Models;
using Deenote.UI.Views.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    public sealed class PerspectiveViewPanelView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] TMP_Text _musicNameText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] Slider _timeSlider;
        [SerializeField] Image _difficultyImage;
        [SerializeField] TMP_Text _levelText;
        [SerializeField] Button _pauseButton;
        [SerializeField] PerspectiveViewComboController _combo; // Update in itself
        [SerializeField] TMP_Text _backgroundStaveText;
        [Header("Args")]

        private Difficulty __difficulty;
        private string __level;

        private Difficulty Difficulty
        {
            get => __difficulty;
            set {
                if (__difficulty == value)
                    return;
                __difficulty = value;

                var args = MainSystem.Args.GameStageViewArgs;
                (_difficultyImage.sprite, _levelText.color) = value switch {
                    Difficulty.Easy => (args.EasyDifficultyIconSprite, args.EasyLevelTextColor),
                    Difficulty.Normal => (args.NormalDifficultyIconSprite, args.NormalLevelTextColor),
                    Difficulty.Hard => (args.HardDifficultyIconSprite, args.HardLevelTextColor),
                    Difficulty.Extra => (args.ExtraDifficultyIconSprite, args.ExtraLevelTextColor),
                    _ => (args.HardDifficultyIconSprite, Color.white),
                };
                _levelText.text = $"{value.ToDisplayString()} Lv {Level}";
            }
        }

        private string Level
        {
            get => __level;
            set {
                if (__level == value)
                    return;
                __level = value;
                _levelText.text = $"{Difficulty.ToDisplayString()} Lv {value}";
            }
        }

        public float AspectRatio
        {
            get;set;
        }

        // PerspectiveViewController;

        // TODO: 关于Size Ratio，这部分从PerspectiveWindow迁移过来

        private void Start()
        {
            _timeSlider.onValueChanged.AddListener(val => MainSystem.GameStage.MusicController.Time = val);
            _pauseButton.onClick.AddListener(MainSystem.GameStage.MusicController.TogglePlayingState);

            MainSystem.GameStage.MusicController.OnTimeChanged += (_, time, _) => _timeSlider.SetValueWithoutNotify(time);
            MainSystem.GameStage.MusicController.OnClipChanged += len => _timeSlider.maxValue = len;

            MainSystem.ProjectManager.RegisterPropertyChangeNotification(
                Project.ProjectManager.NotifyProperty.MusicName,
                projm => _musicNameText.text = _backgroundStaveText.text = projm.CurrentProject.MusicName);

            MainSystem.ProjectManager.RegisterPropertyChangeNotification(
                Project.ProjectManager.NotifyProperty.CurrentProject,
                projm => _musicNameText.text = projm.CurrentProject?.MusicName ?? "");

            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.ChartLevel,
                stage => Level = stage.Chart.Level);

            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.ChartDifficulty,
                stage => Difficulty = stage.Chart.Difficulty);

            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.CurrentChart,
                stage =>
                {
                    var chart = stage.Chart;
                    if (chart is null) {
                        Level = "";
                        Difficulty = Difficulty.Hard;
                    }
                    else {
                        Level = chart.Level;
                        Difficulty = chart.Difficulty;
                    }
                });
            
            MainSystem.GameStage.RegisterPropertyChangeNotification(
                GameStage.GameStageController.NotifyProperty.StageNotesUpdated,
                stage => // Update score
                {
                    var currentCombo = stage.CurrentCombo;
                    if (currentCombo <= 0) {
                        _scoreText.text = "0.00 %";
                        return;
                    }

                    int noteCount = stage.Chart.Notes.NoteCount;
                    float accScore = (float)currentCombo / noteCount;
                    // comboActual = Sum(1..judgeNoteCount);
                    // comboTotal = Sum(1..noteCount)
                    // comboScore = comboActual / comboTotal
                    //            = ((1 + judged) * judged) / ((1 + count) * count)
                    float comboScore = (float)((1 + currentCombo) * currentCombo) / ((1 + noteCount) * noteCount);

                    float score = accScore * 80_00f + comboScore * 20_00f;
                    _scoreText.text = $"{Mathf.Floor(score) / 100f:F2} %";
                });
        }
    }
}