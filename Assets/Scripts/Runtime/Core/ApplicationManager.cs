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

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        private static void _RegisterQuitting()
        {
            Application.wantsToQuit += () =>
            {
                var args = new CancelEventArgs();
                Quitting?.Invoke(args);
                return !args.Cancel;
            };
        }
#endif

        public static void Quit()
        {
            // TODO: SaveConfig
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Resolution

        private static Vector2Int? _resolution;

        public static void SetAspectRatio(float ratio, bool rememberCurrentResolution)
        {
            var height = Screen.height;
            if (rememberCurrentResolution) {
                _resolution = new Vector2Int(Screen.width, height);
            }
            int width = (int)(height * ratio);
            Screen.SetResolution(width, height, false);
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