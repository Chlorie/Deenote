using UnityEngine;

namespace Deenote.ApplicationManaging
{
    public sealed class ResolutionAdjuster
    {
        private int _width;
        private int _height;

        public void SetAspectRatio(float ratio, bool rememberOriginalResolution)
        {
            var height = Screen.height;
            if (rememberOriginalResolution) {
                _height = height;
                _width = Screen.width;
            }
            int width = (int)(height * ratio);
            Screen.SetResolution(width, height, false);
        }

        public void RecoverResolution()
        {
            Screen.SetResolution(_width, _height, false);
        }
    }
}