#nullable enable

using Deenote.Entities;
using Deenote.Library;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.GamePlay
{
    partial class GridsManager
    {
        private const int PositionGridMaxCount = 41;
        /// <summary>
        /// If distance from position to grid &lt;= this, treat as equal
        /// </summary>
        private const float PositionGridMinEqualityThreshold = 1e-4f;

        private List<PositionGridLineData> _positionGridLines = new();

        private PositionGridGenerationKind _positionGridGenerationKind_bf = PositionGridGenerationKind.ByKeyCount;
        private int _positionGridCount_bf;
        private float _positionGridOffset_bf;
        private bool _isPositionBorderVisible_bf;

        public PositionGridGenerationKind PositionGridGeneration
        {
            get => _positionGridGenerationKind_bf;
            set {
                if (Utils.SetField(ref _positionGridGenerationKind_bf, value, out var oldValue)) {
                    if (value is PositionGridGenerationKind.ByCountAndOffset) {
                        Debug.Assert(oldValue is PositionGridGenerationKind.ByKeyCount);

                        switch (_positionGridCount_bf) {
                            case < 2:
                                _isPositionBorderVisible_bf = false;
                                _positionGridOffset_bf = 0f;
                                break;
                            case 2:
                                _isPositionBorderVisible_bf = true;
                                _positionGridCount_bf = 0;
                                _positionGridOffset_bf = 0f;
                                break;
                            case > 2:
                                _isPositionBorderVisible_bf = true;
                                _positionGridCount_bf--;
                                _positionGridOffset_bf = 2 / _positionGridCount_bf;
                                break;
                        }
                    }
                    else {
                        Debug.Assert(value is PositionGridGenerationKind.ByKeyCount);
                        Debug.Assert(oldValue is PositionGridGenerationKind.ByCountAndOffset);

                        _positionGridCount_bf++;
                    }
                }
            }
        }

        public int PositionGridCount
        {
            get => _positionGridCount_bf;
            set {
                value = Mathf.Clamp(value, 0, PositionGridMaxCount);
                if (Utils.SetField(ref _positionGridCount_bf, value)) {
                    UpdatePositionGrids();
                    NotifyFlag(NotificationFlag.PositionGridChanged);
                }
            }
        }

        #region Legacy

        public bool IsPositionGridBorderVisible_Legacy
        {
            get {
                ValidateSettingKind(PositionGridGenerationKind.ByCountAndOffset);
                return _isPositionBorderVisible_bf;
            }
            set {
                ValidateSettingKind(PositionGridGenerationKind.ByCountAndOffset);
                Utils.SetField(ref _isPositionBorderVisible_bf, value);
            }
        }

        public int PositionGridCount_Legacy
        {
            get {
                ValidateSettingKind(PositionGridGenerationKind.ByCountAndOffset);
                return _positionGridCount_bf;
            }
            set {
                ValidateSettingKind(PositionGridGenerationKind.ByCountAndOffset);

                value = Mathf.Clamp(value, 0, PositionGridMaxCount - 1);
                if (Utils.SetField(ref _positionGridCount_bf, value)) {
                    UpdatePositionGrids();
                }
            }
        }

        public float PositionGridOffset_Legacy
        {
            get {
                ValidateSettingKind(PositionGridGenerationKind.ByCountAndOffset);
                return _positionGridOffset_bf;
            }
            set {
                ValidateSettingKind(PositionGridGenerationKind.ByCountAndOffset);

                value = Mathf.Clamp(value, -1f, 1f);
                if (Utils.SetField(ref _positionGridOffset_bf, value)) {
                    UpdatePositionGrids();
                }
            }
        }

        #endregion

        private void SubmitPositionGridsRender()
        {
            _game.AssertStageLoaded();

            var renderer = _game.PerspectiveLinesRenderer;
            var stage = _game.Stage;
            float minZ = stage.ConvertNoteCoordTimeToWorldZ(0f);
            float maxZ = stage.ConvertNoteCoordTimeToWorldZ(stage.NoteAppearAheadTime);
            var args = stage.GridLineArgs;
            foreach (var (pos, kind) in _positionGridLines) {
                var x = stage.ConvertNoteCoordPositionToWorldX(pos);
                renderer.AddLine(new Vector2(x, minZ), new Vector2(x, maxZ),
                    args.PositionGridLineColor,
                    width: kind is PositionGridKind.Border ? args.PositionGridBorderWidth : args.PositionGridLineWidth);
            }

            // Legacy system
            if (PositionGridGeneration is PositionGridGenerationKind.ByCountAndOffset && IsPositionGridBorderVisible_Legacy) {
                float minx = stage.ConvertNoteCoordPositionToWorldX(-EntityArgs.StageMaxPosition);
                float maxx = stage.ConvertNoteCoordPositionToWorldX(EntityArgs.StageMaxPosition);
                renderer.AddLine(new Vector2(minx, minZ), new Vector2(minx, maxZ),
                    args.PositionGridLineColor, args.PositionGridBorderWidth);
                renderer.AddLine(new Vector2(maxx, minZ), new Vector2(maxx, maxZ),
                    args.PositionGridLineColor, args.PositionGridBorderWidth);
            }
        }

        private void UpdatePositionGrids()
        {
            _positionGridLines.Clear();
            switch (PositionGridGeneration) {
                case PositionGridGenerationKind.ByCountAndOffset:
                    for (int i = 0; i < PositionGridCount_Legacy; i++) {
                        var pos = GetPositionGridPosition_Legacy(i);
                        _positionGridLines.Add(new(pos, PositionGridKind.Default));
                    }
                    break;
                case PositionGridGenerationKind.ByKeyCount:
                    ByKeyCount();
                    break;
                default:
                    Debug.Assert(false, "Unknown enum value");
                    break;
            }

            return;

            void ByKeyCount()
            {
                switch (PositionGridCount) {
                    case <= 0:
                        break;
                    case 1: {
                        _positionGridLines.Add(new(0f, PositionGridKind.Default));
                        break;
                    }
                    default: {
                        _positionGridLines.Add(new(-EntityArgs.StageMaxPosition, PositionGridKind.Border));
                        for (int i = 1; i < PositionGridCount - 1; i++) {
                            float pos = GetPositionGridPosition(i);
                            _positionGridLines.Add(new(pos, PositionGridKind.Default));
                        }
                        _positionGridLines.Add(new(EntityArgs.StageMaxPosition, PositionGridKind.Border));
                        break;
                    }
                }
            }
        }

        /// <returns><see langword="null"/> if grid count is 0</returns>
        public float? GetNearestPositionGridPosition(float position)
        {
            return PositionGridGeneration switch {
                PositionGridGenerationKind.ByCountAndOffset => ByLegacy(position),
                PositionGridGenerationKind.ByKeyCount => ByKeyCount(position),
                _ => throw new System.NotImplementedException(),
            };

            float? ByLegacy(float position)
            {
                if (PositionGridCount_Legacy == 0)
                    return null;

                float minDist = float.MaxValue;
                float result = default;
                for (int i = 0; i < PositionGridCount_Legacy; i++) {
                    float gridPos = GetPositionGridPosition_Legacy(i);
                    float dist = Mathf.Abs(gridPos - position);
                    if (dist > minDist)
                        continue;
                    minDist = dist;
                    result = gridPos;
                }

                if (IsPositionGridBorderVisible_Legacy) {
                    float dist = Mathf.Abs(position + EntityArgs.StageMaxPosition);
                    if (dist < minDist) {
                        minDist = dist;
                        result = -EntityArgs.StageMaxPosition;
                    }
                    dist = Mathf.Abs(position - EntityArgs.StageMaxPosition);
                    if (dist < minDist) {
                        result = EntityArgs.StageMaxPosition;
                    }
                }
                return result;
            }

            float? ByKeyCount(float position)
            {
                if (PositionGridCount == 0)
                    return null;
                float minDist = float.MaxValue;
                float result = default;
                for (int i = 0; i < PositionGridCount; i++) {
                    float gridPos = GetPositionGridPosition(i);
                    float dist = Mathf.Abs(gridPos - position);
                    if (minDist <= dist)
                        break;
                    minDist = dist;
                    result = gridPos;
                }
                return result;
            }
        }

        public float? FloorToNearestNextPositionGridPosition(float position)
        {
            return PositionGridGeneration switch {
                PositionGridGenerationKind.ByCountAndOffset => ByLegacy(position),
                PositionGridGenerationKind.ByKeyCount => ByKeyCount(position),
                _ => throw new System.NotImplementedException(),
            };

            float? ByLegacy(float position)
            {
                if (PositionGridCount_Legacy == 0)
                    return null;

                float? result = default;
                for (int i = 0; i < PositionGridCount_Legacy; i++) {
                    float gridPos = GetPositionGridPosition_Legacy(i);
                    if (position > gridPos + PositionGridMinEqualityThreshold)
                        result = gridPos;
                }

                if (IsPositionGridBorderVisible_Legacy && position > -EntityArgs.StageMaxPosition)
                    result = -EntityArgs.StageMaxPosition;

                return result;
            }

            float? ByKeyCount(float position)
            {
                if (PositionGridCount == 0)
                    return null;
                else if (PositionGridCount == 1) {
                    var gridPos = GetPositionGridPosition(0);
                    return position > gridPos ? gridPos : null;
                }

                for (int i = PositionGridCount - 1; i >= 0; i--) {
                    float gridPos = GetPositionGridPosition(i);
                    if (gridPos + PositionGridMinEqualityThreshold < position)
                        return gridPos;
                }
                Debug.Assert(position <= -EntityArgs.StageMaxPosition + PositionGridMinEqualityThreshold);
                return position == -EntityArgs.StageMaxPosition ? null : -EntityArgs.StageMaxPosition;
            }
        }

        public float? CeilToNearestNextPositionGridPosition(float position)
        {
            return PositionGridGeneration switch {
                PositionGridGenerationKind.ByCountAndOffset => ByLegacy(position),
                PositionGridGenerationKind.ByKeyCount => ByKeyCount(position),
                _ => throw new System.NotImplementedException(),
            };

            float? ByLegacy(float position)
            {
                if (PositionGridCount_Legacy == 0)
                    return null;

                float? result = default;
                for (int i = 0; i < PositionGridCount_Legacy; i++) {
                    float gridPos = GetPositionGridPosition_Legacy(i);
                    if (position < gridPos - PositionGridMinEqualityThreshold) {
                        if (result is { } res)
                            result = Mathf.Min(res, gridPos);
                        else
                            result = gridPos;
                    }
                }

                if (IsPositionGridBorderVisible_Legacy && position < EntityArgs.StageMaxPosition) {
                    if (result is { } res)
                        result = Mathf.Min(res, EntityArgs.StageMaxPosition);
                    else
                        result = EntityArgs.StageMaxPosition;
                }

                return result;
            }

            float? ByKeyCount(float position)
            {
                if (PositionGridCount == 0)
                    return null;
                else if (PositionGridCount == 1) {
                    var gridPos = GetPositionGridPosition(0);
                    return position < gridPos ? gridPos : null;
                }

                for (int i = 0; i < PositionGridCount; i++) {
                    float gridPos = GetPositionGridPosition(i);
                    if (gridPos - PositionGridMinEqualityThreshold > position)
                        return gridPos;
                }
                Debug.Assert(position >= EntityArgs.StageMaxPosition - PositionGridMinEqualityThreshold);
                return position == EntityArgs.StageMaxPosition ? null : EntityArgs.StageMaxPosition;
            }
        }

        private float GetPositionGridPosition(int index)
        {
            return EntityArgs.StageMaxPositionWidth * ((float)index / (PositionGridCount - 1)) - EntityArgs.StageMaxPosition;
        }

        private float GetPositionGridPosition_Legacy(int index)
        {
            float result = (index + 0.5f) / PositionGridCount_Legacy * 4 - 2;
            result += PositionGridOffset_Legacy;

            if (result < -EntityArgs.StageMaxPosition)
                result += EntityArgs.StageMaxPositionWidth;
            else if (result > EntityArgs.StageMaxPosition)
                result -= EntityArgs.StageMaxPositionWidth;
            return result;
        }

        private void ValidateSettingKind(PositionGridGenerationKind kind)
        {
            if (PositionGridGeneration != kind)
                throw new System.InvalidOperationException($"Invalid setting kind, required {kind}");
        }

        private enum PositionGridKind
        {
            Default,
            Border,
        }

        public enum PositionGridGenerationKind
        {
            ByCountAndOffset,
            ByKeyCount,
        }

        private record struct PositionGridLineData(
            float Position,
            PositionGridKind Kind);
    }
}