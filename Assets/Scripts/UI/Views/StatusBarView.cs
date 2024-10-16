using Deenote.Localization;
using Deenote.UI.ComponentModel;
using Deenote.Utilities;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Deenote.UI.Views
{
    public sealed class StatusBarView : MonoBehaviour, INotifyPropertyChange<StatusBarView, StatusBarView.NotifyProperty>
    {
        [SerializeField] LocalizedText _statusMessageText;
        [SerializeField] LocalizedText _fpsText;

        private bool _isFpsShown;
        public bool IsFpsShown
        {
            get => _isFpsShown;
            set {
                if (_isFpsShown == value)
                    return;

                _isFpsShown = value;
                _fpsText.gameObject.SetActive(value);
                _propertyChangedNotifier.Invoke(this, NotifyProperty.FpsShown);
                MainSystem.PreferenceWindow.NotifyIsFpsShownChanged(value);
            }
        }

        private float _fpsTimer;
        private int _fpsFrameCount;

        public void SetStatusMessage(LocalizableText message, ReadOnlySpan<string> args = default)
        {
            _statusMessageText.SetText(message, args);
        }

        private void Update()
        {
            if (IsFpsShown) {
                _fpsFrameCount++;
                if (_fpsTimer.IncAndTryWrap(Time.deltaTime, 1f)) {
                    string fpsStr = _fpsFrameCount.ToString();
                    _fpsText.SetLocalizedText("Status_Fps", fpsStr);
                    _fpsFrameCount = 0;
                }
            }
        }

        private readonly PropertyChangeNotifier<StatusBarView, NotifyProperty> _propertyChangedNotifier = new();
        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<StatusBarView> action)
            => _propertyChangedNotifier.AddListener(flag, action);

        public enum NotifyProperty
        {
            FpsShown
        }
    }
}