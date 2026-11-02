#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Core.Project;
using Deenote.Entities;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Components;
using Deenote.Library.Mathematics;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.GamePlay.UI
{
    public sealed class DeemoPerspectiveViewForegroundPanel : PerspectiveViewForegroundBase
    {
        [Header("Info Bar")]
        [SerializeField] TMP_Text _musicNameText = default!;
        [SerializeField] TMP_Text _scoreText = default!;
        [SerializeField] Slider _timeSlider = default!;
        [SerializeField] Image _difficultyImage = default!;
        [SerializeField] TMP_Text _levelText = default!;
        [SerializeField] Button _pauseButton = default!;
        [Header("Combo UI")]
        [SerializeField] GameObject _comboGameObject = default!;
        [SerializeField] TextMeshProUGUI _numberText = default!;
        [SerializeField] TextMeshProUGUI _shadowText = default!;
        [SerializeField] Image _shockWaveCircleImage = default!;
        [SerializeField] Image _shockWaveImage = default!;
        [SerializeField] RectTransform _shockWaveEnterPosTransform = default!;
        [SerializeField] RectTransform _shockWaveExitPosTransform = default!;
        [SerializeField] Image _charmingImage = default!;
        [Header("Speed Change Warning UI")]
        [SerializeField] List<Image> _speedChangeWarningImages = new();

        [Header("Resources")]
        [SerializeField] DeemoGameStageUIArgs _args = default!;

        private float _shockWaveEnterPosX;
        private float _shockWaveExitPosX;

        private Vector3[] _speedChangeWarningBaseScales = Array.Empty<Vector3>();
        private Color[] _speedChangeWarningBaseColors = Array.Empty<Color>();

        private Difficulty _difficulty_bf;
        private Difficulty Difficulty
        {
            get => _difficulty_bf;
            set {
                if (Utils.SetField(ref _difficulty_bf, value)) {
                    (_difficultyImage.sprite, _levelText.color) = base.Args.GetDifficultyArgs(value);
                    UpdateLevelText();
                }
            }
        }

        private string? _level_bf;
        private string Level
        {
            get => _level_bf!;
            set {
                if (Utils.SetField(ref _level_bf, value)) {
                    UpdateLevelText();
                }
            }
        }


        protected override void Awake()
        {
            _shockWaveEnterPosX = _shockWaveEnterPosTransform.anchorMin.x;
            _shockWaveExitPosX = _shockWaveExitPosTransform.anchorMin.x;

            _speedChangeWarningBaseScales = new Vector3[_speedChangeWarningImages.Count];
            _speedChangeWarningBaseColors = new Color[_speedChangeWarningImages.Count];
            for (int i = 0; i < _speedChangeWarningImages.Count; i++) {
                var image = _speedChangeWarningImages[i];
                if (image is null)
                    continue;
                _speedChangeWarningBaseScales[i] = image.transform.localScale;
                _speedChangeWarningBaseColors[i] = image.color;
                image.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            _timeSlider.onValueChanged.AddListener(val => MainSystem.GamePlayManager.MusicPlayer.Time = val);
            _pauseButton.onClick.AddListener(() => MainSystem.GamePlayManager.MusicPlayer.TogglePlayingState());

            MainSystem.GamePlayManager.MusicPlayer.ClipChanged += clip => _timeSlider.maxValue = clip.length;
            MainSystem.GamePlayManager.MusicPlayer.TimeChanged += args => _timeSlider.SetValueWithoutNotify(args.NewTime);
            _timeSlider.maxValue = MainSystem.GamePlayManager.MusicPlayer.ClipLength;
            _timeSlider.SetValueWithoutNotify(MainSystem.GamePlayManager.MusicPlayer.Time);

            MainSystem.ProjectManager.RegisterNotification(
                ProjectManager.NotificationFlag.ProjectMusicName,
                manager =>
                {
                    manager.AssertProjectLoaded();
                    _musicNameText.text = manager.CurrentProject.MusicName;
                });

            MainSystem.GamePlayManager.RegisterNotification(
                GamePlayManager.NotificationFlag.ChartLevel,
                manager =>
                {
                    manager.AssertChartLoaded();
                    Level = manager.CurrentChart.Level;
                });

            MainSystem.GamePlayManager.RegisterNotification(
                GamePlayManager.NotificationFlag.ChartDifficulty,
                manager =>
                {
                    manager.AssertChartLoaded();
                    Difficulty = manager.CurrentChart.Difficulty;
                });

            MainSystem.ProjectManager.RegisterNotificationAndInvoke(
                ProjectManager.NotificationFlag.CurrentProject,
                manager =>
                {
                    if (manager.IsProjectLoaded())
                        _musicNameText.text = manager.CurrentProject.MusicName;
                });

            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.CurrentChart,
                manager =>
                {
                    if (manager.CurrentChart is { } chart) {
                        Level = manager.CurrentChart.Level;
                        Difficulty = manager.CurrentChart.Difficulty;
                        gameObject.SetActive(true);
                    }
                    else {
                        gameObject.SetActive(false);
                    }
                    UpdateSpeedChangeWarningRegistrant(manager);
                });

            MainSystem.GamePlayManager.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.ActiveNoteUpdated,
                manager =>
                {
                    if (manager.CurrentChart is not { } chart)
                        return;

                    UpdateComboRegistrant(manager);
                    UpdateSpeedChangeWarningRegistrant(manager);

                    var currentCombo = manager.NotesManager.CurrentCombo;
                    if (currentCombo <= 0) {
                        _scoreText.text = "0.00 %";
                        return;
                    }

                    int noteCount = chart.NoteCount;
                    float accScore = (float)currentCombo / noteCount;
                    // comboActual = Sum(1..judgeNoteCount);
                    // comboTotal = Sum(1..noteCount)
                    // comboScore = comboActual / comboTotal
                    //            = ((1 + judged) * judged) / ((1 + count) * count)
                    float comboScore = (float)((1 + currentCombo) * currentCombo) / ((1 + noteCount) * noteCount);

                    float score = accScore * 80_00f + comboScore * 20_00f;
                    _scoreText.text = $"{Mathf.Floor(score) / 100f:F2} %";
                });

            MainSystem.GamePlayManager.RegisterNotification(
                GamePlayManager.NotificationFlag.ActiveSpeedChangeWarningUpdated,
                UpdateSpeedChangeWarningRegistrant);
        }

        private void UpdateSpeedChangeWarningRegistrant(GamePlayManager stage)
        {
            if (stage.CurrentChart is not { } chart) {
                SetSpeedChangeWarningImagesActive(false);
                return;
            }

            var currentTime = stage.MusicPlayer.Time;
            var warnings = chart.SpeedChangeWarnings.AsSpan();
            int warningIndex = FindPreviousWarningIndex(warnings, currentTime);
            float duration = _args.SpeedChangeWarningPulseDuration;
            if (warningIndex < 0 || duration <= 0f) {
                SetSpeedChangeWarningImagesActive(false);
                return;
            }

            float elapsed = currentTime - warnings[warningIndex].Time;
            if (elapsed < 0f || elapsed >= duration) {
                SetSpeedChangeWarningImagesActive(false);
                return;
            }

            SetSpeedChangeWarningImagesActive(true);

            int pulseCount = Mathf.Max(1, _args.SpeedChangeWarningPulseCount);
            float pulseDuration = duration / pulseCount;
            int pulseIndex = Mathf.Min((int)(elapsed / pulseDuration), pulseCount - 1);
            float pulseRatio = Mathf.Clamp01((elapsed - pulseIndex * pulseDuration) / pulseDuration);

            // Eased instead of linear, so the scale grows and recovers smoothly
            float growRatio = pulseRatio < 0.5f ? pulseRatio * 2f : (1f - pulseRatio) * 2f;
            float scale = Mathf.Lerp(1f, _args.SpeedChangeWarningPulseMaxScale, Mathf.SmoothStep(0f, 1f, growRatio));
            float alpha = pulseIndex == pulseCount - 1
                ? 1f - Mathf.SmoothStep(0f, 1f, pulseRatio)
                : pulseIndex == 0
                    ? Mathf.SmoothStep(0f, 1f, Mathf.Min(pulseRatio * 2, 1))
                    : 1f;

            for (int i = 0; i < _speedChangeWarningImages.Count; i++) {
                var image = _speedChangeWarningImages[i];
                if (image is null)
                    continue;
                image.transform.localScale = _speedChangeWarningBaseScales[i] * scale;
                var color = _speedChangeWarningBaseColors[i];
                image.color = color with { a = color.a * alpha };
            }

            return;

            void SetSpeedChangeWarningImagesActive(bool active)
            {
                foreach (var image in _speedChangeWarningImages) {
                    if (image is not null && image.gameObject.activeSelf != active)
                        image.gameObject.SetActive(active);
                }
            }

            static int FindPreviousWarningIndex(ReadOnlySpan<SpeedChangeWarningModel> warnings, float time)
            {
                int left = 0, right = warnings.Length - 1;
                int result = -1;
                while (left <= right) {
                    int mid = (left + right) >> 1;
                    if (warnings[mid].Time <= time) {
                        result = mid;
                        left = mid + 1;
                    }
                    else {
                        right = mid - 1;
                    }
                }
                return result;
            }
        }

        private void UpdateComboRegistrant(GamePlayManager stage)
        {
            int combo = stage.NotesManager.CurrentCombo;
            if (combo < _args.MinDisplayCombo) {
                _comboGameObject.SetActive(false);
                return;
            }

            // prevHitNoteIndex wont smaller than combo, so here
            // it is asserted a valid index
            var prevHitNote = stage.NotesManager.GetPreviousHitComboNode();
            Debug.Assert(prevHitNote?.IsComboNode ?? false);

            _comboGameObject.SetActive(true);
            var deltaTime = stage.MusicPlayer.Time - prevHitNote!.Time;
            Debug.Assert(deltaTime >= 0, $"actual delta time:{deltaTime}");
            _numberText.text = _shadowText.text = combo.ToString();

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
                _numberText.color = new Color(grey, grey, grey);
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
                _shadowText.transform.localScale = new Vector3(scale, scale, scale);
                _shadowText.color = Color.black with { a = alpha };
            }
            // Shock Wave Circle
            {
                float alpha;
                float scale;
                if (deltaTime <= _args.ComboCircleScaleStartTime)
                    scale = 0f;
                else if (deltaTime <= _args.ComboCircleScaleEndTime)
                    scale = MathUtils.MapTo(deltaTime, _args.ComboCircleScaleStartTime, _args.ComboCircleScaleEndTime, 0f, _args.ComboCircleMaxScale);
                else
                    scale = 0f;

                if (deltaTime <= _args.ComboCircleFadeInStartTime)
                    alpha = 0f;
                else if (deltaTime <= _args.ComboCircleFadeInEndTime)
                    alpha = Mathf.InverseLerp(_args.ComboCircleScaleStartTime, _args.ComboCircleScaleEndTime, deltaTime);
                else if (deltaTime <= _args.ComboCircleFadeOutStartTime)
                    alpha = 1f;
                else
                    alpha = Mathf.InverseLerp(_args.ComboCircleScaleEndTime, _args.ComboCircleFadeOutStartTime, deltaTime);

                _shockWaveCircleImage.transform.localScale = new Vector3(scale, scale, scale);
                _shockWaveCircleImage.color = Color.white with { a = alpha };
            }
            // Shock Wave Strike
            {
                _shockWaveImage.rectTransform.WithAnchoredMinMaxX(GetStrikePosition(deltaTime));
                _shockWaveImage.WithColorAlpha(GetStrikeAlpha(deltaTime));

                float GetStrikePosition(float deltaTime)
                {
                    if (deltaTime <= _args.ComboShockWaveAlphaIncTime) {
                        return _shockWaveEnterPosX;
                    }
                    else if (deltaTime < _args.ComboShockWaveAlphaIncTime + _args.ComboShockWaveMoveTime) {
                        float ratio = (deltaTime - _args.ComboShockWaveAlphaIncTime) / _args.ComboShockWaveMoveTime;
                        return Mathf.Lerp(_shockWaveEnterPosX, _shockWaveExitPosX, ratio);
                    }
                    else {
                        return _shockWaveExitPosX;
                    }
                }

                float GetStrikeAlpha(float deltaTime)
                {
                    if (deltaTime <= _args.ComboShockWaveAlphaIncTime) {
                        float ratio = deltaTime / _args.ComboShockWaveAlphaIncTime;
                        return ratio;
                    }
                    else if (deltaTime < _args.ComboShockWaveAlphaIncTime + _args.ComboShockWaveMoveTime) {
                        return 1f;
                    }
                    else if (deltaTime < _args.ComboShockWaveAlphaIncTime + _args.ComboShockWaveMoveTime +
                    _args.ComboShockWaveAlphaDecTime) {
                        float ratio = (deltaTime - _args.ComboShockWaveAlphaIncTime - _args.ComboShockWaveMoveTime) /
                                      _args.ComboShockWaveAlphaDecTime;
                        return 1 - ratio;
                    }
                    else {
                        return 0;
                    }
                }
            }
            // Charming
            {
                float scale;
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
                _charmingImage.transform.localScale = new Vector3(1f, scale, 1f);

                float alpha;
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
                _charmingImage.color = Color.white with { a = alpha };
            }
        }

        private void UpdateLevelText()
        {
            _levelText.text = $"{Difficulty.ToDisplayString()} Lv {Level}";
        }
    }
}