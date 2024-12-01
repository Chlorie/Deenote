#nullable enable

using Deenote.Settings;
using Deenote.UI;
using Deenote.UI.ComponentModel;
using Deenote.UI.Themes;
using System;
using UnityEngine;

namespace Deenote
{
    partial class MainSystem
    {
        [Header("Args")]
        [SerializeField] GameStageViewArgs _gameStageViewArgs = default!;
        [SerializeField] UIIcons _uiIcons = default!;
        [SerializeField] UIColors _uiColors = default!;
        [SerializeField] UIPrefabs _uIPrefabs = default!;
        partial class Args
        {
            public static GameStageViewArgs GameStageViewArgs => Instance._gameStageViewArgs;

#if UNITY_EDITOR
            private static T GetScriptableObject<T>() where T : ScriptableObject => UnityEditor.AssetDatabase.LoadAssetAtPath<T>($"Assets/ScriptableObjects/UI/{typeof(T).Name}.asset");
#endif

            public static UIIcons UIIcons
#if UNITY_EDITOR
                => GetScriptableObject<UIIcons>();
#else
                => Instance._uiIcons;
#endif
            public static UIColors UIColors
#if UNITY_EDITOR
            { get; } = GetScriptableObject<UIColors>();
#else
                => Instance._uiColors;
#endif
            public static UIPrefabs UIPrefabs
#if UNITY_EDITOR
            { get; } = GetScriptableObject<UIPrefabs>();
#else
                => Instance._uiPrefabs;
#endif
        }

        public sealed class Settings : INotifyPropertyChange<Settings, Settings.NotifyProperty>
        {
            private bool _isVSyncOn;
            public bool IsVSyncOn
            {
                get => _isVSyncOn;
                set {
                    if (_isVSyncOn == value)
                        return;
                    _isVSyncOn = value;
                    QualitySettings.vSyncCount = _isVSyncOn ? 1 : 0;
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.VSync);
                }
            }

            private bool _isIneffectivePropertiesVisible;
            public bool IsIneffectivePropertiesVisible
            {
                get => _isIneffectivePropertiesVisible;
                set {
                    if (_isIneffectivePropertiesVisible == value)
                        return;

                    _isIneffectivePropertiesVisible = value;
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.IneffectivePropertiesVisiblility);
                }
            }

            private bool _isFpsShown;
            public bool IsFpsShown
            {
                get => _isFpsShown;
                set {
                    if (_isFpsShown == value)
                        return;

                    _isFpsShown = value;
                    _propertyChangeNotifier.Invoke(this, NotifyProperty.FpsShown);
                }
            }

            private PropertyChangeNotifier<Settings, NotifyProperty> _propertyChangeNotifier = new();
            public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<Settings> action)
                => _propertyChangeNotifier.AddListener(flag, action);

            public enum NotifyProperty
            {
                VSync,
                IneffectivePropertiesVisiblility,
                FpsShown,
            }
        }
    }
}