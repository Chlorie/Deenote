using Deenote.GameStage;
using Deenote.Project.Models.Datas;
using UnityEngine;

namespace Deenote.Edit.Elements
{
    public sealed class NoteIndicatorController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _noteSpriteRenderer = null!;

        private (Vector2 start, Vector2 offset)? _linkLine;
        private bool _shouldDrawLinkLine;
        private NoteData _notePrototype = null!;

        public void Initialize(NoteData note)
        {
            _notePrototype = note;

            var prefab = _notePrototype switch
            {
                { IsSlide: true } => MainSystem.GameStage.SlideNoteSpritePrefab,
                { HasSound: true } => MainSystem.GameStage.BlackNoteSpritePrefab,
                _ => MainSystem.GameStage.NoSoundNoteSpritePrefab,
            };
            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.gameObject.transform.localScale = new(prefab.Scale * _notePrototype.Size, prefab.Scale, prefab.Scale);
            UpdateLinkLineVisibility(MainSystem.GameStage.IsShowLinkLines);
            if (note.NextLink is not null)
            {
                var (toX, toZ) = MainSystem.Args.NoteCoordToWorldPosition(note.NextLink.PositionCoord - _notePrototype.PositionCoord);
                _linkLine = (Vector2.zero, new Vector2(toX, toZ));
            }
        }

        public void MoveTo(NoteCoord coord)
        {
            var (x, z) = MainSystem.Args.NoteCoordToWorldPosition(coord, MainSystem.GameStage.CurrentMusicTime);
            gameObject.transform.localPosition = new Vector3(x, 0f, z);
            if (_linkLine is not null)
                _linkLine = _linkLine.Value with { start = new Vector2(x, z) };
        }

        public void UpdateLinkLineVisibility(bool showLinkLines) => _shouldDrawLinkLine = showLinkLines;

        private void Update()
        {
            if (_shouldDrawLinkLine && _linkLine is var (start, offset))
                PerspectiveLinesRenderer.Instance.AddLine(start, start + offset, MainSystem.Args.LinkLineColor, 2.0f);
        }
    }
}