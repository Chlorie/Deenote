using Deenote.Project.Models;
using Deenote.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    partial class PerspectiveViewWindow
    {
        [Header("UI Foreground")]
        [SerializeField] TMP_Text _musicNameText;
        [SerializeField] TMP_Text _scoreText;
        [SerializeField] Slider _timeSlider;
        [SerializeField] Image _difficultyImage;
        [SerializeField] TMP_Text _levelText;
        [SerializeField] Button _pauseButton;
        [Header("UI Combo")]
        [SerializeField] GameObject _comboParentGameObject;
        [SerializeField] TMP_Text _comboNumberText;
        [SerializeField] TMP_Text _comboShadowText;
        [SerializeField] Image _shockWaveCircleImage;
        [SerializeField] Image _shockWaveImage;
        [SerializeField] Image _charmingImage;

        [Header("UI Stage")]
        [SerializeField] TMP_Text _backgroundStaveText;

        private ChartModel _chart;

        private Difficulty _currentDifficulty;
        private string _currentLevel;

        private void AwakeStageUI()
        {
            _timeSlider.onValueChanged.AddListener(OnTimeSliderValueChanged);
            _pauseButton.onClick.AddListener(OnPauseClicked);
        }

        #region Events

        private void OnTimeSliderValueChanged(float value) => _gameStageController.CurrentMusicTime = value;

        private void OnPauseClicked() => _gameStageController.TogglePlayingState();

        #endregion

        #region Notify

        public void NotifyMusicNameChanged(string name)
        {
            if (!_window.IsActivated)
                return;
            _musicNameText.text = name;
            _backgroundStaveText.text = name;
        }

        public void NotifyChartLevelChanged(string level)
        {
            if (!_window.IsActivated)
                return;
            if (level == _currentLevel)
                return;
            _currentLevel = level;

            _levelText.text = $"{_currentDifficulty.ToDisplayString()} Lv {level}";
        }

        public void NotifyChartDifficultyChanged(Difficulty difficulty)
        {
            if (!_window.IsActivated)
                return;

            if (difficulty == _currentDifficulty)
                return;
            _currentDifficulty = difficulty;

            _difficultyImage.sprite = difficulty switch {
                Difficulty.Easy => _args.EasyDifficultyIconSprite,
                Difficulty.Normal => _args.NormalDifficultyIconSprite,
                Difficulty.Hard => _args.HardDifficultyIconSprite,
                Difficulty.Extra => _args.ExtraDifficultyIconSprite,
                _ => _args.HardDifficultyIconSprite,
            };
            _levelText.color = difficulty switch {
                Difficulty.Easy => _args.EasyLevelTextColor,
                Difficulty.Normal => _args.NormalLevelTextColor,
                Difficulty.Hard => _args.HardLevelTextColor,
                Difficulty.Extra => _args.ExtraLevelTextColor,
                _ => throw new System.NotImplementedException(),
            };
            _levelText.text = $"{difficulty.ToDisplayString()} Lv {_currentLevel}";
        }

        public void NotifyMusicTimeChanged(float time)
        {
            _timeSlider.SetValueWithoutNotify(time);
        }

        public void NotifyChartChanged(ProjectModel project, ChartModel chart)
        {
            if (!_window.IsActivated)
                return;

            if (project is null) {
                NotifyMusicNameChanged("");
                NotifyChartLevelChanged("");
                NotifyChartDifficultyChanged(Difficulty.Hard);
                NotifyMusicTimeChanged(0f);
                NotifyGameStageProgressChanged(0);
                return;
            }

            Debug.Assert(project.Charts.Contains(chart));

            NotifyMusicNameChanged(project.MusicName);
            _timeSlider.maxValue = project.AudioClip.length;
            if (chart is null) {
                NotifyChartLevelChanged("");
                NotifyChartDifficultyChanged(Difficulty.Hard);
                NotifyMusicTimeChanged(0f);
                NotifyGameStageProgressChanged(0);
                return;
            }

            _chart = chart;
            NotifyChartLevelChanged(chart.Level);
            NotifyChartDifficultyChanged(chart.Difficulty);
        }

        public void NotifyGameStageProgressChanged(int nextHitNoteIndex)
        {
            UpdateScore(nextHitNoteIndex);
            UpdateCombo(nextHitNoteIndex);
        }

        #endregion

        private void UpdateScore(int judgedNoteCount)
        {
            if (judgedNoteCount <= 0) {
                _scoreText.text = "0.00 %";
                return;
            }

            int noteCount = _chart.Notes.Count;
            float accScore = (float)judgedNoteCount / noteCount;
            // comboActual = Sum(1..judgeNoteCount);
            // comboTotal = Sum(1..noteCount)
            // comboScore = comboActual / comboTotal
            //            = ((1 + judged) * judged) / ((1 + count) * count)
            float comboScore = (float)((1 + judgedNoteCount) * judgedNoteCount) / ((1 + noteCount) * noteCount);

            float score = accScore * 80_00f + comboScore * 20_00f; ;
            _scoreText.text = $"{Mathf.Floor(score) / 100f:F2} %";
        }

        private void UpdateCombo(int combo)
        {
            if (combo < 5) {
                _comboParentGameObject.SetActive(false);
                return;
            }

            var prevHitNoteIndex = combo - 1;
            _comboParentGameObject.SetActive(true);
            var deltaTime = _gameStageController.CurrentMusicTime - _chart.Notes[prevHitNoteIndex].Data.Time;
            Debug.Assert(deltaTime >= 0, $"actual delta time:{deltaTime}");
            _comboNumberText.text = _comboShadowText.text = combo.ToString();

            // Number
            {
                float grey;
                if (deltaTime <= _args.ComboNumberGreyIncTime) {
                    float ratio = deltaTime / _args.ComboNumberGreyIncTime;
                    grey = Mathf.Lerp(0f, 1f, ratio);
                }
                else if (deltaTime <= _args.ComboNumberGreyIncTime + _args.ComboNumberGreyDecTime) {
                    float ratio = (deltaTime - _args.ComboNumberGreyIncTime) / _args.ComboNumberGreyDecTime;
                    grey = Mathf.Pow(1f - ratio, 0.67f);
                }
                else {
                    grey = 0f;
                }
                _comboNumberText.color = new Color(grey, grey, grey);
            }
            // Shadow Number
            {
                float alpha;
                float scale;
                if (deltaTime <= _args.ComboShadowDuration) {
                    float ratio = deltaTime / _args.ComboShadowDuration;
                    alpha = Mathf.Lerp(1f, _args.ComboShadowMinAlpha, ratio);
                    scale = Mathf.Lerp(1f, _args.ComboShadowMaxScale, ratio);
                }
                else {
                    alpha = _args.ComboShadowMinAlpha;
                    scale = _args.ComboShadowMaxScale;
                }
                _comboShadowText.transform.localScale = new Vector3(scale, scale, scale);
                _comboShadowText.color = Color.black.WithAlpha(alpha);
            }
            // Shock Wave Circle
            {
                float alpha;
                float scale;
                if (deltaTime <= _args.ComboCircleStartTime) {
                    scale = alpha = 0f;
                }
                else if (deltaTime <= _args.ComboCircleStartTime + _args.ComboCircleScaleIncTime) {
                    float ratio = (deltaTime - _args.ComboCircleStartTime) / _args.ComboCircleScaleIncTime;
                    scale = Mathf.Lerp(0f, _args.ComboCircleMaxScale, ratio);

                    float endTime = _args.ComboCircleStartTime + _args.ComboCircleScaleIncTime;
                    alpha = Mathf.InverseLerp(endTime, _args.ComboCircleFadeOutStartTime, deltaTime);
                }
                else {
                    scale = alpha = 0f;
                }
                _shockWaveCircleImage.transform.localScale = new Vector3(scale, scale, scale);
                _shockWaveCircleImage.color = Color.white.WithAlpha(alpha);
            }
            // Shock Wave Strike
            {
                float x;
                float alpha;

                if (deltaTime <= _args.ComboShockWaveAlphaIncTime) {
                    float ratio = deltaTime / _args.ComboShockWaveAlphaIncTime;
                    x = _args.ComboShockWaveStartX;
                    alpha = ratio;
                }
                else if (deltaTime < _args.ComboShockWaveAlphaIncTime + _args.ComboShockWaveMoveTime) {
                    float ratio = (deltaTime - _args.ComboShockWaveAlphaIncTime) / _args.ComboShockWaveMoveTime;
                    x = Mathf.Lerp(_args.ComboShockWaveStartX, _args.ComboShockWaveEndX, ratio);
                    alpha = 1f;
                }
                else if (deltaTime < _args.ComboShockWaveAlphaIncTime + _args.ComboShockWaveMoveTime + _args.ComboShockWaveAlphaDecTime) {
                    float ratio = (deltaTime - _args.ComboShockWaveAlphaIncTime - _args.ComboShockWaveMoveTime) / _args.ComboShockWaveAlphaDecTime;
                    x = _args.ComboShockWaveEndX;
                    alpha = 1 - ratio;
                }
                else {
                    x = 0;
                    alpha = 0;
                }
                _shockWaveImage.transform.localPosition = new(x, 0f, 0f);
                _shockWaveImage.color = Color.white.WithAlpha(alpha);
            }
            // Charming
            {
                float scale;
                float alpha;
                if (deltaTime <= _args.ComboCharmingGrowTime) {
                    float ratio = deltaTime / _args.ComboCharmingGrowTime;
                    scale = Mathf.Lerp(1f, _args.ComboCharmingMaxScaleY, ratio);
                }
                else if (deltaTime <= _args.ComboCharmingGrowTime + _args.ComboCharmingFadeTime) {
                    float ratio = (deltaTime - _args.ComboCharmingGrowTime) / _args.ComboCharmingFadeTime;
                    scale = Mathf.Lerp(_args.ComboCharmingMaxScaleY, 1f, ratio);
                }
                else {
                    scale = 0f;
                }

                if (deltaTime <= _args.ComboCharmingAlphaIncTime) {
                    float ratio = deltaTime / _args.ComboCharmingAlphaIncTime;
                    alpha = Mathf.Lerp(0f, 1f, ratio);
                }
                else if (deltaTime <= _args.ComboCharmingAlphaDecStartTime) {
                    alpha = 1f;
                }
                else if (deltaTime <= _args.ComboCharmingAlphaDecStartTime + _args.ComboCharmingAlphaDecTime) {
                    float ratio = (deltaTime - _args.ComboCharmingAlphaDecStartTime) / _args.ComboCharmingAlphaDecTime;
                    alpha = Mathf.Lerp(1f, 0f, ratio);
                }
                else {
                    alpha = 0f;
                }
                _charmingImage.transform.localScale = new Vector3(1f, scale, 1f);
                _charmingImage.color = Color.white.WithAlpha(alpha);
            }
        }
    }
}