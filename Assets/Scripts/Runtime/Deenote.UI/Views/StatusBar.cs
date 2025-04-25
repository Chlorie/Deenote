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

        #region LocalizedTextKeys

        private const string ReadyTextKey = "StatusBar_ReadyText";
        private const string FpsStatusFormatKey = "StatusBar_Fps";

        #endregion

        public void SetStatusMessage(LocalizableText message, ReadOnlySpan<string> args = default)
            => _messageText.SetText(message, args);

        public void SetLocalizedStatusMessage(string key, ReadOnlySpan<string> args = default)
            => SetStatusMessage(LocalizableText.Localized(key), args);

        public void SetLocalizedStatusMessage(string key, string arg)
            => SetLocalizedStatusMessage(key, MemoryMarshal.CreateReadOnlySpan(ref arg, 1));

        public void SetRawTextStatusMessage(string text) => SetStatusMessage(LocalizableText.Raw(text));

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