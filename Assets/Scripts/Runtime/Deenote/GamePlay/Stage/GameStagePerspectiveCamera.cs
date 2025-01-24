#nullable enable

using Deenote.Entities;
using UnityEngine;

namespace Deenote.GamePlay.Stage
{
    [RequireComponent(typeof(Camera))]
    public sealed class GameStagePerspectiveCamera : MonoBehaviour
    {
        [SerializeField] Camera _camera = default!;
        [SerializeField] Camera _backgroundCamera = default!;

        public Camera Camera => _camera;

        private GamePlayManager _manager = default!;
        private GameStageControllerBase _stage = default!;

        public void OnInstantiate(GamePlayManager manager, GameStageControllerBase stage)
        {
            _manager = manager;
            _stage = stage;
        }

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

        public bool TryConvertViewportPointToNoteCoord(Vector2 perspectiveViewPanelViewportPoint, out NoteCoord coord)
        {
            var viewportPoint = perspectiveViewPanelViewportPoint with {
                y = perspectiveViewPanelViewportPoint.y / _camera.rect.height
            };

            if (!IsInViewArea(viewportPoint)) {
                coord = default;
                return false;
            }

            var ray = _camera.ViewportPointToRay(viewportPoint);
            var plane = new Plane(_stage.NotePanelTransform.up, _stage.NotePanelTransform.position);
            if (plane.Raycast(ray, out var distance)) {
                var hitpoint = ray.GetPoint(distance);
                coord = new NoteCoord(
                    _stage.ConvertWorldXToNoteCoordPosition(hitpoint.x),
                    _stage.ConvertWorldZToNoteCoordTime(hitpoint.z) + _manager.MusicPlayer.Time);
                return true;
            }
            coord = default;
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: > 0f and <= 1f };
        }

        public float SuddenPlusRangeToVisibleRangePercentage(float suddenPlus)
        {
            var x = _stage.ConvertNoteCoordPositionToWorldX(0f);
            var y = _stage.NotePanelTransform.position.y;
            var maxZ = _stage.ConvertNoteCoordTimeToWorldZ(_stage.NoteActiveAheadTime);
            var minZ = _stage.ConvertNoteCoordTimeToWorldZ(0f);

            var maxVp = _camera.WorldToViewportPoint(new Vector3(x, y, maxZ));
            var minVp = _camera.WorldToViewportPoint(new Vector3(x, y, minZ));

            var midVpy = Mathf.Lerp(maxVp.y, minVp.y, suddenPlus);
            var ray = _camera.ViewportPointToRay(new Vector3(maxVp.x, midVpy));
            var notePanelPlane = new Plane(_stage.NotePanelTransform.up, _stage.NotePanelTransform.position);
            if (notePanelPlane.Raycast(ray, out var enter)) {
                var hitpoint = ray.GetPoint(enter);
                return Mathf.InverseLerp(minZ, maxZ, hitpoint.z);
            }
            else {
                Debug.LogWarning("Cannot find visible note plane start posion");
                return 1f;
            }
        }

        private void OnValidate()
        {
            _camera ??= GetComponent<Camera>();
        }
    }
}