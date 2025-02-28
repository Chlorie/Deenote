#nullable enable

using UnityEngine;

namespace Deenote.Core.GameStage.Args
{
    [CreateAssetMenu(
        fileName = nameof(GridLineArgs),
        menuName = $"{nameof(Deenote)}/{nameof(GamePlay)}/{nameof(GridLineArgs)}")]
    public sealed class GridLineArgs : ScriptableObject
    {
        public Color LinkLineColor = new(1f, 233f / 255f, 135f / 255f);
        public Color SubBeatLineColor = new(42f / 255f, 42 / 255f, 42 / 255f, 0.75f);
        public Color BeatLineColor = new(0.5f, 0f, 0f, 1f);
        public Color TempoLineColor = new(0f, 0.5f, 0.5f, 1f);
        public Color PositionGridLineColor = new(42f / 255f, 42 / 255f, 42 / 255f, 0.75f);
        public Color CurveLineColor = new(85f / 255, 192f / 255, 1f);

        public float LinkLineWidth = 2f;
        public float TimeGridWidth = 2f;
        public float PositionGridLineWidth = 2f;
        public float PositionGridBorderWidth = 4f;
        public float CurveLineWidth = 2f;
    }
}