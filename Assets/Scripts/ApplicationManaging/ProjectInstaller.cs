#nullable enable

using Deenote.Core;
using Deenote.Localization;
using Reflex.Core;
using UnityEngine;

namespace Deenote.ApplicationManaging
{
    /// <summary>
    /// The project-global installer, responsible for building the global container.
    /// </summary>
    public sealed class ProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder) => builder
            .AddSingleton(typeof(UnhandledExceptionHandler))
            .AddSingleton(typeof(LocalizationSystem));
    }
}