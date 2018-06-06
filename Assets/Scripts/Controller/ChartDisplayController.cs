using System.Collections.Generic;
using UnityEngine;

public class ChartDisplayController : MonoBehaviour
{
    public static ChartDisplayController Instance { get; private set; }
    private int _difficulty = 4;
    private Chart _chart = null;
    private List<int> _combo = new List<int>();
    public int _firstNoteIndex = 0;
    public int _lastNoteIndex = 0;
    public void LoadChartFromProject(int difficulty)
    {
        _difficulty = difficulty;
        _chart = ProjectManagement.project.charts[difficulty];
        InitializeComboCount();
    }
    private void InitializeComboCount()
    {
        _combo.Clear();
        int noteCount = _chart.notes.Count;
        _combo.Capacity = noteCount;
        if (noteCount > 0) _combo.Add(_chart.notes[0].IsShown ? 1 : 0);
        for (int i = 1; i < noteCount; i++) _combo.Add(_combo[i - 1] + (_chart.notes[i].IsShown ? 1 : 0));
    }
    private void SetStage()
    {

    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ChartDisplayController");
        }
    }
    private void Update()
    {
        
    }
}
