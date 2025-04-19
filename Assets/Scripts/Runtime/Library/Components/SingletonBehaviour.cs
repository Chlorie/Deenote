#nullable enable

using UnityEngine;

namespace Deenote.Library.Components
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Instance
        {
            [System.Diagnostics.DebuggerStepThrough]
            get {
#if DEBUG
                if (_instance is null)
                    Debug.LogError($"The reference to {typeof(T).Name} is still null. " +
                                   "Is the script added into the scene? Is base.Awake() called?");
#endif
                return _instance!;
            }
        }

        protected virtual void Awake()
        {
            T instance = (T)this;
#if DEBUG
            if (_instance is null)
                _instance = instance;
            else {
                Destroy(instance);
                Debug.LogError($"Unexpected multiple instances of {typeof(T).Name}.");
            }
#else
            _instance = instance;
#endif
        }

        private static T? _instance;
    }

    public abstract class PersistentSingletonBehavior<T> : MonoBehaviour where T : PersistentSingletonBehavior<T>
    {
        public static T Instance => _instance ??= UnityUtils.CreatePersistentComponent<T>();
        private static T? _instance;
    }
}