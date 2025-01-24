#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Library;
using Deenote.Library.Components;
using Deenote.UIFramework.Controls;
using System;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class StatusBar : MonoBehaviour
    {
        [SerializeField] TextBlock _messageText = default!;
        [SerializeField] TextBlock _fpsText = default!;

        private bool _isFpsUpdate;
        private int _fpsFrameCount;
        private float _fpsTimer;

        #region LocalizedTextKeys

        private const string FpsStatusFormatKey = "StatusBar_Fps";

        #endregion

        public void SetStatusMessage(LocalizableText message, ReadOnlySpan<string> args = default)
            => _messageText.SetText(message, args);

        public void SetLocalizedStatusMessage(string key, ReadOnlySpan<string> args = default)
            => SetStatusMessage(LocalizableText.Localized(key), args);

        public UniTaskVoid ShowToastAsync(LocalizableText message, float duration_s)
        {
            // TODO: impl
            return default!;
        }

        public UniTaskVoid ShowLocalizedToastAsync(string key, float duration_s)
            => ShowToastAsync(LocalizableText.Localized(key), duration_s);

        private void Awake()
        {
            MainSystem.GlobalSettings.RegisterNotificationAndInvoke(
                MainSystem.Settings.NotificationFlag.FpsShown,
                settings => _fpsText.gameObject.SetActive(_isFpsUpdate = settings.IsFpsShown));
        }

        private void Update()
        {
            if (_isFpsUpdate) {
                _fpsFrameCount++;
                if (_fpsTimer.IncAndTryWrap(Time.deltaTime, 1f)) {
                    string fpsStr = _fpsFrameCount.ToString();
                    _fpsText.SetLocalizedText(FpsStatusFormatKey, fpsStr);
                    _fpsFrameCount = 0;
                }
            }
        }
    }
}