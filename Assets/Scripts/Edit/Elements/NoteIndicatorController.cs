using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using UnityEngine;

namespace Deenote.Edit.Elements
{
    public sealed class NoteIndicatorController : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _noteSpriteRenderer;
        [SerializeField] LineRenderer _linkLineRenderer;

        private NoteData NotePrototype;

        private void Awake()
        {
            _linkLineRenderer.SetSolidColor(MainSystem.Args.LinkLineColor);
        }

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
            UpdateLinkLineVisibility(MainSystem.GameStage.IsShowLinkLines);
            if (NotePrototype.NextLink is not null) {
                var (toX, toZ) = MainSystem.Args.NoteCoordToWorldPosition(note.NextLink.PositionCoord - NotePrototype.PositionCoord);
                _linkLineRenderer.SetPosition(0, Vector3.zero);
                _linkLineRenderer.SetPosition(1, new Vector3(toX, 0, toZ));
            }
        }

        public void MoveTo(NoteCoord coord)
        {
            var (x, z) = MainSystem.Args.NoteCoordToWorldPosition(coord, MainSystem.GameStage.CurrentMusicTime);
            gameObject.transform.localPosition = new(x, 0f, z);
        }

        public void UpdateLinkLineVisibility(bool showLinkLines)
        {
            if (showLinkLines) {
                _linkLineRenderer.gameObject.SetActive(NotePrototype.NextLink is not null);
            }
            else {
                _linkLineRenderer.gameObject.SetActive(false);
            }
        }
    }
}