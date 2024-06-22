using Deenote.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.GameStage
{
    partial class GridController
    {
        [Header("Vertical Grid")]
        [SerializeField] GameObject _bordersGameObject;

        private ObjectPool<LineRenderer> _verticalGridPool;
        private List<LineRenderer> _verticalGrids;

        [SerializeField] float _verticalGridOffset;
        // [SerializeField] bool _showBorder;

        public bool IsBorderVisible
        {
            get => _bordersGameObject.activeSelf;
            set => _bordersGameObject.SetActive(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyCount">
        /// Count of line including border,
        /// Eg: 5 will draw lines for 5 key placement, on -2, -1, 0, 1, 2.
        /// </param>
        public void SetVerticalGridEqualInterval(int keyCount)
        {
            if (keyCount <= 0) {
                foreach (var line in _verticalGrids)
                    _verticalGridPool.Release(line);
                _verticalGrids.Clear();
            }
            else if (keyCount == 1) {
                if (_verticalGrids.Count < 1) {
                    _verticalGrids.Add(GetVerticalGrid(0 + _verticalGridOffset));
                }
                else {
                    SetVerticalGridPosition(_verticalGrids[0], 0 + _verticalGridOffset);
                    for (int i = 1; i < _verticalGrids.Count; i++)
                        _verticalGridPool.Release(_verticalGrids[i]);
                    _verticalGrids.RemoveRange(1, _verticalGrids.Count - 1);
                }
            }
            else {
                // TODO:Optimize
                foreach (var line in _verticalGrids)
                    _verticalGridPool.Release(line);
                _verticalGrids.Clear();
                // Generated position contains -2 and 2
                for (int i = 0; i < keyCount; i++)
                    _verticalGrids.Add(GetVerticalGrid(2 * MainSystem.Args.StageMaxPosition * ((float)i / (keyCount - 1)) - MainSystem.Args.StageMaxPosition + _verticalGridOffset));
            }
        }

        public float GetNearestVerticalGridPosition(float position)
        {
            float minDist = float.MaxValue;
            float snapPos = default;
            foreach (var gridPos in new VerticalGridEnumerable(this)) {
                float dist = Mathf.Abs(gridPos - position);
                if (minDist <= dist)
                    break;

                minDist = dist;
                snapPos = gridPos;
            }

            return snapPos;
        }

        /// <param name="position">-2 - 2</param>
        private LineRenderer GetVerticalGrid(float position)
        {
            var line = _verticalGridPool.Get();
            line.widthMultiplier = 0.035f;
            line.positionCount = 2;
            line.SetSolidColor(new Color(42f / 255f, 42 / 255f, 42 / 255f, 0.75f));

            SetVerticalGridPosition(line, position);

            return line;
        }

        private void SetVerticalGridPosition(LineRenderer line, float position)
        {
            var x = MainSystem.Args.PositionToX(position);
            line.SetPosition(0, new Vector3(x, 0, 0f));
            line.SetPosition(1, new Vector3(x, 0, MainSystem.Args.NoteAppearZ));
        }

        private void AwakeVerticalGrid()
        {
            _verticalGridPool = UnityUtils.CreateObjectPool(_linePrefab, _lineParentTransform);
            _verticalGrids = new();
        }

        public readonly struct VerticalGridEnumerable : IEnumerable<float>
        {
            private readonly int _count;
            private readonly float _offset;

            public VerticalGridEnumerable(GridController gridController)
            {
                _count = gridController._verticalGrids.Count;
                _offset = gridController._verticalGridOffset;
            }

            public Enumerator GetEnumerator() => new(_count, _offset);

            IEnumerator<float> IEnumerable<float>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public struct Enumerator : IEnumerator<float>
            {
                private readonly int _count;
                private readonly float _offset;
                private int _current;

                public Enumerator(int count, float offset)
                {
                    _count = count;
                    _offset = offset;
                    _current = -1;
                }

                public readonly float Current => 2 * MainSystem.Args.StageMaxPosition * ((float)_current / (_count - 1)) - MainSystem.Args.StageMaxPosition + _offset;

                readonly object IEnumerator.Current => Current;

                public readonly void Dispose() { }
                public bool MoveNext()
                {
                    if (++_current < _count) {
                        return true;
                    }
                    return false;
                }
                void IEnumerator.Reset() => _current = -1;
            }
        }
    }
}