using UnityEngine;

namespace Deenote.Components
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public sealed class GameStageScaleCamera : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [Range(0f,180f)]
        [SerializeField] float _horizontalFOV = 120f;

        // TODO:不需要时刻更新相机设置，更改为Awake
        private void Update()
        {
            UpdateFieldOfView();
        }

        // reference: https://discussions.unity.com/t/fixed-width-relative-height-on-different-aspect-ratio-screen/114935/4
        private void UpdateFieldOfView()
        {
            float aspectRatio = _camera.aspect;
            float hFov_rad = _horizontalFOV * Mathf.Deg2Rad;
            float vFov_rad = 2 * Mathf.Atan(Mathf.Tan(hFov_rad / 2) / aspectRatio);
            float vFov = vFov_rad * Mathf.Rad2Deg;

            _camera.fieldOfView = vFov;

        }
    }
}
