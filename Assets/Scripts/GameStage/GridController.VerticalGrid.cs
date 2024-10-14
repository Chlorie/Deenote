using System.Collections.Generic;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GridController
    {
        private const int MaxKeyCount = 41;

        private enum VerticalGridKind
        {
            Default,
            Border
        }

        private List<(float, VerticalGridKind)> _verticalGridData = new();

        private VerticalGridGenerationKind _verticalGridGenerationKind;

        [Header("Vertical Grid")]
        [SerializeField, Range(0, 41)]
        private int __verticalGridCount;
        [SerializeField, Range(-1f, 1f)]
        private float __verticalGridOffset;
        [SerializeField]
        private bool __isVerticalBorderVisible;

        public VerticalGridGenerationKind VerticalGridGeneration
        {
            get => _verticalGridGenerationKind;
            set {
                if (_verticalGridGenerationKind == value)
                    return;
                if (value == VerticalGridGenerationKind.ByCountAndOffset) {
                    switch (__verticalGridCount) {
                        case < 2:
                            __isVerticalBorderVisible = false;
                            __verticalGridOffset = 0f;
                            break;
                        case 2:
                            __isVerticalBorderVisible = true;
                            __verticalGridCount = 0;
                            __verticalGridOffset = 0f;
                            break;
                        case > 2:
                            __isVerticalBorderVisible = true;
                            __verticalGridCount--;
                            __verticalGridOffset = 2 / __verticalGridCount;
                            break;
                    }
                }
                else {
                    // ByKeyCount
                    __verticalGridCount++;
                }
                // We do not immediately update grids when generation kind changed
            }
        }

        public int VerticalGridCount
        {
            get => __verticalGridCount;
            set {
                value = Mathf.Clamp(value, 0, MaxKeyCount);
                if (__verticalGridCount == value)
                    return;

                __verticalGridCount = value;
                UpdateVerticalGrids();
                _propertyChangedNotifier.Invoke(this, NotifyProperty.VerticalGridCount);
                _editorPropertiesWindow.NotifyVerticalGridCountChanged(__verticalGridCount);
            }
        }

        public bool IsBorderVisible_Legacy
        {
            get => __isVerticalBorderVisible;
            set {
                Debug.Assert(VerticalGridGeneration is VerticalGridGenerationKind.ByCountAndOffset);
                __isVerticalBorderVisible = value;
            }
        }

        /// <summary>
        /// Valid when <see cref="VerticalGridGenerationKind.ByCountAndOffset"/>
        /// </summary>
        public int VerticalGridCount_Legacy
        {
            get => __verticalGridCount;
            set {
                Debug.Assert(VerticalGridGeneration is VerticalGridGenerationKind.ByCountAndOffset);
                value = Mathf.Clamp(value, 0, MaxKeyCount - 1);
                if (__verticalGridCount == value)
                    return;

                __verticalGridCount = value;
                UpdateVerticalGrids();
            }
        }

        /// <summary>
        /// Valid when <see cref="VerticalGridGenerationKind.ByCountAndOffset"/>
        /// </summary>
        public float VerticalGridOffset_Legacy
        {
            get => __verticalGridOffset;
            set {
                Debug.Assert(VerticalGridGeneration is VerticalGridGenerationKind.ByCountAndOffset);
                value = Mathf.Clamp(value, -1f, 1f);
                if (__verticalGridOffset == value)
                    return;

                __verticalGridOffset = value;
                UpdateVerticalGrids();
            }
        }

        private void DrawVerticalGrids()
        {
            var lineRenderer = PerspectiveLinesRenderer.Instance;
            float maxZ = MainSystem.Args.NoteAppearZ;
            foreach (var (x, kind) in _verticalGridData) {
                lineRenderer.AddLine(new Vector2(x, 0), new Vector2(x, maxZ),
                    color: MainSystem.Args.SubBeatLineColor,
                    width: kind is VerticalGridKind.Border ? MainSystem.Args.GridBorderWidth : MainSystem.Args.GridWidth);
            }

            if (!IsBorderVisible_Legacy ||
                _verticalGridGenerationKind is not VerticalGridGenerationKind.ByCountAndOffset)
                return;
            // Legacy system
            float maxX = MainSystem.Args.PositionToX(MainSystem.Args.StageMaxPosition);
            lineRenderer.AddLine(new Vector2(-maxX, 0), new Vector2(-maxX, maxZ),
                MainSystem.Args.SubBeatLineColor, MainSystem.Args.GridBorderWidth);
            lineRenderer.AddLine(new Vector2(maxX, 0), new Vector2(maxX, maxZ),
                MainSystem.Args.SubBeatLineColor, MainSystem.Args.GridBorderWidth);
        }

        public void UpdateVerticalGrids()
        {
            _verticalGridData.Clear();
            switch (VerticalGridGeneration) {
                case VerticalGridGenerationKind.ByCountAndOffset:
                    for (int i = 0; i < VerticalGridCount_Legacy; i++) {
                        float x = MainSystem.Args.PositionToX(GetVerticalGridPosition_Legacy(i));
                        _verticalGridData.Add((x, VerticalGridKind.Default));
                    }
                    break;
                case VerticalGridGenerationKind.ByKeyCount:
                    ByKeyCount();
                    break;
                default:
                    Debug.Assert(false, $"Unknown enum value of {nameof(VerticalGridGenerationKind)}");
                    break;
            }
            return;

            void ByKeyCount()
            {
                switch (VerticalGridCount) {
                    case <= 0:
                        break;
                    case 1: {
                        _verticalGridData.Add((0f, VerticalGridKind.Default));
                        break;
                    }
                    default: {
                        for (int i = 0; i < VerticalGridCount - 2; i++) {
                            float x = MainSystem.Args.PositionToX(GetVerticalGridPosition(i + 1));
                            _verticalGridData.Add((x, VerticalGridKind.Default));
                        }
                        float maxX = MainSystem.Args.PositionToX(MainSystem.Args.StageMaxPosition);
                        _verticalGridData.Add((-maxX, VerticalGridKind.Border));
                        _verticalGridData.Add((maxX, VerticalGridKind.Border));
                        break;
                    }
                }
            }
        }

        /// <returns><see langword="null"/> if gridCount == 0</returns>
        public float? GetNearestVerticalGridPosition(float position)
        {
            switch (VerticalGridGeneration) {
                case VerticalGridGenerationKind.ByCountAndOffset: {
                    if (VerticalGridCount_Legacy == 0)
                        return null;

                    float minDist = float.MaxValue;
                    float snapPos = default;
                    for (int i = 0; i < VerticalGridCount_Legacy; i++) {
                        float gridPos = GetVerticalGridPosition_Legacy(i);
                        float dist = Mathf.Abs(gridPos - position);
                        if (!(dist < minDist)) continue;
                        minDist = dist;
                        snapPos = gridPos;
                    }

                    if (IsBorderVisible_Legacy) {
                        float dist = Mathf.Abs(position + MainSystem.Args.StageMaxPosition);
                        if (dist < minDist) {
                            minDist = dist;
                            snapPos = -MainSystem.Args.StageMaxPosition;
                        }
                        dist = Mathf.Abs(position - MainSystem.Args.StageMaxPosition);
                        if (dist < minDist) {
                            snapPos = MainSystem.Args.StageMaxPosition;
                        }
                    }

                    return snapPos;
                }
                case VerticalGridGenerationKind.ByKeyCount: {
                    if (VerticalGridCount == 0)
                        return null;

                    float minDist = float.MaxValue;
                    float snapPos = default;
                    for (int i = 0; i < VerticalGridCount; i++) {
                        float gridPos = GetVerticalGridPosition(i);
                        float dist = Mathf.Abs(gridPos - position);
                        if (minDist <= dist)
                            break;

                        minDist = dist;
                        snapPos = gridPos;
                    }

                    return snapPos;
                }
                default:
                    Debug.Assert(false, $"Unknown enum value of {nameof(VerticalGridGenerationKind)}");
                    return null;
            }
        }

        public float? FloorToNearestNextVerticalGridPosition(float position)
        {
            switch (VerticalGridGeneration) {
                case VerticalGridGenerationKind.ByCountAndOffset: {
                    if (VerticalGridCount_Legacy == 0)
                        return null;

                    float? snapPos = null;
                    for (int i = 0; i < VerticalGridCount_Legacy; i++) {
                        float gridPos = GetVerticalGridPosition_Legacy(i);
                        if (position > gridPos)
                            snapPos = gridPos;
                    }

                    if (IsBorderVisible_Legacy && position > -MainSystem.Args.StageMaxPosition)
                        snapPos = -MainSystem.Args.StageMaxPosition;

                    return snapPos;
                }
                case VerticalGridGenerationKind.ByKeyCount: {
                    if (VerticalGridCount == 0)
                        return null;

                    float pos = position;
                    for (int i = 0; i < VerticalGridCount; i++) {
                        float gridPos = GetVerticalGridPosition(i);
                        if (gridPos >= position)
                            return pos;
                        pos = gridPos;
                    }
                    Debug.Assert(false, "Unreachable");
                    return null;
                }
                default:
                    Debug.Assert(false, $"Unknown enum value of {nameof(VerticalGridGenerationKind)}");
                    return null;
            }
        }

        public float? CeilToNearestNextVerticalGridPosition(float position)
        {
            switch (VerticalGridGeneration) {
                case VerticalGridGenerationKind.ByCountAndOffset: {
                    if (VerticalGridCount_Legacy == 0)
                        return null;

                    float? snapPos = null;
                    for (int i = 0; i < VerticalGridCount_Legacy; i++) {
                        float gridPos = GetVerticalGridPosition_Legacy(i);
                        if (position < gridPos)
                            snapPos = gridPos;
                    }

                    if (IsBorderVisible_Legacy && position < MainSystem.Args.StageMaxPosition)
                        snapPos = MainSystem.Args.StageMaxPosition;

                    return snapPos;
                }
                case VerticalGridGenerationKind.ByKeyCount: {
                    for (int i = 0; i < VerticalGridCount; i++) {
                        float gridPos = GetVerticalGridPosition(i);
                        if (gridPos > position)
                            return gridPos;
                    }
                    Debug.Assert(false, "Unreachable");
                    return null;
                }
                default:
                    Debug.Assert(false, $"Unknown enum value of {nameof(VerticalGridGenerationKind)}");
                    return null;
            }
        }

        #region Pool & Unity

        private void SetVerticalGridPosition(LineRenderer line, float position)
        {
            var x = MainSystem.Args.PositionToX(position);
            line.SetPosition(0, new Vector3(x, 0, 0f));
            line.SetPosition(1, new Vector3(x, 0, MainSystem.Args.NoteAppearZ));
        }

        #endregion

        private float GetVerticalGridPosition(int index) =>
            2 * MainSystem.Args.StageMaxPosition * ((float)index / (VerticalGridCount - 1)) -
            MainSystem.Args.StageMaxPosition;

        private float GetVerticalGridPosition_Legacy(int index)
        {
            float result = (index + 0.5f) / VerticalGridCount_Legacy * 4 - 2 + VerticalGridOffset_Legacy;
            if (result < -MainSystem.Args.StageMaxPosition)
                result += MainSystem.Args.StageMaxPosition * 2;
            else if (result > MainSystem.Args.StageMaxPosition)
                result -= MainSystem.Args.StageMaxPosition * 2;
            return result;
        }

        public enum VerticalGridGenerationKind
        {
            ByCountAndOffset,
            ByKeyCount,
        }
    }
}