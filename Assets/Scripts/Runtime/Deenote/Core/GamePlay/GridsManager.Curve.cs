#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.Entities;
using Deenote.Entities.Comparisons;
using Deenote.Entities.Models;
using Deenote.Library;
using Deenote.Library.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.Library.Collections;
using UnityEngine;

namespace Deenote.Core.GamePlay
{
    partial class GridsManager
    {
        private const int CubicCurveSegmentCount = 400;

        private (List<NoteModel> InterpolationNotes, CurveKind Kind) _curveGenerationContext = (new(), default);
        private CurveLineData? _positionCurveData;
        private CurveLineData? _sizeCurveData;
        private CurveLineData? _speedCurveData;
        private NoteCoord[] _curveRenderPositions = new NoteCoord[CubicCurveSegmentCount];
        private bool _shouldRenderCurve;
        private bool _isCurveOn_bf;

        [MemberNotNullWhen(true, nameof(_positionCurveData))]
        public bool IsCurveOn
        {
            get => _isCurveOn_bf;
            private set {
                if (Utils.SetField(ref _isCurveOn_bf, value)) {
                    NotifyFlag(NotificationFlag.IsCurveOnChanged);
                }
            }
        }

        public (float Start, float End)? CurveTimeInterval => IsCurveOn ? (_positionCurveData.MinX, _positionCurveData.MaxX) : null;

        public void InitializeCurve(ReadOnlySpan<NoteModel> interpolationNotes, CurveKind kind)
        {
            NoteTimeComparer.AssertInOrder(interpolationNotes);

            _curveGenerationContext.InterpolationNotes.Clear();
            _positionCurveData = null;
            _sizeCurveData = null;
            _speedCurveData = null;

            if (interpolationNotes.Length < 2) {
                IsCurveOn = false;
                return;
            }
            else {
                _curveGenerationContext.InterpolationNotes.AddRange(interpolationNotes);
                _positionCurveData = kind switch {
                    CurveKind.Cubic => CurveLineData.Cubic(interpolationNotes, n => n.Position),
                    CurveKind.Linear => CurveLineData.Cubic(interpolationNotes, n => n.Position),
                    _ => throw new InvalidOperationException("Unknown curve kind"),
                };
                IsCurveOn = true;
                UpdateCurveLine();
            }
        }

        public float? GetCurveTransformedValue(float time, CurveApplyProperty property)
        {
            if (!IsCurveOn)
                return null;

            ref var curveData = ref Unsafe.NullRef<CurveLineData>();
            switch (property) {
                case CurveApplyProperty.Size: curveData = ref _sizeCurveData; break;
                case CurveApplyProperty.Speed: curveData = ref _speedCurveData; break;
                default: throw new InvalidOperationException("Unsupported property");
            }

            if (curveData is null) {
                Func<NoteModel, float> selector = property switch {
                    CurveApplyProperty.Size => n => n.Size,
                    CurveApplyProperty.Speed => n => n.Speed,
                    _ => throw new InvalidOperationException("Unsupported property"),
                };
                curveData = _curveGenerationContext.Kind switch {
                    CurveKind.Cubic => CurveLineData.Cubic(_curveGenerationContext.InterpolationNotes.AsSpan(), selector),
                    CurveKind.Linear => CurveLineData.Linear(_curveGenerationContext.InterpolationNotes.AsSpan(), selector),
                    _ => throw new InvalidOperationException("Unknown curve kind"),
                };
            }

            return curveData.GetValue(time);
        }

        public float? GetCurveTransformedPosition(float time)
        {
            if (!IsCurveOn)
                return null;
            return _positionCurveData.GetValue(time);
        }

        public void HideCurve() => IsCurveOn = false;

        private void SubmitCurveRender()
        {
            if (IsCurveOn && _shouldRenderCurve) {
                _game.AssertStageLoaded();

                var args = _game.Stage.GridLineArgs;
                var strip = (stackalloc Vector2[_curveRenderPositions.Length]);
                for (int i = 0; i < strip.Length; i++) {
                    var (x, z) = _game.Stage.ConvertNoteCoordToWorldPosition(_curveRenderPositions[i]);
                    strip[i] = new Vector2(x, z);
                }
                _game.PerspectiveLinesRenderer.AddLineStrip(strip, args.CurveLineColor, args.CurveLineWidth);
            }
        }

        private void UpdateCurveLine()
        {
            if (!IsCurveOn)
                return;
            if (_game.Stage is null)
                return;

            var currentTime = _game.MusicPlayer.Time;
            var stageMaxTime = _game.Stage.NoteAppearAheadTime;

            using var coords_so = _positionCurveData.GetRenderValues(currentTime, stageMaxTime);
            var coords = coords_so.Span;
            if (coords.IsEmpty) {
                // TODO:Next: 这个扔到submit里判断时间
                _shouldRenderCurve = false;
                return;
            }
            _shouldRenderCurve = true;

            Debug.Assert(coords.Length == _curveRenderPositions.Length);
            coords.CopyTo(_curveRenderPositions);
        }


        public enum CurveKind { Cubic, Linear, }

        public enum CurveApplyProperty { Size, Speed, }

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

            public static CurveLineData Linear(ReadOnlySpan<NoteModel> interpolationNotes, Func<NoteModel, float> valueSelector)
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

            public static CurveLineData Cubic(ReadOnlySpan<NoteModel> interpolationNotes, Func<NoteModel, float> valueSelector)
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
                    return SpanOwner<NoteCoord>.Empty;

                float min = Mathf.Max(MinX, renderMinTime);
                float max = Mathf.Min(MaxX, renderMaxTime);
                var coords = SpanOwner<NoteCoord>.Allocate(CubicCurveSegmentCount);
                var span = coords.Span;
                for (int i = 0; i < CubicCurveSegmentCount; i++) {
                    float time = Mathf.Lerp(min, max, (float)i / CubicCurveSegmentCount);
                    float pos = GetValue(time)!.Value;
                    span[i] = NoteCoord.ClampPosition(pos, time);
                }
                return coords;
            }
        }
    }
}