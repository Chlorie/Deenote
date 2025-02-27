#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Deenote.Core.Configurations
{
    public sealed class ConfigSerializer
    {
        private readonly string _configFilePath;
        private readonly string _configTempFilePath;

        public event Action<ConfigRegistration>? Saving;
        public event Action<ConfigReader>? Loaded;

        public ConfigSerializer(string configDirectoryPath, string configFileName)
        {
            _configFilePath = Path.Combine(configDirectoryPath, configFileName);
            _configTempFilePath = _configFilePath + ".tmp";
        }

        public void Save()
        {
            var configs = new ConfigRegistration();
            Saving?.Invoke(configs);

            var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
            File.WriteAllText(_configTempFilePath, json);
            if (File.Exists(_configFilePath))
                File.Delete(_configFilePath);
            File.Move(_configTempFilePath, _configFilePath);
        }

        public void Load()
        {
            if (File.Exists(_configFilePath)) {
                var json = File.ReadAllText(_configFilePath);
                var configs = JsonConvert.DeserializeObject<Dictionary<string, JToken?>>(json);
                Loaded?.Invoke(new ConfigReader(configs));
            }
            else {
                Loaded?.Invoke(new ConfigReader(null));
            }
        }

        public readonly struct ConfigReader
        {
            private readonly Dictionary<string, JToken?>? _configs;

            internal ConfigReader(Dictionary<string, JToken?>? configs)
            {
                _configs = configs;
            }

            public int GetInt32(string key, int defaultValue = default)
                => GetToken(key, JTokenType.Integer) is { } token ? (int)token : defaultValue;

            public bool GetBoolean(string key, bool defaultValue = default)
                => GetToken(key, JTokenType.Boolean) is { } token ? (bool)token : defaultValue;

            public T? GetObject<T>(string key, T? defaultValue = default!)
            {
                if (_configs is null)
                    return defaultValue;

                if (_configs.TryGetValue(key, out var value)) {
                    if (value is not null)
                        return value.ToObject<T>();
                }
                return defaultValue;
            }

            private JToken? GetToken(string key, JTokenType tokenType)
            {
                if (_configs is null)
                    return null;
                if (_configs.TryGetValue(key, out var token)) {
                    if (token?.Type == tokenType)
                        return token;
                }
                return null;
            }
        }

        public struct ConfigRegistration
        {
            private Dictionary<string, object>? _configs;

            internal IDictionary<string, object> Configs => _configs ??= new();

            public void Add(string key, int value) => AddInternal(key, value);
            public void Add(string key, bool value) => AddInternal(key, value);
            public void AddDictionary<TValue>(string key, IReadOnlyDictionary<string, TValue> dictionary) => AddInternal(key, dictionary);
            public void AddObject(string key, object obj) => AddInternal(key, obj);

            private void AddInternal(string key, object value)
            {
                Configs.Add(key, value);
            }
        }
    }
}