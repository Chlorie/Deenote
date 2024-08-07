using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Deenote.Utilities;
using Deenote.UI.Windows;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Deenote.GameStage
{
    /// <summary>
    /// Render all lines in the perspective view.
    /// The lines should be submitted in <c>Update</c> calls.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PerspectiveLinesRenderer : SingletonBehavior<PerspectiveLinesRenderer>
    {
        public void AddLineStrip(Span<Vector2> points, Color color, float width)
        {
            Vector2 prev = points[0];
            foreach (var point in points)
            {
                _vertices.Add(new VertexData
                {
                    Positions = new Vector4(point.x, point.y, prev.x, prev.y),
                    Color = color,
                    Width = width
                });
                prev = point;
            }
        }

        public void AddLine(Vector2 p1, Vector2 p2, Color color, float width)
        {
            Span<Vector2> points = stackalloc Vector2[2] { p1, p2 };
            AddLineStrip(points, color, width);
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

        private static readonly VertexAttributeDescriptor[] VertexLayout =
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 4),
            new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 1)
        };

        private static Mesh? _emptyMesh;
        private static readonly int FadeInZ = Shader.PropertyToID("_FadeInZ");
        private static readonly int CutOffZ = Shader.PropertyToID("_CutOffZ");
        private static readonly int SmoothingPx = Shader.PropertyToID("_SmoothingPx");
        private static readonly int ActualViewSize = Shader.PropertyToID("_ActualViewSize");

        [SerializeField][HideInInspector] private MeshFilter _meshFilter = null!;
        [SerializeField][HideInInspector] private MeshRenderer _meshRenderer = null!;
        [SerializeField] private Color _testColor = Color.white;
        [SerializeField] private float _testWidth = 4.0f;
        [SerializeField] private int _subDivisions = 8;
        [SerializeField] private float _testSmoothingPx = 1.0f;
        private List<VertexData> _vertices = new();
        private MaterialPropertyBlock _props = null!;

        private void OnValidate()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
        }

        protected override void Awake()
        {
            base.Awake();
            // TODO: set sorting layer here
            // _meshRenderer.sortingLayerName = "Default";
            // TODO: do not use hardcoded values
            _props = new MaterialPropertyBlock();
            _props.SetFloat(FadeInZ, 18.0f);
            _props.SetFloat(CutOffZ, 40.0f);
        }

        private Mesh GenerateMesh()
        {
            // No lines
            if (_vertices.Count == 0)
            {
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

        private void Update()
        {
            var viewSize = PerspectiveViewWindow.Instance.ViewSize;
            _props.SetFloat(SmoothingPx, _testSmoothingPx);
            _props.SetVector(ActualViewSize, new Vector4(viewSize.x, viewSize.y));
            _meshRenderer.SetPropertyBlock(_props);

            for (int i = 0; i <= _subDivisions; i++)
            {
                float x = 6.52f / _subDivisions * i - 3.26f;
                AddLine(new Vector2(x, 0), new Vector2(x, 40f), _testColor, _testWidth);
            }
        }

        private void LateUpdate() => _meshFilter.mesh = GenerateMesh();
    }
}
