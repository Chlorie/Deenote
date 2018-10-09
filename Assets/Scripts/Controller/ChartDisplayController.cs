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
    private float _spaceStartTime;
    public int LastHitNoteIndex { get; private set; } = -1;
    [SerializeField] private Transform _noteParent;
    [SerializeField] private NoteObject _notePrefab;
    private ObjectPool<NoteObject> _notePool;
    private List<NoteObject> _noteObjects = new List<NoteObject>();
    public List<Operation> chartPlayingOperations = new List<Operation>();
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

    // Chart playing methods
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
        LastHitNoteIndex = -1;
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
        // ToDo: Should add logic for returning beat lines later
    }
    private void SetStage()
    {
        UpdateLastHitNoteIndex();
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
        // ToDo: Should add logic for placing beat lines later
        // Update effects
        UpdateScore();
        UpdateJudgeLineEffect();
        UpdateCombo();
    }
    private void UpdateLastHitNoteIndex()
    {
        while (LastHitNoteIndex < Chart.notes.Count)
            if (LastHitNoteIndex < 0 || Chart.notes[LastHitNoteIndex].time <= AudioPlayer.Instance.Time)
                LastHitNoteIndex++;
            else
                break;
        LastHitNoteIndex--;
        while (LastHitNoteIndex > 0)
            if (!Chart.notes[LastHitNoteIndex].IsShown)
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
        JudgeLineEffectPerspective.Instance.ChangeScale(LastHitNoteIndex < 0 ? -10.0f : Chart.notes[LastHitNoteIndex].time);
    private void UpdateCombo()
    {
        if (LastHitNoteIndex < 0)
            ComboEffectPerspective.Instance.UpdateCombo(0, 0.0f);
        else
            ComboEffectPerspective.Instance.UpdateCombo(_combo[LastHitNoteIndex], Chart.notes[LastHitNoteIndex].time);
    }

    // Operation related methods
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
            shortcut = new Shortcut { key = KeyCode.UpArrow, alt = true }
        }); // Alt + Up
        chartPlayingOperations.Add(new Operation
        {
            callback = () => { ChangePlaySpeed(-1); },
            shortcut = new Shortcut { key = KeyCode.DownArrow, alt = true }
        }); // Alt + Down
    }
    public void ChangePlaySpeed(int amount)
    {
        if (amount != 1 && amount != -1) throw new System.ArgumentOutOfRangeException(nameof(amount));
        int newSpeed = _playSpeed + amount;
        if (newSpeed < 1 || newSpeed > 19) return; // Out of range
        _playSpeed = newSpeed;
        // ToDo: Change the shown text for _playSpeed
        ResetStage();
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
        InitializeChartPlayingOperations();
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
