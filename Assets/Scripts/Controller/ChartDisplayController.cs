using System.Collections.Generic;
using UnityEngine;

public class ChartDisplayController : MonoBehaviour
{
    public static ChartDisplayController Instance { get; private set; }
    public int Difficulty { get; private set; } = 4;
    private int _playSpeed = 10;

    // Note related
    private Chart _chart;
    public Chart Chart
    {
        get => _chart;
        private set
        {
            _chart = value;
            InitializeComboCount();
            ResetStage();
        }
    }
    private List<int> _combo = new List<int>();
    private int _nextShownNoteIndex;
    public int LastHitNoteIndex { get; private set; } = -1;
    [SerializeField] private Transform _noteParent;
    [SerializeField] private NoteObjectPerspective _notePrefab;
    private ObjectPool<NoteObjectPerspective> _notePool;
    private List<NoteObjectPerspective> _noteObjects = new List<NoteObjectPerspective>();

    // Beat line related
    public List<TempoEvent> Tempos { get; private set; }
    public int TimeGridPartition { get; private set; } = 4;
    public readonly List<TimeGridData> timeGrids = new List<TimeGridData>();
    public void UpdateTimeGrid()
    {
        timeGrids.Clear();
        if (TimeGridPartition == 0) return;
        for (int i = 0; i < Tempos.Count; i++)
        {
            float currentTempo = Tempos[i].tempo;
            float currentTime = Tempos[i].time;
            if (currentTempo <= 0.0f)
            {
                timeGrids.Add(new TimeGridData { time = currentTime, type = TimeGridData.Type.FreeTempo });
                continue;
            }
            float nextTime = i != Tempos.Count - 1 ? Tempos[i + 1].time : AudioPlayer.Instance.Length;
            int counter = 0;
            float increment = 60.0f / currentTempo / TimeGridPartition;
            float time;
            timeGrids.Add(new TimeGridData { time = currentTime, type = TimeGridData.Type.TempoChange });
            do
            {
                counter++;
                time = currentTime + counter * increment;
                timeGrids.Add(new TimeGridData
                {
                    time = time,
                    type = counter % TimeGridPartition == 0 ? TimeGridData.Type.Beat : TimeGridData.Type.SubBeat
                });
            } while (time < nextTime);
        }
    }
    [SerializeField] private Transform _timeGridParent;
    [SerializeField] private TimeGridPerspective _timeGridPrefab;
    private ObjectPool<TimeGridPerspective> _timeGridPool;
    private List<TimeGridPerspective> _timeGridObjects = new List<TimeGridPerspective>();
    private int _nextShownTimeGridIndex;

    // World position and time space position
    public float PerspectiveTime(float time) =>
        Parameters.Params.perspectiveDistancesPerSecond[_playSpeed] * (time - AudioPlayer.Instance.Time);
    public Vector3 PerspectivePosition(float time, float position) =>
        new Vector3(PerspectiveTime(time), 0.0f, position * Parameters.Params.perspectiveHorizontalScale);
    public bool NoteShownInPerspectiveView(float time)
    {
        float position = PerspectiveTime(time);
        if (position <= Parameters.Params.perspectiveMaxDistance && position >= 0.0f) return true;
        return position < 0.0f && AudioPlayer.Instance.Time - time <= Parameters.Params.noteAnimationLength;
    }
    public bool TimeGridShownInPerspectiveView(float time)
    {
        float position = PerspectiveTime(time);
        return position <= Parameters.Params.perspectiveMaxDistance && position >= 0.0f;
    }

