using Deenote.Audio;
using Deenote.Inputting;
using Deenote.Localization;
using Deenote.UI.Windows;
using Deenote.Utilities;
using Reflex.Core;
using UnityEngine;

namespace Deenote.ApplicationManaging
{
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder) => builder
            .AddSingleton(_fontSetter)
            .AddSingleton(_musicController)
            .AddSingleton(_windowsManager)
            .AddSingletonComponent<KeyBindingManager>(gameObject);

        [SerializeField] private SystemFontFallbackSetter _fontSetter = null!;
        [SerializeField] private MusicController _musicController = null!;
        [SerializeField] private WindowsManager _windowsManager = null!;
    }
}