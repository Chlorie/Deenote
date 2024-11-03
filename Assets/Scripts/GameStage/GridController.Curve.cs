#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Project.Comparers;
using Deenote.Project.Models;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GridController
    {
        private const int CubicCurveSegmentCount = 400;

        // Update _curveGenerationContext and _curveLineData everytime when initialize
        // new curve, and set _size/speedCurveData to null
        // Lazy initialize _size/speedCurveData when required, and that why we need to
        // cache _curveGenerationContext
        private (List<NoteData> InterpolationNotes, CurveKind Kind) _curveGenerationContext = (new(), default);
        private CurveLineData? _curveLineData;
        private CurveLineData? _sizeCurveData;
        private CurveLineData? _speedCurveData;
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
                _propertyChangeNotifier.Invoke(this, NotifyProperty.IsCurveOn);
            }
        }

        public (float Start, float End)? CurveTime => IsCurveOn ? (_curveLineData!.MinX, _curveLineData.MaxX) : null;

        public void InitializeCurve(ReadOnlySpan<NoteModel> interpolationNotes, CurveKind kind)
        {
            NoteTimeComparer.AssertInOrder(interpolationNotes);
            if (interpolationNotes.Length < 2) {
                _curveGenerationContext.InterpolationNotes.Clear();
                _curveLineData = null;
                _sizeCurveData = null;
                _speedCurveData = null;
                IsCurveOn = false;
                return;
            }

            _curveGenerationContext.InterpolationNotes.Clear();
            foreach (var note in interpolationNotes)
                _curveGenerationContext.InterpolationNotes.Add(note.Data);

            _curveLineData = kind switch {
                CurveKind.Cubic => CurveLineData.Cubic(_curveGenerationContext.InterpolationNotes.AsSpan(), n => n.Position),
                CurveKind.Linear => CurveLineData.Linear(_curveGenerationContext.InterpolationNotes.AsSpan(), n => n.Position),
                _ => null,
            };
            _sizeCurveData = null;
            _speedCurveData = null;
            IsCurveOn = true;
            UpdateCurveLine();
        }

        /// <summary>
        /// Apply size or speed transform with current curve
        /// </summary>
        /// <returns>null if curve is disabled, or given <paramref name="note"/> is not in curve range</returns>
        public float? GetCurveTransformedValue(float propertyValue, CurveApplyProperty property)
        {
            if (!IsCurveOn)
                return null;

            ref var curveData = ref Unsafe.NullRef<CurveLineData>();
            switch (property) {
                case CurveApplyProperty.Size: curveData = ref _sizeCurveData; break;
                case CurveApplyProperty.Speed: curveData = ref _speedCurveData; break;
                default: Debug.Assert(false, "Unknown CurveApplyProperty."); return null!;
            }

            if (curveData is null) {
                Func<NoteData, float> selector = property switch {
                    CurveApplyProperty.Size => n => n.Size,
                    CurveApplyProperty.Speed => n => n.Speed,
                    _ => null!,
                };
                curveData = _curveGenerationContext.Kind switch {
                    CurveKind.Cubic => CurveLineData.Cubic(_curveGenerationContext.InterpolationNotes.AsSpan(), selector),
                    CurveKind.Linear => CurveLineData.Linear(_curveGenerationContext.InterpolationNotes.AsSpan(), selector),
                    _ => null!
                };
            }

            return curveData.GetValue(propertyValue);
        }

        public void HideCurve() => IsCurveOn = false;

        private void DrawCurve()
        {
            if (_isCurveOn && _shouldRenderCurve)
                PerspectiveLinesRenderer.Instance.AddLineStrip(_curveCoords, MainSystem.Args.CurveLineColor, MainSystem.Args.GridWidth);
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

        public enum CurveApplyProperty
        {
            Size,
            Speed,
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

            public static CurveLineData Linear(ReadOnlySpan<NoteData> interpolationNotes, Func<NoteData, float> valueSelector)
            {
                var curve = new CurveLineData(interpolationNotes.Length);

                for (int i = 0; i < interpolationNotes.Length; i++) {
                    var note = interpolationNotes[i];
                    curve.x[i] = note.Time;
                    curve.a[i] = valueSelector.Invoke(note);
                }
                for (int i = 0; i < interpolationNotes.Length - 1; i++) {
                    curve.b[i] = (curve.a[i + 1] - curve.a[i]) / (curve.x[i + 1] - curve.x[i]);
                }
                Array.Clear(curve.c, 0, curve.c.Length);
                Array.Clear(curve.d, 0, curve.d.Length);

                return curve;
            }

            public static CurveLineData Cubic(ReadOnlySpan<NoteData> interpolationNotes, Func<NoteData, float> valueSelector)
            {
                var curve = new CurveLineData(interpolationNotes.Length);

                int count = interpolationNotes.Length;
                int n = count - 1;

                for (int i = 0; i < count; i++) {
                    curve.x[i] = interpolationNotes[i].Time;
                    curve.a[i] = valueSelector.Invoke(interpolationNotes[i]);
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

            public float? GetValue(float time)
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

            public SpanOwner<NoteCoord> GetRenderValues(float renderMinTime, float renderMaxTime)
            {
                if (MinX >= renderMaxTime || MaxX <= renderMinTime || renderMinTime >= renderMaxTime)
                    return default;

                float min = Mathf.Max(MinX, renderMinTime);
                float max = Mathf.Min(MaxX, renderMaxTime);
                var coords = SpanOwner<NoteCoord>.Allocate(CubicCurveSegmentCount);
                var span = coords.Span;
                for (int i = 0; i < CubicCurveSegmentCount; i++) {
                    float time = Mathf.Lerp(min, max, (float)i / CubicCurveSegmentCount);
                    float pos = GetValue(time).Value;
                    span[i] = NoteCoord.ClampPosition(time, pos);
                }
                return coords;
            }
        }
    }
}