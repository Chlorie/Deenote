#nullable enable

using Deenote.Library;
using Deenote.Library.Components;
using Deenote.Library.Mathematics;
using Deenote.Localization;
using Deenote.UIFramework.Controls;
using System;
using System.Runtime.InteropServices;
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

        private float _autoSetReadyTimer;

        #region LocalizedTextKeys

        private const string ReadyTextKey = "StatusBar_ReadyText";
        private const string FpsStatusFormatKey = "StatusBar_Fps";

        #endregion

        private void SetStatusMessageInternal(LocalizableText message, ReadOnlySpan<string> args, float duration)
        {
            _messageText.SetText(message, args);
            _autoSetReadyTimer = duration;
        }

        public void SetStatusMessage(LocalizableText message, ReadOnlySpan<string> args = default, float duration = -1f)
            => SetStatusMessageInternal(message, args, duration);

        public void SetLocalizedStatusMessage(string key, ReadOnlySpan<string> args = default, float duration = -1f)
            => SetStatusMessage(LocalizableText.Localized(key), args, duration);

        public void SetLocalizedStatusMessage(string key, string arg, float duration = -1f)
            => SetLocalizedStatusMessage(key, MemoryMarshal.CreateReadOnlySpan(ref arg, 1), duration);

        public void SetRawTextStatusMessage(string text, float duration = -1f) => SetStatusMessage(LocalizableText.Raw(text), default, duration);

        public void SetReadyStatusMessage()
            => SetLocalizedStatusMessage(ReadyTextKey);

        private void Awake()
        {
            MainSystem.GlobalSettings.RegisterNotificationAndInvoke(
                GlobalSettings.NotificationFlag.FpsShown,
                settings => _fpsText.gameObject.SetActive(_isFpsUpdate = settings.IsFpsShown));
        }

        private void Start()
        {
            SetReadyStatusMessage();
        }

        private void Update()
        {
            Update_Fps();
            Update_AutoSetReady();
        }

        private void Update_Fps()
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

        private void Update_AutoSetReady()
        {
            if (_autoSetReadyTimer > 0f) {
                _autoSetReadyTimer -= Time.deltaTime;
                if (_autoSetReadyTimer <= 0) {
                    SetReadyStatusMessage();
                }
            }
        }
    }
}