#nullable enable

using Deenote.Entities;
using Deenote.Library;
using Deenote.Library.Components;
using UnityEngine;

namespace Deenote.GamePlay.Stage
{
    public abstract class GameStageControllerBase : MonoBehaviour
    {
        [SerializeField] Material _holdBodyCullMaterial = default!;

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
        public float NoteActiveAheadTime => Args.NotePanelBaseLengthTime / _manager.ActualNoteSpeed;

        /// <summary>
        /// The time offset from current time when notes become visible
        /// </summary>
        public float NoteAppearAheadTime => NoteActiveAheadTime * _manager.VisibleRangePercentage;

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
            => z / Args.NoteTimeToZBaseMultiplier / _manager.ActualNoteSpeed;

        public float ConvertNoteCoordPositionToWorldX(float position)
            => position * (Args.NotePanelWidth / EntityArgs.StageMaxPositionWidth);

        public float ConvertNoteCoordTimeToWorldZ(float time, float noteSpeed = 1f)
            => _manager.ActualNoteSpeed * noteSpeed * ConvertNoteCoordTimeToWorldZBase(time);

        private float ConvertNoteCoordTimeToWorldZBase(float time)
            => time * Args.NoteTimeToZBaseMultiplier;

        public float ConvertNoteCoordTimeToHoldScaleY(float time, float noteSpeed = 1f)
            => _manager.ActualNoteSpeed * noteSpeed * ConvertNoteCoordTimeToWorldZBase(time) / Args.HoldSpritePrefab.Sprite.bounds.size.y;

        public (float X, float Z) ConvertNoteCoordToWorldPosition(NoteCoord coord)
            => (ConvertNoteCoordPositionToWorldX(coord.Position), ConvertNoteCoordTimeToWorldZ(coord.Time));

        #endregion
    }
}