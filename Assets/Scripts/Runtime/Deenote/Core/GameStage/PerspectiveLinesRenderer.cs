#nullable enable

using Deenote.Core.GamePlay;
using Deenote.Library.Components;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Deenote.Core.GameStage
{
    /// <summary>
    /// Render all lines in the perspective view.
    /// The lines should be submitted in <c>Update</c> calls.
    /// The add line methods should be called in each frame when the line should be displayed,
    /// since the list of lines to display is reset after each frame.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class PerspectiveLinesRenderer : MonoBehaviour
    {
        private static readonly VertexAttributeDescriptor[] VertexLayout = {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
            new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 1)
        };
        private static Mesh? _emptyMesh;
        private static readonly int FadeInZ = Shader.PropertyToID("_FadeInZ");
        private static readonly int CutOffZ = Shader.PropertyToID("_CutOffZ");
        private static readonly int SmoothingPx = Shader.PropertyToID("_SmoothingPx");
        private static readonly int ActualViewSize = Shader.PropertyToID("_ActualViewSize");

        [SerializeField] MeshFilter _meshFilter = null!;
        [SerializeField] MeshRenderer _meshRenderer = null!;
        private List<VertexData> _vertices = new();
        private MaterialPropertyBlock _props = null!;

        private GamePlayManager _game = default!;

        public event Action<LineCollector>? LineCollecting;

        private void Awake()
        {
            _meshRenderer.sortingLayerName = "Lines";
            _props = new MaterialPropertyBlock();
        }

        internal void OnInstantiate(GamePlayManager manager)
        {
            _game = manager;
            _game.RegisterNotificationAndInvoke(
                GamePlayManager.NotificationFlag.SuddenPlus,
                _OnSuddenPlusChanged);
        }

        private void OnDestroy()
        {
            _game.UnregisterNotification(
                GamePlayManager.NotificationFlag.SuddenPlus,
                _OnSuddenPlusChanged);
        }

        private void Update()
        {
            Vector2 c = default!, d = default;
            var e = c * d;
            // TODO: Temp, find a way to render lines that does not depends on ui
            var viewSize = new Vector2(1280, 720);// MainWindow.PerspectiveViewPanelView.ViewSize;
            _props.SetFloat(SmoothingPx, 2.0f); // TODO: is it necessary for this to be configurable?
            _props.SetVector(ActualViewSize, new Vector4(viewSize.x, viewSize.y));
            _meshRenderer.SetPropertyBlock(_props);
        }

        private void LateUpdate()
        {
            var collector = new LineCollector(this);
            LineCollecting?.Invoke(collector);
            _meshFilter.mesh = GenerateMesh();
        }

        private void _OnSuddenPlusChanged(GamePlayManager manager)
        {
            manager.AssertStageLoaded();

            var args = manager.Stage.Args;
            float percent = manager.VisibleRangePercentage;
            float cutoff = args.NotePanelBaseLength * percent;
            _props.SetFloat(CutOffZ, cutoff);
            _props.SetFloat(FadeInZ, cutoff * args.NoteFadeInRangePercent);
        }

        private Mesh GenerateMesh()
        {
            // No lines
            if (_vertices.Count == 0) {
                if (_emptyMesh is not null) return _emptyMesh;
                _emptyMesh = new Mesh();
                _emptyMesh.SetVertexBufferParams(0, VertexLayout);
                return _emptyMesh;
            }

            // Add end (sentinel) vertex, both the position and prev position
            // will be the same as the last actual vertex.
            VertexData sentinel = _vertices[^1];
            sentinel.Positions.z = sentinel.Positions.x;
            sentinel.Positions.w = sentinel.Positions.y;
            _vertices.Add(sentinel);

            Mesh mesh = new();
            mesh.SetVertexBufferParams(_vertices.Count, VertexLayout);
            mesh.SetVertexBufferData(_vertices, 0, 0, _vertices.Count);
            NativeArray<int> indices = new(_vertices.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < _vertices.Count; i++)
                indices[i] = i;
            mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
            _vertices.Clear();
            return mesh;
        }

        private void OnValidate()
        {
            _meshFilter ??= GetComponent<MeshFilter>();
            _meshRenderer ??= GetComponent<MeshRenderer>();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VertexData
        {
            /// <summary>
            /// Positions of the current vertex and the previous vertex in world space,
            /// only x and z dimensions (y = 0).
            /// </summary>
            public Vector4 Positions;

            public Color32 Color;

            /// <summary>
            /// Width in screen space px.
            /// </summary>
            public float Width;
        }

        public struct LineCollector
        {
            private PerspectiveLinesRenderer _renderer;

            public LineCollector(PerspectiveLinesRenderer renderer)
            {
                _renderer = renderer;
            }

            public void AddLine(Vector2 p1, Vector2 p2, Color color, float width)
            {
                ReadOnlySpan<Vector2> points = stackalloc Vector2[] { p1, p2 };
                AddLineStrip(points, color, width);
            }

            public void AddLineStrip(ReadOnlySpan<Vector2> points, Color color, float width)
            {
                Vector2 prev = points[0];
                foreach (var point in points) {
                    _renderer._vertices.Add(new VertexData {
                        Positions = new Vector4(point.x, point.y, prev.x, prev.y),
                        Color = color,
                        Width = width
                    });
                    prev = point;
                }
            }
        }
    }
}