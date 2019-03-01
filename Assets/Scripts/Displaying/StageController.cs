using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageController : MonoBehaviour
{
    //-Stage activation-
    public bool stageActivated;
    //-Sounds-
    public AudioSource musicSource;
    public AudioSource clickSoundSource;
    //-Stage lights-
    public Light stageLight;
    public Toggle lightEffectToggle;
    public bool lightEffectState = true;
    //-Note controlling-
    public Chart chart;
    public int diff;
    private List<NoteController> notes = new List<NoteController>();
    public List<bool> collided = new List<bool>();
    public NoteController notePrefab;
    public ObjectPool<NoteController> notePool;
    public int prevNoteID;
    public int returnNoteID;
    public Transform noteParentTransform;
    public bool forceToPlaceNotes;
    private float antiZureTime;
    //-T grid controlling-
    public TGridId prevLineID;
    private List<TGridController> tGrids = new List<TGridController>();
    public TGridObjectPool tGridPool;
    //-Combo and score effect-
    public List<int> inGameNoteIDs = new List<int>();
    public Text scoreText;
    public GameObject comboParent;
    public ComboEffectController comboController;
    //-Editor settings-
    public int musicPlaySpeed = 10; //Actually it is "pitch" value...
    public int chartPlaySpeed = 10; //Twice the speed of that in the game
    public int musicVolume = 100;
    public int effectVolume = 100;
    public int pianoVolume = 100;
    public int mouseSens = 10;
    public InputField musicVolInputField;
    public InputField effectVolInputField;
    public InputField pianoVolInputField;
    public InputField mouseSensInputField;
    public Slider musicVolSlider;
    public Slider effectVolSlider;
    public Slider pianoVolSlider;
    public Button noteSpeedLeftButton;
    public Button noteSpeedRightButton;
    public Text noteSpeedIntText;
    public Text noteSpeedDeciText;
    public GameObject noteSpeedChangeSetting;
    public Button musicSpeedLeftButton;
    public Button musicSpeedRightButton;
    public Text musicSpeedIntText;
    public Text musicSpeedDeciText;
    public GameObject musicSpeedChangeSetting;
    public EditorController editor;
    //-FPS calculating-
    private float timeCount, frameCount;
    private float fps;
    public bool showFPS = true;
    public Toggle fpsToggle;
    //-Other things-
    public Camera stageCamera;
    public Slider timeSlider;
    public Text fpsText;
    public GameObject emptyImage;
    public RectTransform cameraUICanvas;
    public RectTransform xGridParent;
    public RectTransform linkLineParent;
    public ProjectController projectController;
    public Toggle linkLineToggle;
    public LinePool linePool;
    public Transform lineCanvas;
    //-About sounds playing-
    public PianoSoundsLoader pianoSoundsLoader;
    public float musicLength; // In seconds
    private bool musicPlayState;
    private bool musicEnds;
    public void ClearStage()
    {
        while (notes.Count > 0) notes[0].ForceReturn();
        while (tGrids.Count > 0) tGrids[0].ForceReturn();
        prevNoteID = -1;
        returnNoteID = -1;
        prevLineID = new TGridId(0, -1, editor.tGrid);
    }
    public void ToggleLightEffect()
    {
        lightEffectState = lightEffectToggle.isOn;
        if (!lightEffectState) stageLight.intensity = 2.5f;
    }
    public void ToggleMusicPlayState()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            musicSource.time = timeSlider.value;
        }
        else
        {
            if (musicEnds)
            {
                musicSource.time = timeSlider.value = 0.0f;
                musicSource.Play();
                OnSliderValueChanged();
                musicEnds = false;
            }
            else
            {
                musicSource.Play();
                musicSource.time = timeSlider.value;
            }
        }
        musicPlayState = !musicPlayState;
    }
    private void MusicStopped()
    {
        StopPlaying();
        musicSource.time = timeSlider.value = musicLength;
        musicEnds = true;
    }
    public void StopPlaying()
    {
        musicSource.Stop();
        musicSource.time = timeSlider.value;
        musicPlayState = false;
    }
    public void InitializeStage(Project initProj, int difficulty, ProjectController projCtrl)
    {
        projectController = projCtrl;
        tGridPool.Initialize();
        comboParent.SetActive(true);
        comboController.stage = this;
        noteSpeedChangeSetting.SetActive(true);
        musicSpeedChangeSetting.SetActive(true);
        stageActivated = true;
        chart = initProj.charts[difficulty];
        diff = difficulty;
        editor.chart = chart;
        editor.ActivateEditor();
        ChartProperties.GetInGameNoteIds(initProj.charts[difficulty], out inGameNoteIDs);
        ChartProperties.GetCollidedNotes(initProj.charts[difficulty], out collided);
        musicLength = musicSource.clip.length;
        timeSlider.minValue = 0.0f;
        timeSlider.maxValue = musicLength;
        timeSlider.value = 0.0f;
        noteSpeedIntText.text = (chartPlaySpeed / 2).ToString();
        noteSpeedDeciText.text = (chartPlaySpeed % 2 * 5).ToString();
        musicSpeedIntText.text = (musicPlaySpeed / 10).ToString();
        musicSpeedDeciText.text = (musicPlaySpeed % 10).ToString();
        OnSliderValueChanged();
    }
    public void OnSliderValueChanged()
    {
        musicSource.time = timeSlider.value;
        musicEnds = false;
        ResetStage();
    }
    private void InitNoteObject(int noteId)
    {
        NoteController note = notePool.GetObject();
        notes.Add(note);
        note.gameObject.SetActive(true);
        note.gameObject.transform.SetParent(noteParentTransform);
        note.Activate(noteId, chart.notes[noteId], this, pianoSoundsLoader);
    }
    private void InitTGridObject(TGridId id)
    {
        TGridController tGrid = tGridPool.GetObject();
        tGrids.Add(tGrid);
        tGrid.grid.SetActive(true);
        tGrid.Activate(id, TGridTime(id), this);
    }
    private float TGridTime(TGridId id)
    {
        float curLineTime = chart.beats[id.id];
        if (id.sub != 0)
        {
            float dTime = chart.beats[id.id + 1] - curLineTime;
            curLineTime += dTime / editor.tGrid * id.sub;
        }
        return curLineTime;
    }
    public void ReturnNote(NoteController note)
    {
        GameObject noteObject = note.gameObject;
        noteObject.SetActive(false);
        noteObject.transform.localPosition = Vector3.zero;
        if (returnNoteID < note.id) returnNoteID = note.id;
        notePool.ReturnObject(note);
        notes.Remove(note);
    }
    public void ReturnLine(TGridController tGrid)
    {
        tGrid.grid.SetActive(false);
        SetPrevLineId(tGrid.id);
        tGridPool.ReturnObject(tGrid);
        tGrids.Remove(tGrid);
    }
    public void SetPrevNoteId(int id)
    {
        if (id > prevNoteID) prevNoteID = id;
    }
    public void SetPrevLineId(TGridId id)
    {
        if (id > prevLineID) prevLineID = id;
    }
    private void PlaceNewObjects()
    {
        {
            int cur;
            int prev = notes.Count > 0 ? notes[notes.Count - 1].id : returnNoteID;
            for (cur = prev + 1; cur < chart.notes.Count; cur++)
                if (chart.notes[cur].time > timeSlider.value + Parameters.NoteFallTime(chartPlaySpeed))
                    break;
            for (int i = prev + 1; i < cur; i++) InitNoteObject(i);
        }
        {
            TGridId prev = tGrids.Count > 0 ? tGrids[tGrids.Count - 1].id : prevLineID;
            TGridId cur = prev; cur++;
            for (; cur <= new TGridId(chart.beats.Count - 1, 0, editor.tGrid); cur++)
                if (TGridTime(cur) > timeSlider.value + Parameters.NoteFallTime(chartPlaySpeed))
                    break;
            TGridId i = prev; i++;
            for (; i < cur; i++) InitTGridObject(i);
        }
        ChangeScoreText();
    }
    public void ResetStage()
    {
        ClearStage();
        for (prevNoteID = 0; prevNoteID < chart.notes.Count; prevNoteID++)
            if (chart.notes[prevNoteID].time > timeSlider.value)
                break;
        prevNoteID--;
        for (returnNoteID = prevNoteID; returnNoteID >= 0; returnNoteID--)
            if (chart.notes[returnNoteID].time + Parameters.noteReturnTime <= timeSlider.value)
                break;
        for (prevLineID = new TGridId(0, 0, editor.tGrid);
            prevLineID <= new TGridId(chart.beats.Count - 1, 0, editor.tGrid); prevLineID++)
            if (TGridTime(prevLineID) > timeSlider.value)
                break;
        prevLineID--;
        PlaceNewObjects();
    }
    private void ChangeScoreText()
    {
        if (chart.notes.Count <= 1) return;
        int ttl = inGameNoteIDs[chart.notes.Count - 1] + 1, cur = 0;
        if (prevNoteID > 0) cur = inGameNoteIDs[prevNoteID] + 1;
        int score = (int)((800000.0 * cur * (ttl - 1) + 200000.0 * cur * (cur - 1)) / (ttl * (ttl - 1)));
        int intPart = score / 10000, decPart = (score / 100) % 100;
        string str = "" + intPart;
        if (decPart >= 10) str += "." + decPart + " %"; else str += ".0" + decPart + " %";
        scoreText.text = str;
    }
    public void NoteSpeedChange(bool increase)
    {
        if (increase) chartPlaySpeed++; else chartPlaySpeed--;
        noteSpeedLeftButton.interactable = chartPlaySpeed != 1;
        noteSpeedRightButton.interactable = chartPlaySpeed != 19;
        noteSpeedIntText.text = "" + chartPlaySpeed / 2;
        noteSpeedDeciText.text = "" + chartPlaySpeed % 2 * 5;
        ResetStage();
    }
    public void MusicSpeedChange(bool increase)
    {
        if (increase) musicPlaySpeed++; else musicPlaySpeed--;
        musicSpeedLeftButton.interactable = musicPlaySpeed != 1;
        musicSpeedRightButton.interactable = musicPlaySpeed != 30;
        musicSpeedIntText.text = "" + musicPlaySpeed / 10;
        musicSpeedDeciText.text = "" + musicPlaySpeed % 10;
        musicSource.pitch = musicPlaySpeed / 10.0f;
    }
    public void MusicVolSliderChange()
    {
        float value = musicVolSlider.value;
        musicVolume = (int)value;
        musicSource.volume = musicVolume / 100.0f;
        musicVolInputField.text = "" + (int)value;
    }
    public void EffectVolSliderChange()
    {
        float value = effectVolSlider.value;
        effectVolume = (int)value;
        effectVolInputField.text = "" + (int)value;
    }
    public void PianoVolSliderChange()
    {
        float value = pianoVolSlider.value;
        pianoVolume = (int)value;
        pianoVolInputField.text = "" + (int)value;
    }
    public void MusicVolInput()
    {
        int vol = Utility.GetInt(musicVolInputField.text);
        if (vol > 100 || vol < 0) { musicVolInputField.text = "" + musicVolume; return; }
        musicVolInputField.text = "" + vol;
        musicVolSlider.value = vol;
        musicVolume = vol;
        musicSource.volume = musicVolume / 100.0f;
    }
    public void EffectVolInput()
    {
        int vol = Utility.GetInt(effectVolInputField.text);
        if (vol > 100 || vol < 0) { effectVolInputField.text = "" + effectVolume; return; }
        effectVolInputField.text = "" + vol;
        effectVolSlider.value = vol;
        effectVolume = vol;
    }
    public void PianoVolInput()
    {
        int vol = Utility.GetInt(pianoVolInputField.text);
        if (vol > 100 || vol < 0) { pianoVolInputField.text = "" + pianoVolume; return; }
        pianoVolInputField.text = "" + vol;
        pianoVolSlider.value = vol;
        pianoVolume = vol;
    }
    public void MouseSensInput()
    {
        int mSens = Utility.GetInt(mouseSensInputField.text);
        if (mSens >= 100 || mSens <= 0) { mouseSensInputField.text = "" + mouseSens; return; }
        mouseSensInputField.text = "" + mSens;
        mouseSens = mSens;
    }
    public void ToggleFPS()
    {
        showFPS = fpsToggle.isOn;
        fpsText.gameObject.SetActive(showFPS);
    }
    public void ToggleLinkLine(bool state) => Utility.linkLineParent.gameObject.SetActive(state);
    private void OnApplicationFocus(bool focus)
    {
        if (musicPlayState) ToggleMusicPlayState();
    }
    private void Shortcuts()
    {
        if (!stageActivated || CurrentState.ignoreAllInput) return;
        if (Utility.DetectKeys(KeyCode.Return)) //Enter
            ToggleMusicPlayState();
        if (Utility.DetectKeys(KeyCode.Space)) //Space
        {
            float value = timeSlider.value;
            antiZureTime = value >= musicLength ? 0.0f : value;
            if (!musicPlayState) ToggleMusicPlayState();
        }
        if (Utility.ReleaseKeys(KeyCode.Space)) //Space(Release)
        {
            timeSlider.value = antiZureTime;
            StopPlaying();
            ResetStage();
        }
        if (Utility.DetectKeys(KeyCode.Home))
        {
            timeSlider.value = 0.0f;
            OnSliderValueChanged();
        }
        if (Utility.DetectKeys(KeyCode.End))
        {
            timeSlider.value = musicLength - 0.001f;
            OnSliderValueChanged();
        }
        if (Utility.DetectKeys(KeyCode.UpArrow, Utility.CTRL)) //Ctrl+Up
            if (noteSpeedRightButton.IsInteractable())
                NoteSpeedChange(true);
        if (Utility.DetectKeys(KeyCode.DownArrow, Utility.CTRL)) //Ctrl+Down
            if (noteSpeedLeftButton.IsInteractable())
                NoteSpeedChange(false);
        if (Utility.DetectKeys(KeyCode.UpArrow, Utility.ALT)) //Alt+Up
            if (musicSpeedRightButton.IsInteractable())
                MusicSpeedChange(true);
        if (Utility.DetectKeys(KeyCode.DownArrow, Utility.ALT)) //Alt+Down
            if (musicSpeedLeftButton.IsInteractable())
                MusicSpeedChange(false);
        if (Utility.HeldKeys(KeyCode.UpArrow)) //Up(Hold)
            if (timeSlider.value > Time.deltaTime * Parameters.slowScrollSpeed)
            { timeSlider.value -= Time.deltaTime * Parameters.slowScrollSpeed; OnSliderValueChanged(); }
        if (Utility.HeldKeys(KeyCode.DownArrow)) //Down(Hold)
            if (timeSlider.value < musicLength - Time.deltaTime * Parameters.slowScrollSpeed)
            { timeSlider.value += Time.deltaTime * Parameters.slowScrollSpeed; OnSliderValueChanged(); }
        if (Utility.HeldKeys(KeyCode.UpArrow, Utility.SHIFT)) //Shift+Up(Hold)
            if (timeSlider.value > Time.deltaTime * Parameters.fastScrollSpeed)
            { timeSlider.value -= Time.deltaTime * Parameters.fastScrollSpeed; OnSliderValueChanged(); }
        if (Utility.HeldKeys(KeyCode.DownArrow, Utility.SHIFT)) //Shift+Down(Hold)
            if (timeSlider.value < musicLength - Time.deltaTime * Parameters.fastScrollSpeed)
            { timeSlider.value += Time.deltaTime * Parameters.fastScrollSpeed; OnSliderValueChanged(); }
        float mWheel = Input.GetAxis("Mouse ScrollWheel");
        float difTime = mWheel * mouseSens * 0.1f;
        if (difTime == 0 || Input.mousePosition.x >= Utility.stageWidth || CurrentState.ignoreScroll) return;
        timeSlider.value = Mathf.Clamp(timeSlider.value - difTime, 0, musicLength);
        OnSliderValueChanged();
    }
    private void LoadPlayerPrefs()
    {
        lightEffectToggle.isOn = Utility.PlayerPrefsGetBool("Light Effect", lightEffectState);
        ToggleLightEffect();
        fpsToggle.isOn = Utility.PlayerPrefsGetBool("Show FPS", showFPS);
        ToggleFPS();
        mouseSensInputField.text = PlayerPrefs.GetInt("Mouse Wheel Sensitivity", mouseSens).ToString();
        MouseSensInput();
        chartPlaySpeed = PlayerPrefs.GetInt("Note Speed", chartPlaySpeed) - 1;
        NoteSpeedChange(true);
        musicPlaySpeed = PlayerPrefs.GetInt("Music Speed", musicPlaySpeed) - 1;
        MusicSpeedChange(true);
        effectVolInputField.text = PlayerPrefs.GetInt("Effect Volume", effectVolume).ToString();
        EffectVolInput();
        musicVolInputField.text = PlayerPrefs.GetInt("Music Volume", musicVolume).ToString();
        MusicVolInput();
        pianoVolInputField.text = PlayerPrefs.GetInt("Piano Volume", pianoVolume).ToString();
        PianoVolInput();
        linkLineToggle.isOn = Utility.PlayerPrefsGetBool("Show Link Line", linkLineParent.gameObject.activeSelf);
        ToggleLinkLine(linkLineToggle.isOn);
    }
    private void Start()
    {
        // Pool initialization
        notePool = new ObjectPool<NoteController>(notePrefab, null, 20, note => note.stage = this);
        // Utility changes
        Utility.stageCamera = stageCamera;
        Utility.emptyImage = emptyImage;
        Utility.cameraUICanvas = cameraUICanvas;
        Utility.xGridParent = xGridParent;
        Utility.linkLineParent = linkLineParent;
        Utility.linePool = linePool;
        Utility.lineCanvas = lineCanvas;
        linePool.Initialize();
        // Load player prefs
        LoadPlayerPrefs();
        // Draw border
        Line line = Utility.DrawLineInWorldSpace
        (
            new Vector3(-15.0f, 0.0f, 32.0f + Parameters.maximumNoteRange),
            new Vector3(-15.0f, 0.0f, 32.0f),
            new Color(42.0f / 255, 42.0f / 255, 42.0f / 255),
            0.06f
        );
        line.transform.SetParent(editor.border.transform);
        line = Utility.DrawLineInWorldSpace
        (
            new Vector3(15.0f, 0.0f, 32.0f + Parameters.maximumNoteRange),
            new Vector3(15.0f, 0.0f, 32.0f),
            new Color(42.0f / 255, 42.0f / 255, 42.0f / 255),
            0.06f
        );
        line.transform.SetParent(editor.border.transform);
        editor.Initialize();
    }
    private void Update()
    {
        float currentTime = Time.time;
        if (showFPS)
        {
            timeCount += Time.deltaTime; frameCount++;
            if (timeCount > 1.0f)
            {
                timeCount -= 1.0f;
                fps = frameCount;
                frameCount = 0;
                fpsText.color = stageActivated ? Color.white : Color.black;
                fpsText.text = "FPS: " + fps;
            }
        }
        if (lightEffectState) stageLight.intensity = 2.5f + 2.5f * Mathf.Sin(2 * currentTime);
        if (musicSource.time >= musicLength && musicPlayState) MusicStopped();
        if (musicPlayState) timeSlider.value = musicSource.time;
        if (musicPlayState && !musicSource.isPlaying) musicPlayState = false;
        if (musicPlayState)
        {
            if (forceToPlaceNotes)
                ResetStage();
            else
                PlaceNewObjects();
        }
        Shortcuts();
    }
}
