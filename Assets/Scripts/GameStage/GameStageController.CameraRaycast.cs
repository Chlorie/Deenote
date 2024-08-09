using UnityEngine;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        public Plane NotePanelPlane => new(Vector3.up, 0f);

        public Vector3 NormalizeGridPosition(Vector3 worldPosition)
        {
            Vector3 res = PerspectiveCamera.WorldToViewportPoint(worldPosition);
            res.z = 1f;
            return PerspectiveCamera.ViewportToWorldPoint(res);
        }
    }
}