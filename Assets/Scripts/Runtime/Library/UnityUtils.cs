#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Library.Collections;
using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Deenote.Library
{
    public static class UnityUtils
    {
        public static void AddListener(this UnityEvent ev, Func<UniTaskVoid> uniTaskFunc)
            => ev.AddListener(UniTask.UnityAction(uniTaskFunc));

        public static Action Action(Func<UniTask> uniTaskFunc) => uniTaskFunc._ActionExt;

        private static void _ActionExt(this Func<UniTask> uniTaskFunc)
            => uniTaskFunc().Forget();

        public static void Resize(this RenderTexture texture, Vector2Int newSize)
        {
            int width = newSize.x, height = newSize.y;
            if (width <= 0 || height <= 0) return;
            if (texture.width == width && texture.height == height) return;
            texture.Release();
            texture.width = width;
            texture.height = height;
            texture.Create();
        }

        /// <summary>
        /// Cache a child component reference into the given reference.
        /// If the given reference is already set, it will be returned unmodified,
        /// otherwise the reference is updated with the child component.
        /// </summary>
        /// <remarks>This is used to simplify initializing component references in <see cref="GameObject"/>s.</remarks>
        /// <typeparam name="T">Child component type.</typeparam>
        /// <param name="component">The parent component.</param>
        /// <param name="childComponentRef">A reference to the child component cache.</param>
        /// <returns>The child component reference.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T MaybeGetComponent<T>(this Component component, ref T? childComponentRef) where T : Component
        {
            childComponentRef ??= component.GetComponent<T>();
#if DEBUG
            if (childComponentRef is null)
                Debug.LogError($"Missing component {typeof(T).Name} in {component}");
#endif
            return childComponentRef!;
        }

        public static T CreatePersistentComponent<T>() where T : Component => GlobalGameObject.AddComponent<T>();

        private static GameObject? _globalGameObject;

        private static GameObject GlobalGameObject
        {
            get {
                if (_globalGameObject is not null) return _globalGameObject;
                _globalGameObject = new GameObject("GlobalObject") { hideFlags = HideFlags.HideAndDontSave };
                UnityEngine.Object.DontDestroyOnLoad(_globalGameObject);
                return _globalGameObject;
            }
        }

        public static ObjectPool<T> CreateObjectPool<T>(T prefab, Transform? parentTransform = null,
            Action<T>? onCreate = null, int defaultCapacity = 10, int maxSize = 10000) where T : Component
            => CreateObjectPool(() =>
            {
                var item = UnityEngine.Object.Instantiate(prefab, parentTransform);
                onCreate?.Invoke(item);
                return item;
            }, defaultCapacity,
                maxSize);

        public static ObjectPool<T> CreateObjectPool<T>(Func<T> createFunc, int defaultCapacity = 10,
            int maxSize = 10000) where T : Component
            => new(createFunc,
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                obj =>
                {
                    obj.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(obj);
                },
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);

        /// <summary>
        /// Converts Unity fake <see langword="null"/>s into real <see langword="null"/>s,
        /// so that they can be used with null-coalescing operators.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="self">The component.</param>
        /// <returns>A valid Unity object or an actual <see langword="null"/>.</returns>
        public static T? CheckNull<T>(this T? self) where T : UnityEngine.Object => self != null ? self : null;

        public static void SetSiblingIndicesInOrder<T>(this PooledObjectListView<T> list) where T : Component
        {
            for (int i = 0; i < list.Count; i++) {
                list[i].transform.SetSiblingIndex(i);
            }
        }

        public static bool IsFunctionalKeyHolding(bool ctrl = false, bool shift = false, bool alt = false)
        {
            var ctrlOk = ctrl == (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            var shiftOk = shift == (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            var altOk = alt == (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
            return ctrlOk && shiftOk && altOk;
        }

        public static bool IsKeyDown(KeyCode key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKeyDown(key) &&
                   (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }

        public static bool IsKeyUp(KeyCode key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKeyUp(key) &&
                   (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }

        public static bool IsKeyHolding(KeyCode key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKey(key) &&
                   (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }
    }
}