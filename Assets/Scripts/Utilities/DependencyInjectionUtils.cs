#nullable enable

using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;

namespace Deenote.Utilities
{
    public static class DependencyInjectionUtils
    {
        public static ContainerBuilder AddSingletonComponent<T>(
            this ContainerBuilder builder, GameObject parent) where T : Component
        {
            builder.AddSingleton(container =>
            {
                var component = parent.AddComponent<T>();
                AttributeInjector.Inject(component, container);
                return component;
            });
            return builder;
        }
    }
}