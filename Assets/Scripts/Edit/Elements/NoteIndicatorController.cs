#nullable enable

using Deenote.GameStage;
using Deenote.Project.Models.Datas;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.Edit.Elements
{
    public sealed class NoteIndicatorController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _noteSpriteRenderer = null!;
        [SerializeField] private SpriteRenderer _holdBodySpriteRenderer = null!;

        private (Vector2 start, Vector2 offset)? _linkLine;
        private bool _shouldDrawLinkLine;
        private NoteData _notePrototype = null!;

        public NoteData NotePrototype => _notePrototype;

        public void Initialize(NoteData note)
        {
            _notePrototype = note;

            var prefab = _notePrototype switch {
                { IsSwipe: true } => MainSystem.GameStage.Args.SwipeNoteSpritePrefab,
                { IsSlide: true } => MainSystem.GameStage.Args.SlideNoteSpritePrefab,
                { HasSound: true } => MainSystem.GameStage.Args.BlackNoteSpritePrefab,
                _ when MainSystem.GameStage.IsPianoNotesDistinguished
                    => MainSystem.GameStage.Args.NoSoundNoteSpritePrefab,
                _ => MainSystem.GameStage.Args.BlackNoteSpritePrefab,
            };
            _noteSpriteRenderer.sprite = prefab.Sprite;
            _noteSpriteRenderer.gameObject.transform.localScale =
                new(prefab.Scale * _notePrototype.Size, prefab.Scale, prefab.Scale);
            UpdateLinkLineVisibility(MainSystem.GameStage.IsShowLinkLines);
            if (note.NextLink is not null) {
                var (toX, toZ) =
                    MainSystem.Args.NoteCoordToWorldPosition(note.NextLink.PositionCoord -
                                                             _notePrototype.PositionCoord);
                _linkLine = (Vector2.zero, new Vector2(toX, toZ));
            }
            else {
                _linkLine = null;
            }

            if (note.IsHold) {
                ref readonly var holdpref = ref MainSystem.GameStage.Args.HoldSpritePrefab;
                _holdBodySpriteRenderer.gameObject.SetActive(true);
                _holdBodySpriteRenderer.transform.localScale = _holdBodySpriteRenderer.transform.localScale with {
                    x = note.Size * holdpref.ScaleX,
                    y = MainSystem.Args.TimeToHoldScaleY(note.Duration),
                };
            }
            else {
                _holdBodySpriteRenderer.gameObject.SetActive(false);
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
                PerspectiveLinesRenderer.Instance.AddLine(start, start + offset,
                    color: MainSystem.Args.LinkLineColor,
                    width: MainSystem.Args.GridWidth);
        }
    }
}