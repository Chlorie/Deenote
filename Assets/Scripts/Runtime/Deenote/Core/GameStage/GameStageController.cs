#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Core.GameStage.Args;
using Deenote.Entities;
using Deenote.Library;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    public abstract class GameStageController : MonoBehaviour
    {
        [SerializeField] GameStagePerspectiveCamera _perspectiveCamera = default!;
        [SerializeField] PerspectiveLinesRenderer _perspectiveLineRenderer = default!;
        [SerializeField] Transform _notePanelTransform = default!;
        [SerializeField] Transform _noteIndicatorPanelTransform = default!;
        [SerializeField] RectTransform _noteDragSelectionPanelTransform = default!;
        [SerializeField] Material _holdBodyCullMaterial = default!;

        public GameStagePerspectiveCamera PerspectiveCamera => _perspectiveCamera;

        public PerspectiveLinesRenderer PerspectiveLinesRenderer => _perspectiveLineRenderer;

        /// <summary>
        /// The parent transform of instantiated notes
        /// </summary>
        public Transform NotePanelTransform => _notePanelTransform;

        public Transform NoteIndicatorPanelTransform => _noteIndicatorPanelTransform;

        [field: SerializeField]
        public GameStageArgs Args { get; private set; } = default!;
        [field: SerializeField]
        public GridLineArgs GridLineArgs { get; private set; } = default!;

        protected GamePlayManager _manager = default!;

        private bool _isStageEffectOn_bf;
        private float _visibleRangePercentage_bf;

        public bool IsStageEffectOn
        {
            get => _isStageEffectOn_bf;
            set {
                if (Utils.SetField(ref _isStageEffectOn_bf, value)) {
                    OnIsStageEffectOnChanged(value);
                }
            }
        }
        public float VisibleRangePercentage
        {
            get => _visibleRangePercentage_bf;
            set {
                if (Utils.SetField(ref _visibleRangePercentage_bf, value)) {
                    OnVisibleRangePercentageChanged(value);
                }
            }
        }

        private static readonly int HoldCullMaxZPropertyId = Shader.PropertyToID("_CullMaxZ");

        protected internal virtual void OnInstantiate(GamePlayManager manager)
        {
            _manager = manager;
            _manager.RegisterNotification(
                GamePlayManager.NotificationFlag.SuddenPlus,
                manager => VisibleRangePercentage = manager.VisibleRangePercentage);
            _perspectiveLineRenderer.OnInstantiate(_manager);
        }

        internal void SetSelectionPanelRect(NoteCoord startCoord, NoteCoord endCoord)
        {
            var (xMin, zMin) = _manager.ConvertNoteCoordToWorldPosition(startCoord - new NoteCoord(0, _manager.MusicPlayer.Time));
            var (xMax, zMax) = _manager.ConvertNoteCoordToWorldPosition(endCoord - new NoteCoord(0, _manager.MusicPlayer.Time));

            _noteDragSelectionPanelTransform.gameObject.SetActive(true);
            _noteDragSelectionPanelTransform.offsetMin = new(xMin, zMin);
            _noteDragSelectionPanelTransform.offsetMax = new(xMax, zMax);
        }

        internal void SetSelectionPanelRectInvisible()
        {
            _noteDragSelectionPanelTransform.gameObject.SetActive(false);
            return;
        }

        protected virtual void OnIsStageEffectOnChanged(bool value) { }

        protected virtual void OnVisibleRangePercentageChanged(float value)
        {
            var time = _manager.StageNoteActiveAheadTime * value;
            var z = _manager.ConvertNoteCoordTimeToWorldZ(time);
            _holdBodyCullMaterial.SetFloat(HoldCullMaxZPropertyId, z);
        }

        #region Perspective Converters

        internal Vector2 ConvertPerspectiveViewportPointToRaycastingViewportPoint(Vector2 perspectiveViewPanelViewportPoint)
        {
            var camera = _perspectiveCamera.Camera;
            return perspectiveViewPanelViewportPoint with {
                y = perspectiveViewPanelViewportPoint.y / camera.rect.height
            };
        }

        internal bool TryRaycastRaycastingViewportPointToNote(Vector2 raycastingViewportPoint, [NotNullWhen(true)] out GameStageNoteController? note)
        {
            var camera = _perspectiveCamera.Camera;
            var ray = camera.ViewportPointToRay(raycastingViewportPoint);
            if (Physics.Raycast(ray, out var hitInfo)) {
                var c = hitInfo.collider;
                if (c != null && c.TryGetComponent<GameStageNoteRaycastingCollider>(out var collider)) {
                    note = collider.NoteController;
                    return true;
                }
            }

            note = null;
            return false;
        }

        internal bool TryConvertNotePanelPositionToRaycastingViewportPoint((float X, float Z) notePanelPosition, out Vector2 viewportPoint)
        {
            var y = _notePanelTransform.position.y;
            var camera = _perspectiveCamera.Camera;
            var position = new Vector3(notePanelPosition.X, y, notePanelPosition.Z);
            var vp = camera.WorldToViewportPoint(position);

            if (vp.z >= camera.nearClipPlane && vp.z <= camera.farClipPlane) {
                viewportPoint = vp;
                return true;
            }
            viewportPoint = default;
            return false;
        }

        internal bool TryConvertRaycastingViewportPointToNotePanelPosition(Vector2 raycastingViewportPoint, out (float X, float Z) notePanelPosition)
        {
            var camera = _perspectiveCamera.Camera;
            var ray = camera.ViewportPointToRay(raycastingViewportPoint);
            var plane = new Plane(_notePanelTransform.up, _notePanelTransform.position);
            if (plane.Raycast(ray, out var distance)) {
                var hitPoint = ray.GetPoint(distance);
                notePanelPosition = (hitPoint.x, hitPoint.z);
                return true;
            }
            notePanelPosition = default;
            return false;
        }

        #endregion
    }
}