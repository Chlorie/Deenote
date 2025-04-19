#nullable enable

using Deenote.Systems.Configurations;
using System;
using UnityEngine;

namespace Deenote.Core
{
    public sealed class SaveSystem
    {
        private ConfigSerializer _configSerializer = new(Application.persistentDataPath, "config.json");

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
    }
}