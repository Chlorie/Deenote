using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Deenote.ApplicationManaging
{
    /// <summary>
    /// The project-global installer, responsible for building the global container.
    /// </summary>
    public sealed class ProjectInstaller : MonoBehaviour, IInstaller
    {
        public void InstallBindings(ContainerBuilder builder)
        {
            InitializeHelpers();
            builder.AddSingleton(typeof(UnhandledExceptionHandler));
        }

        private static ProjectInstaller? _instance;
        private GameObject _persistentObject = null!;

        /// <summary>
        /// Check the uniqueness of the instance, and create a helper <see cref="GameObject"/>
        /// for registering <see cref="MonoBehaviour"/> services.
        /// </summary>
        private void InitializeHelpers()
        {
#if DEBUG
            if (_instance is null)
                _instance = this;
            else {
                Debug.LogError($"There may not be multiple {nameof(ProjectInstaller)} instances at the same time.");
                Destroy(this);
            }
#else
            _instance = this;
#endif
            _persistentObject = new GameObject("PersistentObject") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(_persistentObject);
        }

        private void AddSingletonComponent<T>(ContainerBuilder builder) where T : Component =>
            builder.AddSingleton(container =>
            {
                var component = _persistentObject.AddComponent<T>();
                AttributeInjector.Inject(component, container);
                return component;
            });
    }
}