    // Chart playing methods
    public void LoadFromProject(int difficulty)
    {
        Difficulty = difficulty;
        _chart = ProjectManagement.project.charts[difficulty];
        InitializeComboCount();
        Tempos = ProjectManagement.project.tempos;
        Tempos.Add(new TempoEvent { tempo = 202.0f, time = 0.593f });
        UpdateTimeGrid();
        ResetStage();
    }
    public void ReloadChart()
    {
        InitializeComboCount();
        ResetStage();
    }
    private void InitializeComboCount()
    {
        _combo.Clear();
        int noteCount = _chart.notes.Count;
        _combo.Capacity = noteCount;
        if (noteCount > 0) _combo.Add(_chart.notes[0].IsShown ? 1 : 0);
        for (int i = 1; i < noteCount; i++) _combo.Add(_combo[i - 1] + (_chart.notes[i].IsShown ? 1 : 0));
    }
    private void ResetStage()
    {
        ClearStage();
        SetStage();
    }
    private void ClearStage()
    {
        // Return all notes
        LastHitNoteIndex = -1;
        for (int i = 0; i < _noteObjects.Count; i++) _notePool.ReturnObject(_noteObjects[i]);
        _noteObjects.Clear();
        List<Note> notes = _chart.notes;
        int noteCount = notes.Count;
        _nextShownNoteIndex = 0;
        while (_nextShownNoteIndex < noteCount &&
            !NoteShownInPerspectiveView(notes[_nextShownNoteIndex].time))
            _nextShownNoteIndex++;
        // Return all beat lines
        for (int i = _timeGridObjects.Count - 1; i >= 0; i--)
        {
            _timeGridPool.ReturnObject(_timeGridObjects[i]);
            _timeGridObjects.RemoveAt(i);
        }
        _nextShownTimeGridIndex = 0;
        int timeGridCount = timeGrids.Count;
        while (_nextShownTimeGridIndex < timeGridCount &&
            !TimeGridShownInPerspectiveView(timeGrids[_nextShownTimeGridIndex].time))
            _nextShownTimeGridIndex++;
        // ToDo: Should add logic for orthogonal view later
    }
    private void SetStage()
    {
        // Place notes
        UpdateLastHitNoteIndex();
        while (_noteObjects.Count > 0 && !_noteObjects[0].IsShown)
        {
            _notePool.ReturnObject(_noteObjects[0]);
            _noteObjects.RemoveAt(0);
        }
        List<Note> notes = _chart.notes;
        int noteCount = notes.Count;
        while (_nextShownNoteIndex < noteCount &&
            NoteShownInPerspectiveView(notes[_nextShownNoteIndex].time))
        {
            if (notes[_nextShownNoteIndex].IsShown)
            {
                NoteObjectPerspective noteObject = _notePool.GetObject();
                noteObject.Id = _nextShownNoteIndex;
                noteObject.gameObject.SetActive(true);
                noteObject.Activate();
                _noteObjects.Add(noteObject);
            }
            _nextShownNoteIndex++;
        }
        // Place beat lines
        while (_timeGridObjects.Count > 0 && !_timeGridObjects[0].IsShown)
        {
            _timeGridPool.ReturnObject(_timeGridObjects[0]);
            _timeGridObjects.RemoveAt(0);
        }
        int timeGridCount = timeGrids.Count;
        while (_nextShownTimeGridIndex < timeGridCount &&
            NoteShownInPerspectiveView(timeGrids[_nextShownTimeGridIndex].time))
        {
            TimeGridPerspective timeGridObject = _timeGridPool.GetObject();
            timeGridObject.Id = _nextShownTimeGridIndex;
            timeGridObject.gameObject.SetActive(true);
            _timeGridObjects.Add(timeGridObject);
            _nextShownTimeGridIndex++;
        }
        // ToDo: Should add logic for orthogonal view later
        // Update effects
        UpdateScore();
        UpdateJudgeLineEffect();
        UpdateCombo();
    }
    private void UpdateLastHitNoteIndex()
    {
        while (LastHitNoteIndex < _chart.notes.Count)
            if (LastHitNoteIndex < 0 || _chart.notes[LastHitNoteIndex].time <= AudioPlayer.Instance.Time)
                LastHitNoteIndex++;
            else
                break;
        LastHitNoteIndex--;
        while (LastHitNoteIndex > 0)
            if (!_chart.notes[LastHitNoteIndex].IsShown)
                LastHitNoteIndex--;
            else
                break;
    }
    private void UpdateScore()
    {
        int totalShownNotes = _combo.Count > 0 ? _combo[_combo.Count - 1] : 0;
        int currentNoteCombo = LastHitNoteIndex > 0 ? _combo[LastHitNoteIndex] : 0;
        if (totalShownNotes <= 1)
            PerspectiveView.Instance.SetScore(0);
        else
            PerspectiveView.Instance.SetScore((int)((800000.0 * currentNoteCombo * (totalShownNotes - 1) +
                200000.0 * currentNoteCombo * (currentNoteCombo - 1)) / (totalShownNotes * (totalShownNotes - 1))));
    }
    private void UpdateJudgeLineEffect() =>
        JudgeLineEffectPerspective.Instance.ChangeScale(LastHitNoteIndex < 0 ? -10.0f : _chart.notes[LastHitNoteIndex].time);
    private void UpdateCombo()
    {
        if (LastHitNoteIndex < 0)
            ComboEffectPerspective.Instance.UpdateCombo(0, 0.0f);
        else
            ComboEffectPerspective.Instance.UpdateCombo(_combo[LastHitNoteIndex], _chart.notes[LastHitNoteIndex].time);
    }

