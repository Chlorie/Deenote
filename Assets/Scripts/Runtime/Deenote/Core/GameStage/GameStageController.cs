#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Core.GameStage.Args;
using Deenote.Entities;
using Deenote.Library;
using UnityEngine;

namespace Deenote.Core.GameStage
{
    public abstract class GameStageController : MonoBehaviour
    {
        [SerializeField] GameStagePerspectiveCamera _perspectiveCamera = default!;

        [SerializeField] Material _holdBodyCullMaterial = default!;

        public GameStagePerspectiveCamera PerspectiveCamera => _perspectiveCamera;

        /// <summary>
        /// The parent transform of instantiated notes
        /// </summary>
        [field: SerializeField]
        public Transform NotePanelTransform { get; private set; } = default!;
        [field: SerializeField]
        public Transform NoteIndicatorPanelTransform { get; private set; } = default!;
        [field: SerializeField]
        public RectTransform NoteDragSelectionPanelTransform { get; private set; } = default!;
        [field: SerializeField]
        public GameStageArgs Args { get; private set; } = default!;
        [field: SerializeField]
        public GridLineArgs GridLineArgs { get; private set; } = default!;

        /// <summary>
        /// The time offset from current time when notes activate
        /// </summary>
        /// <remarks>
        /// We start tracking notes when note appears as if sudden + is 0, and sets its
        /// visibility according to <see cref="NoteAppearAheadTime"/>, this is for tracking
        /// notes in different speed easier.
        /// </remarks>
        public float NoteActiveAheadTime => Args.NotePanelBaseLengthTime / _manager.ActualNoteFallSpeed;

        public float GetNoteActiveAheadTime(float noteSpeed) => NoteActiveAheadTime / noteSpeed;

        public float GetNoteActiveTime(IStageNoteNode node) => node.Time - GetNoteActiveAheadTime(node.Speed);

        /// <summary>
        /// The time offset from current time when notes(speed==1) become visible
        /// </summary>
        public float NoteAppearAheadTime => NoteActiveAheadTime * _manager.VisibleRangePercentage;

        public float GetNoteAppearAheadTime(float noteSpeed) => NoteAppearAheadTime / noteSpeed;

        public float GetNoteAppearTime(IStageNoteNode node) => node.Time - GetNoteAppearAheadTime(node.Speed);

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
        }

        protected virtual void OnIsStageEffectOnChanged(bool value) { }

        protected virtual void OnVisibleRangePercentageChanged(float value)
        {
            var time = NoteActiveAheadTime * value;
            var z = ConvertNoteCoordTimeToWorldZ(time);
            _holdBodyCullMaterial.SetFloat(HoldCullMaxZPropertyId, z);
        }

        #region Converters

        public float ConvertWorldXToNoteCoordPosition(float x)
            => x / (Args.NotePanelWidth / EntityArgs.StageMaxPositionWidth);

        public float ConvertWorldZToNoteCoordTime(float z)
            => z / Args.NoteTimeToZBaseMultiplier / _manager.ActualNoteFallSpeed;

        public float ConvertNoteCoordPositionToWorldX(float position)
            => position * (Args.NotePanelWidth / EntityArgs.StageMaxPositionWidth);

        public float ConvertNoteCoordTimeToWorldZ(float time, float noteSpeed = 1f)
            => _manager.ActualNoteFallSpeed * noteSpeed * ConvertNoteCoordTimeToWorldZBase(time);

        private float ConvertNoteCoordTimeToWorldZBase(float time)
            => time * Args.NoteTimeToZBaseMultiplier;

        public float ConvertNoteCoordTimeToHoldScaleY(float time, float noteSpeed = 1f)
            => _manager.ActualNoteFallSpeed * noteSpeed * ConvertNoteCoordTimeToWorldZBase(time) / Args.HoldSpritePrefab.Sprite.bounds.size.y;

        public (float X, float Z) ConvertNoteCoordToWorldPosition(NoteCoord coord, float noteSpeed = 1f)
            => (ConvertNoteCoordPositionToWorldX(coord.Position), ConvertNoteCoordTimeToWorldZ(coord.Time, noteSpeed));

        public bool TryConvertViewportPointToNoteCoord(Vector2 perspectiveViewPanelViewportPoint, out NoteCoord coord)
        {
            var camera = PerspectiveCamera.Camera;
            var viewportPoint = perspectiveViewPanelViewportPoint with {
                y = perspectiveViewPanelViewportPoint.y / camera.rect.height
            };

            if (!IsInViewArea(viewportPoint)) {
                coord = default;
                return false;
            }

            var ray = camera.ViewportPointToRay(viewportPoint);
            var plane = new Plane(NotePanelTransform.up, NotePanelTransform.position);
            if (plane.Raycast(ray, out var distance)) {
                var hitpoint = ray.GetPoint(distance);
                coord = new NoteCoord(
                    ConvertWorldXToNoteCoordPosition(hitpoint.x),
                    ConvertWorldZToNoteCoordTime(hitpoint.z) + _manager.MusicPlayer.Time);
                return true;
            }
            coord = default;
            return false;

            static bool IsInViewArea(Vector2 vp) => vp is { x: >= 0f and <= 1f, y: > 0f and <= 1f };
        }

        public float ConvertSuddenPlusRangToVisibleRangePercentage(float suddenPlus)
        {
            var x = ConvertNoteCoordPositionToWorldX(0f);
            var y = NotePanelTransform.position.y;
            var maxZ = ConvertNoteCoordTimeToWorldZ(NoteActiveAheadTime);
            var minZ = ConvertNoteCoordTimeToWorldZ(0f);

            var camera = PerspectiveCamera.Camera;

            var maxVp = camera.WorldToViewportPoint(new Vector3(x, y, maxZ));
            var minVp = camera.WorldToViewportPoint(new Vector3(x, y, minZ));

            var midVpy = Mathf.Lerp(maxVp.y, minVp.y, suddenPlus);
            var ray = camera.ViewportPointToRay(new Vector3(maxVp.x, midVpy));
            var notePanelPlane = new Plane(NotePanelTransform.up, NotePanelTransform.position);
            if (notePanelPlane.Raycast(ray, out var enter)) {
                var hitpoint = ray.GetPoint(enter);
                return Mathf.InverseLerp(minZ, maxZ, hitpoint.z);
            }
            else {
                Debug.LogWarning("Cannot find visible note plane start posion");
                return 1f;
            }
        }

        #endregion
    }
}