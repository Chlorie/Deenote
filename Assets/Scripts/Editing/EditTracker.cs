using System;
using System.Collections.Generic;
using UnityEngine;

public class EditTracker : MonoBehaviour
{
    public static EditTracker Instance { get; private set; }
    public class EditOperation
    {
        public Action redo;
        public Action undo;
    }
    private List<EditOperation> _history = new List<EditOperation>();
    private int _currentStep = 0;
    private int _maxStep = 100;
    public int MaxStep
    {
        get { return _maxStep; }
        set
        {
            if (value <= 0) return;
            _maxStep = value;
            while (_history.Count > _maxStep) _history.RemoveAt(0);
        }
    }
    public void RegisterStep(EditOperation step)
    {
        while (_history.Count > _currentStep) _history.RemoveAt(_currentStep);
        _history.Add(step);
        _currentStep++;
        if (_history.Count <= _maxStep) return;
        _history.RemoveAt(0);
        _currentStep--;
    }
    public void Undo()
    {
        if (_currentStep <= 0) return;
        _currentStep--;
        _history[_currentStep].undo?.Invoke();
    }
    public void Redo()
    {
        if (_currentStep >= _history.Count) return;
        _history[_currentStep].redo?.Invoke();
        _currentStep++;
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of EditTracker");
        }
    }
}
