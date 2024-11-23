#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.ComponentModel;
using Deenote.Utilities;
using System;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class StatusBarView : MonoBehaviour
    {
        [SerializeField] LocalizedText _statusMessageText = default!;
        [SerializeField] LocalizedText _fpsText = default!;

        private bool _isFpsUpdate;
        private float _fpsTimer;
        private int _fpsFrameCount;

        public void SetStatusMessage(LocalizableText message, ReadOnlySpan<string> args = default)
        {
            _statusMessageText.SetText(message, args);
        }

        public UniTaskVoid ShowToastAsync(LocalizableText message, float duration_s)
        {
            // TODO: impl
            return default!;
        }

        private void Start()
        {
            MainSystem.GlobalSettings.RegisterPropertyChangeNotificationAndInvoke(
                MainSystem.Settings.NotifyProperty.FpsShown,
                settings => _fpsText.gameObject.SetActive(_isFpsUpdate = settings.IsFpsShown));
        }

        private void Update()
        {
            if (_isFpsUpdate) {
                _fpsFrameCount++;
                if (_fpsTimer.IncAndTryWrap(Time.deltaTime, 1f)) {
                    string fpsStr = _fpsFrameCount.ToString();
                    _fpsText.SetLocalizedText("Status_Fps", fpsStr);
                    _fpsFrameCount = 0;
                }
            }
        }
    }
}