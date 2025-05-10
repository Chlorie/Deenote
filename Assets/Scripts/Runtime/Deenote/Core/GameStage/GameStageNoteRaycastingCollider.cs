#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage
{
    [RequireComponent(typeof(BoxCollider))]
    internal sealed class GameStageNoteRaycastingCollider : MonoBehaviour
    {
        [SerializeField] BoxCollider _collider;
        [SerializeField] GameStageNoteController _noteController;

        public GameStageNoteController NoteController => _noteController;

        private void OnValidate()
        {
            _collider ??= GetComponent<BoxCollider>();
        }
    }
}