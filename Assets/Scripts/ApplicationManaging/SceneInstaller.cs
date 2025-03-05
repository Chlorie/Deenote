#nullable enable

using Deenote.Audio;
using Deenote.Localization;
using Reflex.Core;
using UnityEngine;
using Deenote.Utilities;
using Deenote.Systems.Inputting;

namespace Deenote.ApplicationManaging
{
    public class SceneInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder) => builder
            .AddSingleton(_fontSetter)
            .AddSingletonComponent<KeyBindingManager>(gameObject);

        [SerializeField] private SystemFontFallbackSetter _fontSetter = null!;
    }
}