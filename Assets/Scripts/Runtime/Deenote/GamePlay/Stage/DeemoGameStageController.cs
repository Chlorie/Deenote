#nullable enable

using Deenote.CoreApp.Project;
using Deenote.Library.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.GamePlay.Stage
{
    public sealed class DeemoGameStageController : GameStageControllerBase
    {
        [Header("Effect")]
        [SerializeField] SpriteRenderer _judgeLineBreathingEffectSpriteRenderer = default!;
        [SerializeField] SpriteRenderer _judgeLineHitEffectSpriteRenderer = default!;
        [SerializeField] Image _backgroundBreathingMaskImage = default!;
        [SerializeField] TMP_Text _staveMusicNameText = default!;

        [Header("Args")]
        [SerializeField] DeemoGameStageArgs _deemoArgs = default!;

        protected internal override void OnInstantiate(GamePlayManager manager)
        {
            base.OnInstantiate(manager);

            _manager.RegisterNotification(
                GamePlayManager.NotificationFlag.ActiveNoteUpdated,
                _OnActiveNotesUpdated);
            MainSystem.ProjectManager.RegisterNotification(
                ProjectManager.NotificationFlag.ProjectMusicName,
                ProjectManager.NotificationFlag.CurrentProject,
                _OnProjectNameChanged);
        }

        private void OnDestroy()
        {
            _manager.UnregisterNotification(
                GamePlayManager.NotificationFlag.ActiveNoteUpdated,
                _OnActiveNotesUpdated);
            MainSystem.ProjectManager.UnregisterNotification(
                ProjectManager.NotificationFlag.ProjectMusicName,
                ProjectManager.NotificationFlag.CurrentProject,
                _OnProjectNameChanged);
        }

        private void _OnActiveNotesUpdated(GamePlayManager manager)
        {
            manager.AssertChartLoaded();

            // Update judge line hit effect
            var previousHitNode = _manager.NotesManager.GetPreviousHitNote();
            if (previousHitNode is null) {
                _judgeLineHitEffectSpriteRenderer.color = Color.clear;
                return;
            }

            var hitTime = previousHitNode.Time;
            var deltaTime = manager.MusicPlayer.Time - hitTime;
            Debug.Assert(deltaTime >= 0);

            float alpha;
            if (deltaTime < _deemoArgs.JudgeLineHitEffectAlphaDecTime) {
                float x = deltaTime / _deemoArgs.JudgeLineHitEffectAlphaDecTime;
                alpha = Mathf.Pow(1 - x, 0.5f);
            }
            else {
                alpha = 0f;
            }
            _judgeLineHitEffectSpriteRenderer.color = Color.white with { a = alpha };
        }

        private void _OnProjectNameChanged(ProjectManager manager)
        {
            if (manager.CurrentProject is not null)
                _staveMusicNameText.text = manager.CurrentProject.MusicName;
        }

        protected override void OnIsStageEffectOnChanged(bool value)
        {
            base.OnIsStageEffectOnChanged(value);
            if (!value) {
                _judgeLineBreathingEffectSpriteRenderer.color = Color.clear;
                _backgroundBreathingMaskImage.color = Color.white;
            }
        }

        private void Update()
        {
            if (IsStageEffectOn)
                Update_StageEffect();
        }
        private void Update_StageEffect()
        {
            var time = Time.time;
            // Judgeline
            {
                var ratio = Mathf.Sin(time * (2f * Mathf.PI / _deemoArgs.JudgeLinePeriod));
                ratio = Mathf.InverseLerp(-1f, 1f, ratio);
                _judgeLineBreathingEffectSpriteRenderer.color = Color.white with { a = ratio };
            }

            // Background
            {
                var ratio = Mathf.Sin(time * (2f * Mathf.PI / _deemoArgs.BackgroundMaskPeriod));
                ratio = Mathf.InverseLerp(-1f, 1f, ratio);
                _backgroundBreathingMaskImage.color = Color.white with { a = Mathf.Lerp(_deemoArgs.BackgroundMaskMinAlpha, _deemoArgs.BackgroundMaskMaxAlpha, ratio) };
            }

            // if (lightEffectState) stageLight.intensity = 2.5f + 2.5f * Mathf.Sin(2 * currentTime);

            // Note: DEEMO 4.x have different background effect from 3.x,
            // The above code is the effect that Chlorie made, which is more like 3.x ver, so we remain it here.
            // Also the commented line in StopBackgroundBreatheEffect()

            // Related game objects: Stage.Spotlight, Stage.StagePlane, 
            // Related material: White

            // Note: Change scale of bg image to simulate 3.x effect, the above code
            // is no longer needed.
        }
    }
}