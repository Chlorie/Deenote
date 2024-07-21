using Cysharp.Threading.Tasks;
using Deenote.Utilities.Robustness;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Deenote.Utilities
{
    public static class UnityUtils
    {
        public static Color WithAlpha(this in Color color, float alpha)
            => new(color.r, color.g, color.b, alpha);

        public static Vector3 WithX(this in Vector3 vector, float x) => new(x, vector.y, vector.z);

        public static Vector2 WithX(this in Vector2 vector, float x) => new(x, vector.y);

        public static Vector2 WithY(this in Vector2 vector, float y) => new(vector.x, y);

        public static void AddListener(this UnityEvent ev, Func<UniTaskVoid> uniTaskFunc)
            => ev.AddListener(UniTask.UnityAction(uniTaskFunc));

        public static ObjectPool<T> CreateObjectPool<T>(T prefab, Transform parentTransform = null, int defaultCapacity = 10, int maxSize = 10000) where T : Component
            => CreateObjectPool(() => UnityEngine.Object.Instantiate(prefab, parentTransform), defaultCapacity, maxSize);

        public static ObjectPool<T> CreateObjectPool<T>(Func<T> createFunc, int defaultCapacity = 10, int maxSize = 10000) where T : Component
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

        public static T CheckNull<T>(this T self) where T : UnityEngine.Object
        {
            if (self == null)
                return null;
            else
                return self;
        }

        public static void SetSolidColor(this LineRenderer lineRenderer, Color color)
        {
            var gradient = lineRenderer.colorGradient;
            var keys = gradient.colorKeys;
            var akeys = gradient.alphaKeys;
            foreach (ref var key in keys.AsSpan()) {
                key.color = color;
            }
            foreach (ref var key in akeys.AsSpan()) {
                key.alpha = color.a;
            }
            gradient.colorKeys = keys;
            gradient.alphaKeys = akeys;
            lineRenderer.colorGradient = gradient;
        }

        public static void SetGradientColor(this LineRenderer lineRenderer, float startAlphaUnclamped, float endAlphaUnclamped)
        {
            if (startAlphaUnclamped == endAlphaUnclamped) {
                var g = lineRenderer.colorGradient;
                g.alphaKeys = new GradientAlphaKey[1] { new(Mathf.Clamp01(startAlphaUnclamped), 0f) };
                lineRenderer.colorGradient = g;
                return;
            }

            GradientAlphaKey[] keys;
            switch (startAlphaUnclamped, endAlphaUnclamped) {
                case ( > 1f, >= 1f):
                    keys = new GradientAlphaKey[1] { new(1f, 0f) };
                    break;
                case ( > 1f, >= 0f): {
                    keys = new GradientAlphaKey[3] {
                        new (alpha: 1f, 0f),
                        new (alpha: 1f, Mathf.InverseLerp(startAlphaUnclamped, endAlphaUnclamped, 1f)),
                        new (alpha: endAlphaUnclamped, 1f),
                    };
                    break;
                }
                case ( > 1f, _): {
                    keys = new GradientAlphaKey[4] {
                        new(alpha: 1f, 0f),
                        new(alpha: 1f, Mathf.InverseLerp(startAlphaUnclamped, endAlphaUnclamped, 1f)),
                        new(alpha: 0f, Mathf.InverseLerp(startAlphaUnclamped, endAlphaUnclamped, 0f)),
                        new(alpha: 0f, 1f),
                    };
                    break;
                }
                case ( > 0f, >= 0f): {
                    keys = new GradientAlphaKey[2] {
                        new(alpha: startAlphaUnclamped, 0f),
                        new(alpha: endAlphaUnclamped, 1f),
                    };
                    break;
                }
                case ( > 0f, _): {
                    keys = new GradientAlphaKey[3] {
                        new(alpha: startAlphaUnclamped, 0f),
                        new(alpha: 0f, Mathf.InverseLerp(startAlphaUnclamped, endAlphaUnclamped, 0f)),
                        new(alpha: 0f, 1f),
                    };
                    break;
                }
                default:
                    keys = new GradientAlphaKey[1] { new(0f, 0f) };
                    break;
            }
            var gradient = lineRenderer.colorGradient;
            gradient.alphaKeys = keys;
            lineRenderer.colorGradient = gradient;
        }

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
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKeyDown(key) && (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }

        public static bool IsKeyUp(KeyCode key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKeyUp(key) && (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }

        public static bool IsKeyHolding(KeyCode key, bool ctrl = false, bool shift = false, bool alt = false)
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            return IsFunctionalKeyHolding(ctrl, shift, alt) && Input.GetKey(key) && (obj == null || obj.GetComponent<TMP_InputField>() == null);
        }
    }
}