using Deenote.Edit.Operations;
using System.Collections.Generic;
using UnityEngine;

namespace Deenote.Edit
{
    public sealed class UndoableOperationHistory
    {
        private readonly int _maxCount;
        private readonly List<IUndoableOperation> _operations;
        private int _startIndex;
        private int _currentOffset;
        private int _count;

        public UndoableOperationHistory(int maxCount)
        {
            _maxCount = maxCount;
            _operations = new();
            _startIndex = 0;
            _count = 0;
        }

        public void Do(IUndoableOperation operation)
        {
            // Add and ignore previous undone operations
            if (_currentOffset < _count) {
                _operations[ActualIndex(_currentOffset)] = operation;
                _currentOffset++;
                _count = _currentOffset;
            }
            // Not full
            else if (_count < _maxCount) {
                if (_operations.Count >= _maxCount) {
                    _operations[ActualIndex(_currentOffset)] = operation;
                }
                else {
                    _operations.Add(operation);
                }
                _currentOffset++;
                _count = _currentOffset;
            }
            // History full
            else {
                _operations[_startIndex] = operation;
                IncrementIndex(ref _startIndex);
            }

            operation.Redo();
        }

        public void Redo()
        {
            if (_currentOffset == _count)
                return;

            _operations[ActualIndex(_currentOffset)].Redo();
            _currentOffset++;
        }

        public void Undo()
        {
            if (_currentOffset == 0)
                return;

            _operations[ActualIndex(_currentOffset - 1)].Undo();
            _currentOffset--;
        }

        private void IncrementIndex(ref int value, int add = 1)
        {
            value = (value + add) % _maxCount;
        }

        private int ActualIndex(int index)
        {
            var actualIndex = _startIndex;
            IncrementIndex(ref actualIndex, index);
            return actualIndex;
        }
    }
}