using Deenote.Project.Models.Datas;
using UnityEngine;

namespace Deenote.Edit.Elements
{
    public sealed class NoteIndicatorController : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer;

        private NoteData NotePrototype;

        public void Initialize(NoteData note)
        {
            NotePrototype = note;

            var prefab = NotePrototype switch {
                { IsSlide: true } => MainSystem.GameStage.SlideNoteSpritePrefab,
                { HasSound: true } => MainSystem.GameStage.BlackNoteSpritePrefab,
                _ => MainSystem.GameStage.NoSoundNoteSpritePrefab,
            };
            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.gameObject.transform.localScale = new(prefab.Scale * NotePrototype.Size, prefab.Scale, prefab.Scale);
        }

        public void MoveTo(NoteCoord coord)
        {
            var (x, z) = MainSystem.Args.NoteCoordToWorldPosition(coord, MainSystem.GameStage.CurrentMusicTime);
            gameObject.transform.localPosition = new(x, 0f, z);
        }
    }
}