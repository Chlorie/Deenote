using System.Collections.Generic;
using UnityEngine;

public class ChartDisplayController : MonoBehaviour
{
    public static ChartDisplayController Instance { get; private set; }
    public int Difficulty { get; private set; } = 4;
    public Chart Chart { get; private set; }
    private List<int> _combo = new List<int>();
    private int _playSpeed = 10;
    private int _nextShownNoteIndex;
    [SerializeField] private Transform _noteParent;
    [SerializeField] private NoteObject _notePrefab;
    private ObjectPool<NoteObject> _notePool;
    private List<NoteObject> _noteObjects = new List<NoteObject>();
    public float PerspectivePosition(float time) =>
        Parameters.Params.perspectiveDistancesPerSecond[_playSpeed] * (time - AudioPlayer.Instance.Time);
    public bool ShownInPerspectiveView(Note note)
    {
        float time = note.time;
        float position = PerspectivePosition(time);
        if (position <= Parameters.Params.perspectiveMaxDistance && position >= 0.0f) return true;
        return position < 0.0f && AudioPlayer.Instance.Time - time <= Parameters.Params.noteAnimationLength;
    }
    public void LoadChartFromProject(int difficulty)
    {
        Difficulty = difficulty;
        Chart = ProjectManagement.project.charts[difficulty];
        InitializeComboCount();
        ResetStage();
    }
    private void InitializeComboCount()
    {
        _combo.Clear();
        int noteCount = Chart.notes.Count;
        _combo.Capacity = noteCount;
        if (noteCount > 0) _combo.Add(Chart.notes[0].IsShown ? 1 : 0);
        for (int i = 1; i < noteCount; i++) _combo.Add(_combo[i - 1] + (Chart.notes[i].IsShown ? 1 : 0));
    }
    private void ResetStage()
    {
        ClearStage();
        SetStage();
    }
    private void ClearStage()
    {
        // Return all notes
        for (int i = _noteObjects.Count - 1; i >= 0; i--)
        {
            _notePool.ReturnObject(_noteObjects[i]);
            _noteObjects.RemoveAt(i);
        }
        List<Note> notes = Chart.notes;
        int noteCount = notes.Count;
        _nextShownNoteIndex = 0;
        while (_nextShownNoteIndex < noteCount && !ShownInPerspectiveView(notes[_nextShownNoteIndex])) _nextShownNoteIndex++;
        // Return all beat lines
#warning Should add logic for returning beat lines later
    }
    private void SetStage()
    {
        // Place notes
        while (_noteObjects.Count > 0 && !_noteObjects[0].IsShown)
        {
            _notePool.ReturnObject(_noteObjects[0]);
            _noteObjects.RemoveAt(0);
        }
        List<Note> notes = Chart.notes;
        int noteCount = notes.Count;
        while (_nextShownNoteIndex < noteCount && ShownInPerspectiveView(notes[_nextShownNoteIndex]))
        {
            if (notes[_nextShownNoteIndex].IsShown)
            {
                NoteObject noteObject = _notePool.GetObject();
                noteObject.Id = _nextShownNoteIndex;
                noteObject.gameObject.SetActive(true);
                noteObject.Activate();
                _noteObjects.Add(noteObject);
            }
            _nextShownNoteIndex++;
        }
        // Place beat lines
#warning Should add logic for placing beat lines later
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
    private void Start()
    {
        _notePool = new ObjectPool<NoteObject>(_notePrefab, 20, _noteParent, null, null, (note) =>
        {
            note.gameObject.SetActive(false);
            note.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
        });
        TimeSliderPerspective.Instance.UserMoveSliderEvent += ResetStage;
    }
    private void Update()
    {
        if (Chart != null) SetStage();
    }
}