    // Operation related methods
    private float _spaceStartTime;
    public List<Operation> chartPlayingOperations = new List<Operation>();
    private void InitializeChartPlayingOperations()
    {
        chartPlayingOperations.Add(new Operation
        {
            callback = AudioPlayer.Instance.TogglePlayState,
            shortcut = new Shortcut { key = KeyCode.Return }
        }); // Return
        chartPlayingOperations.Add(new Operation
        {
            callback = () =>
            {
                AudioPlayer.Instance.Play();
                _spaceStartTime = AudioPlayer.Instance.Time;
            },
            shortcut = new Shortcut { key = KeyCode.Space }
        }); // Space
        chartPlayingOperations.Add(new Operation
        {
            callback = () =>
            {
                AudioPlayer.Instance.Time = _spaceStartTime;
                AudioPlayer.Instance.Stop();
                TimeSliderPerspective.Instance.OnUserMoveSlider();
            },
            shortcut = new Shortcut { key = KeyCode.Space, state = Shortcut.State.Release }
        }); // Space (Release)
        chartPlayingOperations.Add(new Operation
        {
            callback = () =>
            {
                AudioPlayer.Instance.Time = 0;
                TimeSliderPerspective.Instance.OnUserMoveSlider();
            },
            shortcut = new Shortcut { key = KeyCode.Home }
        }); // Home
        chartPlayingOperations.Add(new Operation
        {
            callback = () =>
            {
                AudioPlayer.Instance.Stop();
                AudioPlayer.Instance.Time = AudioPlayer.Instance.Length - Parameters.Params.epsilonTime;
                TimeSliderPerspective.Instance.OnUserMoveSlider();
            },
            shortcut = new Shortcut { key = KeyCode.End }
        }); // End
        chartPlayingOperations.Add(new Operation
        {
            callback = () =>
            {
                float deltaTime = Time.deltaTime * Parameters.Params.slowScrollSpeed;
                AudioPlayer instance = AudioPlayer.Instance;
                if (instance.Time > deltaTime) instance.Time -= deltaTime;
                TimeSliderPerspective.Instance.OnUserMoveSlider();
            },
            shortcut = new Shortcut { key = KeyCode.UpArrow, state = Shortcut.State.Hold }
        }); // Up (Hold)
        chartPlayingOperations.Add(new Operation
        {
            callback = () =>
            {
                float deltaTime = Time.deltaTime * Parameters.Params.slowScrollSpeed;
                AudioPlayer instance = AudioPlayer.Instance;
                if (instance.Time < instance.Length - deltaTime) instance.Time += deltaTime;
                TimeSliderPerspective.Instance.OnUserMoveSlider();
            },
            shortcut = new Shortcut { key = KeyCode.DownArrow, state = Shortcut.State.Hold }
        }); // Down (Hold)
        chartPlayingOperations.Add(new Operation
        {
            callback = () =>
            {
                float deltaTime = Time.deltaTime * Parameters.Params.fastScrollSpeed;
                AudioPlayer instance = AudioPlayer.Instance;
                if (instance.Time > deltaTime) instance.Time -= deltaTime;
                TimeSliderPerspective.Instance.OnUserMoveSlider();
            },
            shortcut = new Shortcut { key = KeyCode.UpArrow, shift = true, state = Shortcut.State.Hold }
        }); // Shift + Up (Hold)
        chartPlayingOperations.Add(new Operation
        {
            callback = () =>
            {
                float deltaTime = Time.deltaTime * Parameters.Params.fastScrollSpeed;
                AudioPlayer instance = AudioPlayer.Instance;
                if (instance.Time < instance.Length - deltaTime) instance.Time += deltaTime;
                TimeSliderPerspective.Instance.OnUserMoveSlider();
            },
            shortcut = new Shortcut { key = KeyCode.DownArrow, shift = true, state = Shortcut.State.Hold }
        }); // Shift + Down (Hold)
        chartPlayingOperations.Add(new Operation
        {
            callback = () => { ChangePlaySpeed(1); },
            shortcut = new Shortcut { key = KeyCode.UpArrow, ctrl = true }
        }); // Ctrl + Up
        chartPlayingOperations.Add(new Operation
        {
            callback = () => { ChangePlaySpeed(-1); },
            shortcut = new Shortcut { key = KeyCode.DownArrow, ctrl = true }
        }); // Ctrl + Down
    }
    public void ChangePlaySpeed(int amount)
    {
        int newSpeed = _playSpeed + amount;
        if (newSpeed < 1 || newSpeed > 19) return; // Out of range
        _playSpeed = newSpeed;
        // ToDo: Change the shown text for _playSpeed
        ResetStage();
    }

    // Unity events
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ChartDisplayController");
        }
        InitializeChartPlayingOperations();
    }
    private void Start()
    {
        _notePool = new ObjectPool<NoteObjectPerspective>(_notePrefab, 20, _noteParent, null, null, (note) =>
        {
            note.gameObject.SetActive(false);
            note.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
        });
        _timeGridPool = new ObjectPool<TimeGridPerspective>(_timeGridPrefab, 10, _timeGridParent, null, null, (timeGrid) =>
        {
            timeGrid.gameObject.SetActive(false);
            timeGrid.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
        });
        TimeSliderPerspective.Instance.UserMoveSliderEvent += ResetStage;
    }
    private void Update()
    {
        if (_chart != null) SetStage();
    }
}
