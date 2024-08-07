using Deenote.GameStage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows
{
    partial class PerspectiveViewWindow
    {
        [Header("UI Combo")]
        [SerializeField] GameObject _comboParentGameObject;
        [SerializeField] TMP_Text _comboNumberText;
        [SerializeField] TMP_Text _comboShadowText;
        [SerializeField] Image _shockWaveCircleImage;
        [SerializeField] Image _shockWaveImage;
        [SerializeField] Image _charmingImage;

        private void UpdateScore(int judgedNoteCount)
        {
            if (judgedNoteCount <= 0) {
                _scoreText.text = "0.00 %";
                return;
            }

            // TODO: If both judged count and total count not changed
            if (_judgedNoteCount == judgedNoteCount)
                return;

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
            var deltaTime = GameStageController.Instance.CurrentMusicTime - _chart.Notes[prevHitNoteIndex].Data.Time;
            Debug.Assert(deltaTime >= 0, $"actual delta time:{deltaTime}");
            _comboNumberText.text = _comboShadowText.text = combo.ToString();

            // TODO:我觉得还需要调
            // Number
            {
                const float NumberGreyIncTime = 0.125f;
                const float NumberGreyDecTime = 0.5f;
                float grey;
                if (deltaTime <= NumberGreyIncTime) {
                    float ratio = deltaTime / NumberGreyIncTime;
                    grey = Mathf.Pow(1f - ratio, 0.67f);
                }
                else if (deltaTime <= NumberGreyIncTime + NumberGreyDecTime) {
                    float ratio = (deltaTime - NumberGreyIncTime) / NumberGreyDecTime;
                    grey = Mathf.Pow(1f - ratio, 0.67f);
                }
                else {
                    grey = 0f;
                }
                _comboNumberText.color = new Color(grey, grey, grey, 1f);
            }
            // Shadow Number
            {
                const float ShadowMaxTime = 0.4f;
                const float ShadowMinAlpha = 0.1f;
                const float ShadowMaxScale = 1.5f;
                float alpha;
                float scale;
                if (deltaTime <= ShadowMaxTime) {
                    float ratio = deltaTime / ShadowMaxTime;
                    alpha = Mathf.Lerp(1f, ShadowMinAlpha, ratio);
                    scale = Mathf.Lerp(1f, ShadowMaxScale, ratio);
                }
                else {
                    alpha = ShadowMinAlpha;
                    scale = ShadowMaxScale;
                }
                _comboShadowText.transform.localScale = new Vector3(scale, scale, scale);
                _comboShadowText.color = new Color(0f, 0f, 0f, alpha);
            }
            // Shock Wave Circle
            {
                const float ShockWaveCircleTotalTime = 0.25f;
                const float ShockWaveCircleStartTime = 0.15f;
                const float ShockWaveCircleGrowTime = ShockWaveCircleTotalTime - ShockWaveCircleStartTime;
                const float ShockWaveCircleFadeOutTime = 0.05f;
                const float ShockWaveCircleMaxScale = 2.2f;
                float alpha;
                float scale;
                if (deltaTime > ShockWaveCircleStartTime && deltaTime <= ShockWaveCircleStartTime + ShockWaveCircleGrowTime) {
                    float ratio = (deltaTime - ShockWaveCircleStartTime) / ShockWaveCircleGrowTime;
                    scale = Mathf.Lerp(0f, ShockWaveCircleMaxScale, ratio);
                    if (deltaTime <= ShockWaveCircleTotalTime - ShockWaveCircleFadeOutTime) {
                        alpha = 1f;
                    }
                    else {
                        var aratio = (deltaTime - (ShockWaveCircleTotalTime - ShockWaveCircleFadeOutTime)) / ShockWaveCircleFadeOutTime;
                        alpha = Mathf.Lerp(1f, 0f, aratio);
                    }
                }
                else {
                    scale = 0f;
                    alpha = 0f;
                }
                _shockWaveCircleImage.transform.localScale = new Vector3(scale, scale, scale);
                _shockWaveCircleImage.color = new Color(1f, 1f, 1f, alpha);
            }
            // Shock Wave Strike
            {
                const float ShockWaveTotalTime = 0.25f;
                const float ShockWaveMoveTime = ShockWaveTotalTime / 3f;
                const float ShockWaveFadeInTime = (ShockWaveTotalTime - ShockWaveMoveTime) / 2f;
                const float ShockWaveFadeOutTime = ShockWaveFadeInTime;
                const float ShockWaveStartX = 72f;
                const float ShockWaveEndX = -ShockWaveStartX;
                float x;
                float alpha;

                if (deltaTime <= ShockWaveFadeInTime) {
                    float ratio = deltaTime / ShockWaveFadeInTime;
                    x = ShockWaveStartX;
                    alpha = ratio;
                }
                else if (deltaTime <= ShockWaveFadeInTime + ShockWaveMoveTime) {
                    float ratio = (deltaTime - ShockWaveFadeInTime) / ShockWaveMoveTime;
                    x = Mathf.Lerp(ShockWaveStartX, ShockWaveEndX, ratio);
                    alpha = 1f;
                }
                else if (deltaTime < ShockWaveTotalTime) {
                    float ratio = (deltaTime - ShockWaveFadeInTime - ShockWaveMoveTime) / ShockWaveFadeOutTime;
                    x = ShockWaveEndX;
                    alpha = 1 - ratio;
                }
                else {
                    x = 0f;
                    alpha = 0f;
                }
                _shockWaveImage.transform.localPosition = new(x, 0f, 0f);
                _shockWaveImage.color = new Color(1f, 1f, 1f, alpha);
            }
            // Charming
            {
                const float CharmingIncTime = 0.075f;
                const float CharmingDecTime = 0.225f;
                const float CharmingMaxScale = 1.25f;
                float scale;
                float alpha;
                if (deltaTime <= CharmingIncTime) {
                    float ratio = deltaTime / CharmingIncTime;
                    scale = Mathf.Lerp(1f, CharmingMaxScale, ratio);
                    alpha = 1f;
                }
                else if (deltaTime <= CharmingIncTime + CharmingDecTime) {
                    float ratio = (deltaTime - CharmingIncTime) / CharmingDecTime;
                    scale = Mathf.Lerp(CharmingMaxScale, 1f, ratio);
                    alpha = ratio <= 0.5f ? 1f : Mathf.Lerp(2f, 0f, ratio);
                }
                else {
                    scale = 0f;
                    alpha = 0f;
                }
                _charmingImage.transform.localScale = new Vector3(1f, scale, 1f);
                _charmingImage.color = new Color(1f, 1f, 1f, alpha);
            }
        }
    }
}