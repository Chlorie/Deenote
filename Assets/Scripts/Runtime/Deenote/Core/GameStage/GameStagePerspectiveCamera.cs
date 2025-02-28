#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage
{
    [RequireComponent(typeof(Camera))]
    public sealed class GameStagePerspectiveCamera : MonoBehaviour
    {
        [SerializeField] Camera _camera = default!;
        [SerializeField] Camera _backgroundCamera = default!;

        public Camera Camera => _camera;

        public void ApplyToRenderTexture(RenderTexture renderTexture)
        {
            if (_camera.targetTexture != renderTexture) {
                _camera.targetTexture = renderTexture;
            }
            var width = renderTexture.width;
            var height = renderTexture.height;
            var h = 9f / 16f * width / height;
            _camera.rect = new Rect(0f, 0f, 1f, h);

            _backgroundCamera.targetTexture = renderTexture;
        }

        private void OnValidate()
        {
            _camera ??= GetComponent<Camera>();
        }
    }
}