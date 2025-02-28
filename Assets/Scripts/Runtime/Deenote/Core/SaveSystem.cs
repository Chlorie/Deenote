#nullable enable

using Deenote.Library;
using Deenote.Systems.Configurations;
using System;
using UnityEngine;

namespace Deenote.Core
{
    public sealed class SaveSystem : MonoBehaviour
    {
        private const float AutoSaveIntervalTime_s = 5 * 60f;

        private float _autoSaveTimer;
        private bool _isAutoSaveOn;

        public bool IsAutoSaveOn
        {
            get => _isAutoSaveOn;
            set {
                if (Utils.SetField(ref _isAutoSaveOn, value)) {
                    if (value is false)
                        _autoSaveTimer = 0f;
                }
            }
        }

        private ConfigSerializer _configSerializer = new(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "config.json");

        public event Action? AutoSaving;
        public event Action<ConfigSerializer.ConfigRegistration>? SavingConfigurations
        {
            add => _configSerializer.Saving += value;
            remove => _configSerializer.Saving -= value;
        }
        public event Action<ConfigSerializer.ConfigReader>? LoadedConfigurations
        {
            add => _configSerializer.Loaded += value;
            remove => _configSerializer.Loaded -= value;
        }

        public void SaveConfigurations()
        {
            _configSerializer.Save();
        }

        public void LoadConfigurations()
        {
            _configSerializer.Load();
        }

        private void Update()
        {
            if (IsAutoSaveOn) {
                if (_autoSaveTimer.IncAndTryWrap(Time.unscaledDeltaTime, AutoSaveIntervalTime_s)) {
                    AutoSaving?.Invoke();
                    SaveConfigurations();
                }
            }
        }
    }
}