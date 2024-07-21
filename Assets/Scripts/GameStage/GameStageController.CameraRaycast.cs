using UnityEngine;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        [Header("Camera")]
        [SerializeField] Camera _perspectiveCamera;

        public Plane NotePanelPlane => new(Vector3.up, 0f);

        public Vector3 NormalizeGridPosition(Vector3 worldPosition)
        {
            Vector3 res = _perspectiveCamera.WorldToViewportPoint(worldPosition);
            res.z = 1f;
            return _perspectiveCamera.ViewportToWorldPoint(res);
        }
    }
}