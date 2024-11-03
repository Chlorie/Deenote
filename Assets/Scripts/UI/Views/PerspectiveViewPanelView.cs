#nullable enable

using Deenote.Project.Models;
using Deenote.UI.ComponentModel;
using Deenote.UI.Views.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Views
{
    public sealed class PerspectiveViewPanelView : MonoBehaviour
    {
        [SerializeField] AspectRatioFitter _aspectRatioFitter;
        [Header("Stage UI")]
        [SerializeField] TMP_Text _musicNameText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] Slider _timeSlider;
        [SerializeField] Image _difficultyImage;
        [SerializeField] TMP_Text _levelText;
        [SerializeField] Button _pauseButton;
        [SerializeField] PerspectiveViewComboController _combo;
        [SerializeField] TMP_Text _backgroundStaveText;

        private Difficulty __difficulty;
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

        private string __level;
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

        private void Start()
        {
            MainSystem.GameStage.PerspectiveView.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.PerspectiveViewController.NotifyProperty.AspectRatio,
                view => _aspectRatioFitter.aspectRatio = view.AspectRatio);

            // Stage UI

            _timeSlider.onValueChanged.AddListener(val => MainSystem.GameStage.MusicController.Time = val);
            _pauseButton.onClick.AddListener(MainSystem.GameStage.MusicController.TogglePlayingState);

            MainSystem.GameStage.MusicController.OnClipChanged += len => _timeSlider.maxValue = len;
            MainSystem.GameStage.MusicController.OnTimeChanged += (_, time, _) => _timeSlider.SetValueWithoutNotify(time);
            _timeSlider.maxValue = MainSystem.GameStage.MusicController.Length;
            _timeSlider.SetValueWithoutNotify(MainSystem.GameStage.MusicController.Time);

            MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                Project.ProjectManager.NotifyProperty.MusicName,
                projm => _musicNameText.text = _backgroundStaveText.text = projm.CurrentProject.MusicName);

            MainSystem.ProjectManager.RegisterPropertyChangeNotificationAndInvoke(
                Project.ProjectManager.NotifyProperty.CurrentProject,
                projm => _musicNameText.text =_backgroundStaveText.text = projm.CurrentProject?.MusicName ?? "");

            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.ChartLevel,
                stage => Level = stage.Chart.Level);

            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.ChartDifficulty,
                stage => Difficulty = stage.Chart.Difficulty);

            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.CurrentChart,
                stage =>
                {
                    var chart = stage.Chart;
                    if (chart is null) {
                        gameObject.SetActive(false);
                    }
                    else {
                        Level = chart.Level;
                        Difficulty = chart.Difficulty;
                        gameObject.SetActive(true);
                    }
                });

            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
                GameStage.GameStageController.NotifyProperty.StageNotesUpdated,
                _combo.UpdateComboRegistrant);

            MainSystem.GameStage.RegisterPropertyChangeNotificationAndInvoke(
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