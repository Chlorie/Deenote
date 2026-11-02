#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage
{
    [RequireComponent(typeof(BoxCollider))]
    internal sealed class SpeedChangeWarningRaycastingCollider : MonoBehaviour
    {
        [SerializeField] private BoxCollider _collider = default!;
        [SerializeField] private GameStageSpeedChangeWarningNoteController _warningController = default!;

        public GameStageSpeedChangeWarningNoteController WarningController => _warningController;

        private void Awake()
        {
            _collider ??= GetComponent<BoxCollider>();
            _warningController ??= GetComponentInParent<GameStageSpeedChangeWarningNoteController>();
        }

        private void OnValidate()
        {
            _collider ??= GetComponent<BoxCollider>();
            _warningController ??= GetComponentInParent<GameStageSpeedChangeWarningNoteController>();
        }
    }
}
