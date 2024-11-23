#nullable enable

using Deenote.GameStage;
using Deenote.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Views.Elements
{
    public sealed class PerspectiveViewComboController : MonoBehaviour
    {
        [SerializeField] TMP_Text _numberText = default!;
        [SerializeField] TMP_Text _shadowText = default!;
        [SerializeField] Image _shockWaveCircleImage = default!;
        [SerializeField] Image _shockWaveImage = default!;
        [SerializeField] Image _charmingImage = default!;

        internal void UpdateComboRegistrant(GameStageController stage)
        {
            int combo = stage.CurrentCombo;
            if (combo < MainSystem.Args.GameStageViewArgs.MinDisplayCombo) {
                gameObject.SetActive(false);
                return;
            }

            // prevHitNoteIndex wont smaller than combo, so here
            // it is asserted a valid index
            var prevHitNote = stage.PrevHitNote!;
            Debug.Assert(prevHitNote?.IsComboNote() ?? false);

            gameObject.SetActive(true);
            var deltaTime = stage.CurrentMusicTime - prevHitNote!.Time;
            Debug.Assert(deltaTime >= 0, $"actual delta time:{deltaTime}");
            _numberText.text = _shadowText.text = combo.ToString();

            var args = MainSystem.Args.GameStageViewArgs;
            // Number
            {
                float grey;
                if (deltaTime <= args.ComboNumberGreyIncTime) {
                    float ratio = deltaTime / args.ComboNumberGreyIncTime;
                    grey = Mathf.Lerp(0f, 1f, ratio);
                }
                else if (deltaTime <= args.ComboNumberGreyIncTime + args.ComboNumberGreyDecTime) {
                    float ratio = (deltaTime - args.ComboNumberGreyIncTime) / args.ComboNumberGreyDecTime;
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
                if (deltaTime <= args.ComboShadowDuration) {
                    float ratio = deltaTime / args.ComboShadowDuration;
                    alpha = Mathf.Lerp(1f, args.ComboShadowMinAlpha, ratio);
                    scale = Mathf.Lerp(1f, args.ComboShadowMaxScale, ratio);
                }
                else {
                    alpha = args.ComboShadowMinAlpha;
                    scale = args.ComboShadowMaxScale;
                }
                _shadowText.transform.localScale = new Vector3(scale, scale, scale);
                _shadowText.color = Color.black with { a = alpha };
            }
            // Shock Wave Circle
            {
                float alpha;
                float scale;
                if (deltaTime <= args.ComboCircleStartTime) {
                    scale = alpha = 0f;
                }
                else if (deltaTime <= args.ComboCircleStartTime + args.ComboCircleScaleIncTime) {
                    float ratio = (deltaTime - args.ComboCircleStartTime) / args.ComboCircleScaleIncTime;
                    scale = Mathf.Lerp(0f, args.ComboCircleMaxScale, ratio);

                    float endTime = args.ComboCircleStartTime + args.ComboCircleScaleIncTime;
                    alpha = Mathf.InverseLerp(endTime, args.ComboCircleFadeOutStartTime, deltaTime);
                }
                else {
                    scale = alpha = 0f;
                }
                _shockWaveCircleImage.transform.localScale = new Vector3(scale, scale, scale);
                _shockWaveCircleImage.color = Color.white with { a = alpha };
            }
            // Shock Wave Strike
            {
                float x;
                float alpha;

                if (deltaTime <= args.ComboShockWaveAlphaIncTime) {
                    float ratio = deltaTime / args.ComboShockWaveAlphaIncTime;
                    x = args.ComboShockWaveStartX;
                    alpha = ratio;
                }
                else if (deltaTime < args.ComboShockWaveAlphaIncTime + args.ComboShockWaveMoveTime) {
                    float ratio = (deltaTime - args.ComboShockWaveAlphaIncTime) / args.ComboShockWaveMoveTime;
                    x = Mathf.Lerp(args.ComboShockWaveStartX, args.ComboShockWaveEndX, ratio);
                    alpha = 1f;
                }
                else if (deltaTime < args.ComboShockWaveAlphaIncTime + args.ComboShockWaveMoveTime +
                args.ComboShockWaveAlphaDecTime) {
                    float ratio = (deltaTime - args.ComboShockWaveAlphaIncTime - args.ComboShockWaveMoveTime) /
                                  args.ComboShockWaveAlphaDecTime;
                    x = args.ComboShockWaveEndX;
                    alpha = 1 - ratio;
                }
                else {
                    x = 0;
                    alpha = 0;
                }
                _shockWaveImage.transform.localPosition = new(x, 0f, 0f);
                _shockWaveImage.color = Color.white with { a = alpha };
            }
            // Charming
            {
                float scale;
                float alpha;
                if (deltaTime <= args.ComboCharmingGrowTime) {
                    float ratio = deltaTime / args.ComboCharmingGrowTime;
                    scale = Mathf.Lerp(1f, args.ComboCharmingMaxScaleY, ratio);
                }
                else if (deltaTime <= args.ComboCharmingGrowTime + args.ComboCharmingFadeTime) {
                    float ratio = (deltaTime - args.ComboCharmingGrowTime) / args.ComboCharmingFadeTime;
                    scale = Mathf.Lerp(args.ComboCharmingMaxScaleY, 1f, ratio);
                }
                else {
                    scale = 0f;
                }

                if (deltaTime <= args.ComboCharmingAlphaIncTime) {
                    float ratio = deltaTime / args.ComboCharmingAlphaIncTime;
                    alpha = Mathf.Lerp(0f, 1f, ratio);
                }
                else if (deltaTime <= args.ComboCharmingAlphaDecStartTime) {
                    alpha = 1f;
                }
                else if (deltaTime <= args.ComboCharmingAlphaDecStartTime + args.ComboCharmingAlphaDecTime) {
                    float ratio = (deltaTime - args.ComboCharmingAlphaDecStartTime) / args.ComboCharmingAlphaDecTime;
                    alpha = Mathf.Lerp(1f, 0f, ratio);
                }
                else {
                    alpha = 0f;
                }
                _charmingImage.transform.localScale = new Vector3(1f, scale, 1f);
                _charmingImage.color = Color.white with { a = alpha };
            }
        }
    }
}