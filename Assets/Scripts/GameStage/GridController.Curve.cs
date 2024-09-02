using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Utilities.Robustness;
using System;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GridController
    {
        private const int CubicCurveSegmentCount = 400;

        private CurveLineData? _curveLineData;
        private Vector2[] _curveCoords = new Vector2[CubicCurveSegmentCount];
        private bool _shouldRenderCurve;
        private bool _isCurveOn;

        public bool IsCurveOn
        {
            get => _isCurveOn;
            private set {
                if (_isCurveOn == value)
                    return;
                _isCurveOn = value;
                _editorPropertiesWindow.NotifyCurveOn(_isCurveOn);
            }
        }

        public (float Start, float End)? CurveTime => IsCurveOn ? (_curveLineData!.MinX, _curveLineData.MaxX) : null;

        public void InitializeCurve(ListReadOnlyView<NoteModel> interpolationNotes, CurveKind kind)
        {
            NoteTimeComparer.AssertInOrder(interpolationNotes);
            if (interpolationNotes.Count < 2) {
                IsCurveOn = false;
                return;
            }

            _curveLineData = kind switch {
                CurveKind.Cubic => CurveLineData.Cubic(interpolationNotes),
                CurveKind.Linear => CurveLineData.Linear(interpolationNotes),
                _ => null,
            };
            IsCurveOn = true;
            UpdateCurveLine();
        }

        public void HideCurve() => IsCurveOn = false;

        private void DrawCurve()
        {
            if (_isCurveOn && _shouldRenderCurve)
                PerspectiveLinesRenderer.Instance.AddLineStrip(_curveCoords, MainSystem.Args.CurveLineColor, 2f);
        }

        // Copied from Chlorie's
        public void UpdateCurveLine()
        {
            if (!_isCurveOn)
                return;

            var stage = GameStageController.Instance;
            float stageCurrentTime = stage.CurrentMusicTime;
            float stageMaxTime = stageCurrentTime + stage.StageNoteAheadTime;

            using var coords = _curveLineData!.GetRenderValues(stage.CurrentMusicTime, stageMaxTime);
            var coordSpan = coords.Span;
            if (coordSpan.Length == 0) {
                _shouldRenderCurve = false;
                return;
            }
            _shouldRenderCurve = true;

            Debug.Assert(coordSpan.Length == _curveCoords.Length && coordSpan.Length == CubicCurveSegmentCount);
            for (int i = 0; i < CubicCurveSegmentCount; i++) {
                var (x, z) = MainSystem.Args.NoteCoordToWorldPosition(coordSpan[i]);
                _curveCoords[i] = new Vector2(x, z);
            }
        }

        public enum CurveKind
        {
            Cubic,
            Linear,
        }

        // Copied from Chlorie's
        private sealed class CurveLineData
        {
            private readonly double[] x;
            private readonly double[] a;
            private readonly double[] b;
            private readonly double[] c;
            private readonly double[] d;

            public float MinX => (float)x[0];
            public float MaxX => (float)x[^1];

            private CurveLineData(int pointCount)
            {
                x = new double[pointCount];
                a = new double[pointCount];
                c = new double[pointCount];
                b = new double[pointCount - 1];
                d = new double[pointCount - 1];
            }

            public static CurveLineData Linear(ListReadOnlyView<NoteModel> interpolationNotes)
            {
                var curve = new CurveLineData(interpolationNotes.Count);

                for (int i = 0; i < interpolationNotes.Count; i++) {
                    var note = interpolationNotes[i].Data;
                    curve.x[i] = note.Time;
                    curve.a[i] = note.Position;
                }
                for (int i = 0; i < interpolationNotes.Count - 1; i++) {
                    curve.b[i] = (curve.a[i + 1] - curve.a[i]) / (curve.x[i + 1] - curve.x[i]);
                }
                Array.Clear(curve.c, 0, curve.c.Length);
                Array.Clear(curve.d, 0, curve.d.Length);

                return curve;
            }

            public static CurveLineData Cubic(ListReadOnlyView<NoteModel> interpolationNotes)
            {
                var curve = new CurveLineData(interpolationNotes.Count);

                int count = interpolationNotes.Count;
                int n = count - 1;

                for (int i = 0; i < count; i++) {
                    curve.x[i] = interpolationNotes[i].Data.Time;
                    curve.a[i] = interpolationNotes[i].Data.Position;
                }

                var alpha = (stackalloc double[count - 1]);
                var h = (stackalloc double[count - 1]);
                var mu = (stackalloc double[count]);
                var l = (stackalloc double[count]);
                var z = (stackalloc double[count]);

                for (int i = 0; i < count - 1; i++) {
                    h[i] = i + 1 - curve.x[i + 1] - curve.x[i];
                }
                for (int i = 1; i < count - 1; i++) {
                    alpha[i] = 3 * (curve.a[i + 1] - curve.a[i]) / h[i] - 3 * (curve.a[i] - curve.a[i - 1]) / h[i - 1];
                }

                l[0] = 1d;
                mu[0] = 0d;
                z[0] = 0d;
                for (int i = 1; i < n; i++) {
                    l[i] = 2 * (curve.x[i + 1] - curve.x[i - 1]) - h[i - 1] * mu[i - 1];
                    mu[i] = h[i] / l[i];
                    z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
                }
                l[n] = 1d;
                z[n] = 0d;
                curve.c[n] = 0d;

                for (int i = n - 1; i >= 0; i--) {
                    curve.c[i] = z[i] - mu[i] * curve.c[i + 1];
                    curve.b[i] = (curve.a[i + 1] - curve.a[i]) / h[i] - h[i] * (curve.c[i + 1] + 2 * curve.c[i]) / 3;
                    curve.d[i] = (curve.c[i + 1] - curve.c[i]) / (3 * h[i]);
                }

                return curve;
            }

            public float? GetPosition(float time)
            {
                if (time < x[0] || time > x[^1])
                    return null;

                for (int i = 0; i < x.Length - 1; i++) {
                    if (!(time < x[i + 1])) continue;
                    double diff = time - x[i];
                    return (float)(
                        a[i] +
                        b[i] * diff +
                        c[i] * diff * diff +
                        d[i] * diff * diff * diff
                    );
                }

                Debug.Assert(time == x[^1]);
                return 0;
            }

            public PooledSpan<NoteCoord>.ReadOnlyView GetRenderValues(float renderMinTime, float renderMaxTime)
            {
                if (MinX >= renderMaxTime || MaxX <= renderMinTime || renderMinTime >= renderMaxTime)
                    return default;

                float min = Mathf.Max(MinX, renderMinTime);
                float max = Mathf.Min(MaxX, renderMaxTime);
                var coords = new PooledSpan<NoteCoord>(CubicCurveSegmentCount);
                var span = coords.Span;
                for (int i = 0; i < CubicCurveSegmentCount; i++) {
                    float time = Mathf.Lerp(min, max, (float)i / CubicCurveSegmentCount);
                    float pos = GetPosition(time).Value;
                    span[i] = NoteCoord.ClampPosition(time, pos);
                }
                return coords.ToReadOnly();
            }
        }
    }
}