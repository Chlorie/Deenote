using System;
using System.Collections.Generic;
using UnityEngine;

public class EditTracker : MonoBehaviour
{
    public static EditTracker Instance { get; private set; }
    public struct EditOperation
    {
        public Action undo;
        public Action redo;
    }
    public bool Edited { get; set; }
    private List<EditOperation> _history = new List<EditOperation>();
    private int _currentStep;
    private int _maxStep = 100;
    public int MaxStep
    {
        get => _maxStep;
        set
        {
            if (value <= 0) return;
            _maxStep = value;
            while (_history.Count > _maxStep) _history.RemoveAt(0);
        }
    }
    public void AddStep(EditOperation step)
    {
        ToolbarInitialization.Instance.editSelectable.SetActive(OperationName.Undo, true);
        ToolbarInitialization.Instance.editSelectable.SetActive(OperationName.Redo, false);
        Edited = true;
        while (_history.Count > _currentStep) _history.RemoveAt(_currentStep);
        _history.Add(step);
        _currentStep++;
        if (_history.Count <= _maxStep) return;
        _history.RemoveAt(0);
        _currentStep--;
    }
    public void Undo()
    {
        _currentStep--;
        _history[_currentStep].undo?.Invoke();
        ToolbarInitialization.Instance.editSelectable.SetActive(OperationName.Redo, true);
        if (_currentStep <= 0) ToolbarInitialization.Instance.editSelectable.SetActive(OperationName.Undo, false);
        Edited = true;
    }
    public void Redo()
    {
        _history[_currentStep].redo?.Invoke();
        _currentStep++;
        ToolbarInitialization.Instance.editSelectable.SetActive(OperationName.Undo, true);
        if (_currentStep >= _history.Count) ToolbarInitialization.Instance.editSelectable.SetActive(OperationName.Redo, false);
        Edited = true;
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
