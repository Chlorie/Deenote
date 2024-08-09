using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using UnityEngine;

namespace Deenote.GameStage
{
    partial class GridController
    {
        private const int MaxKeyCount = 41;

        [Header("Vertical Grid")]
        [SerializeField] GameObject _bordersGameObject;
        [SerializeField] LineRenderer _leftBorderLine;
        [SerializeField] LineRenderer _rightBorderLine;
        [SerializeField] Transform _verticalGridParentTransform;

        private PooledObjectListView<LineRenderer> _verticalGrids;

        private VerticalGridGenerationKind _verticalGridGenerationKind;

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
                            __verticalGridCount = __verticalGridCount - 1;
                            __verticalGridOffset = 2 / __verticalGridCount;
                            break;
                    }
                }
                else { // ByKeyCount
                    __verticalGridCount = __verticalGridCount + 1;
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
                _editorPropertiesWindow.NotifyVerticalGridCountChanged(__verticalGridCount);
            }
        }

        public bool IsBorderVisible_Legacy
        {
            get => __isVerticalBorderVisible;
            set {
                Debug.Assert(VerticalGridGeneration is VerticalGridGenerationKind.ByCountAndOffset);
                if (__isVerticalBorderVisible == value)
                    return;

                __isVerticalBorderVisible = value;
                _bordersGameObject.SetActive(value);
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

        public void UpdateVerticalGrids()
        {
            switch (VerticalGridGeneration) {
                case VerticalGridGenerationKind.ByCountAndOffset:
                    ByCountAndOffset();
                    break;
                case VerticalGridGenerationKind.ByKeyCount:
                    ByKeyCount();
                    break;
                default:
                    Debug.Assert(false, $"Unknown enum value of {nameof(VerticalGridGenerationKind)}");
                    break;
            }

            void ByCountAndOffset()
            {
                _verticalGrids.SetCount(VerticalGridCount_Legacy);
                for (int i = 0; i < VerticalGridCount_Legacy; i++) {
                    SetVerticalGridPosition(_verticalGrids[i], GetVerticalGridPosition_Legacy(i));
                }
            }

            void ByKeyCount()
            {
                switch (VerticalGridCount) {
                    case <= 0: {
                        _verticalGrids.Clear();
                        _bordersGameObject.SetActive(false);
                        break;
                    }
                    case 1: {
                        _verticalGrids.SetCount(1);
                        SetVerticalGridPosition(_verticalGrids[0], position: 0f);
                        _bordersGameObject.SetActive(false);
                        break;
                    }
                    default: {
                        _verticalGrids.SetCount(VerticalGridCount - 2);
                        for (int i = 0; i < VerticalGridCount - 2; i++) {
                            SetVerticalGridPosition(_verticalGrids[i], GetVerticalGridPosition(i + 1));
                        }
                        _bordersGameObject.SetActive(true);
                        SetVerticalGridPosition(_leftBorderLine, -MainSystem.Args.StageMaxPosition);
                        SetVerticalGridPosition(_rightBorderLine, MainSystem.Args.StageMaxPosition);

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
                        if (dist < minDist) {
                            minDist = dist;
                            snapPos = gridPos;
                        }
                    }

                    if (IsBorderVisible_Legacy) {
                        float dist = Mathf.Abs(position - (-MainSystem.Args.StageMaxPosition));
                        if (dist < minDist) {
                            minDist = dist;
                            snapPos = -MainSystem.Args.StageMaxPosition;
                        }
                        dist = Mathf.Abs(position - MainSystem.Args.StageMaxPosition);
                        if (dist < minDist) {
                            minDist = dist;
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

                    float minDist = float.MaxValue;
                    float? snapPos = null;
                    for (int i = 0; i < VerticalGridCount_Legacy; i++) {
                        float gridPos = GetVerticalGridPosition_Legacy(i);
                        if (position > gridPos) {
                            minDist = position - gridPos;
                            snapPos = gridPos;
                        }
                    }

                    if (IsBorderVisible_Legacy) {
                        if (position > -MainSystem.Args.StageMaxPosition) {
                            minDist = position - (-MainSystem.Args.StageMaxPosition);
                            snapPos = -MainSystem.Args.StageMaxPosition;
                        }
                    }

                    return snapPos;
                }
                case VerticalGridGenerationKind.ByKeyCount: {
                    if (VerticalGridCount == 0)
                        return null;

                    float pos = position;
                    for (int i = 0; i < VerticalGridCount; i++) {
                        float gridPos = GetVerticalGridPosition(i);
                        if (gridPos >= position)
                            break;
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

                    float minDist = float.MaxValue;
                    float? snapPos = null;
                    for (int i = 0; i < VerticalGridCount_Legacy; i++) {
                        float gridPos = GetVerticalGridPosition_Legacy(i);
                        if (position < gridPos) {
                            minDist = gridPos - position;
                            snapPos = gridPos;
                        }
                    }

                    if (IsBorderVisible_Legacy) {
                        if (position < MainSystem.Args.StageMaxPosition) {
                            minDist = MainSystem.Args.StageMaxPosition - position;
                            snapPos = MainSystem.Args.StageMaxPosition;
                        }
                    }

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
            var z = MainSystem.Args.OffsetTimeToZ(_stage.StageNoteAheadTime);
            line.SetPosition(0, _stage.NormalizeGridPosition(new Vector3(x, 0, 0f)));
            line.SetPosition(1, _stage.NormalizeGridPosition(new Vector3(x, 0, z)));

            float startAlphaUnclamped = _stage.StageNoteAheadTime / (_stage.StageNoteAheadTime * _stage.Args.NoteFadeInRangePercent);
            line.SetGradientColor(startAlphaUnclamped, 0f);
        }

        private void AwakeVerticalGrid()
        {
            _verticalGrids = new PooledObjectListView<LineRenderer>(UnityUtils.CreateObjectPool(() =>
            {
                var line = Instantiate(_linePrefab, _verticalGridParentTransform);
                line.sortingOrder = -14;
                line.widthMultiplier = MainSystem.Args.GridWidth;
                line.positionCount = 2;
                line.SetSolidColor(MainSystem.Args.SubBeatLineColor);
                return line;
            }));

            _leftBorderLine.widthMultiplier = MainSystem.Args.GridBorderWidth;
            _leftBorderLine.SetPosition(0, _stage.NormalizeGridPosition(_leftBorderLine.GetPosition(0)));
            _leftBorderLine.SetPosition(1, _stage.NormalizeGridPosition(_leftBorderLine.GetPosition(1)));
            _rightBorderLine.widthMultiplier = MainSystem.Args.GridBorderWidth;
            _rightBorderLine.SetPosition(0, _stage.NormalizeGridPosition(_rightBorderLine.GetPosition(0)));
            _rightBorderLine.SetPosition(1, _stage.NormalizeGridPosition(_rightBorderLine.GetPosition(1)));
        }

        #endregion

        private float GetVerticalGridPosition(int index) => 2 * MainSystem.Args.StageMaxPosition * ((float)index / (VerticalGridCount - 1)) - MainSystem.Args.StageMaxPosition;

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