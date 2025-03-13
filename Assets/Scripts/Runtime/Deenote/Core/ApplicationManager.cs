#nullable enable

using System;
using System.ComponentModel;
using UnityEngine;

namespace Deenote.Core
{
    public static class ApplicationManager
    {
        #region Quitting

        public static event Action<CancelEventArgs>? Quitting;

        [RuntimeInitializeOnLoadMethod]
        private static void _RegisterQuitting()
        {
            Application.wantsToQuit += () =>
            {
                var args = new CancelEventArgs();
                Quitting?.Invoke(args);
                if (!args.Cancel) {
                    MainSystem.SaveSystem.SaveConfigurations();
                    return true;
                }
                else {
                    return false;
                }
            };
        }

        public static void Quit()
        {
            MainSystem.SaveSystem.SaveConfigurations();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Resolution

        private static Vector2Int? _resolution;
        public static event Action<Vector2Int>? ResolutionChanged;

        public static Vector2Int GetResolution()
            => new(Screen.width, Screen.height);

        public static void SetResolution(Vector2Int resolution)
        {
            Screen.SetResolution(resolution.x, resolution.y, false);
            ResolutionChanged?.Invoke(resolution);
        }

        public static void SetAspectRatio(float ratio, bool rememberCurrentResolution)
        {
            var height = Screen.height;
            if (rememberCurrentResolution) {
                _resolution = new Vector2Int(Screen.width, height);
            }
            int width = (int)(height * ratio);
            Screen.SetResolution(width, height, false);
            ResolutionChanged?.Invoke(new(width, height));
        }

        public static void RecoverResolution()
        {
            if (_resolution is { } resolution) {
                Screen.SetResolution(resolution.x, resolution.y, false);
            }
        }

        #endregion
    }
}