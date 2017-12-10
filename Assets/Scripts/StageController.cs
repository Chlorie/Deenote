using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StageController : MonoBehaviour
{
    //-Stage activation-
    public bool stageActivated = false;
    //-Sounds-
    public AudioSource musicSource;
    public AudioSource clickSoundSource;
    public AudioClip linkSound;
    public AudioClip noteSound;
    //-Stage lights-
    public Light stageLight;
    public Toggle lightEffectToggle;
    public bool lightEffectState = true;
    //-Note controlling-
    public Chart chart;
    public int diff;
    private List<NoteController> notes = new List<NoteController>();
    public NoteObjectPool notePool;
    public int prevNoteID;
    public int returnNoteID;
    public Transform noteParentTransform;
    public bool forceToPlaceNotes = false;
    private float antiZureTime = 0.0f;
    //-T grid controlling-
    public TGridID prevLineID;
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
    private int musicVolume = 100;
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
    private float timeCount = 0.0f, frameCount = 0;
    private float fps = 0.0f;
    private bool showFPS = false;
    public Toggle fpsToggle;
    //-Other things-
    public Camera stageCamera;
    public Slider timeSlider;
    public Text fpsText;
    public bool ignoreAllInput = false;
    public GameObject emptyImage;
    public Sprite cylinder;
    public Sprite cylinderAlpha;
    public RectTransform cameraUICanvas;
    public RectTransform xGridParent;
    public RectTransform linkLineParent;
    public Collider mouseDetector;
    public ProjectController projectController;
    //-About sounds playing-
    public PianoSoundsLoader pianoSoundsLoader;
    public float musicLength = 0.0f; //In seconds
    private bool musicPlayState = false;
    private bool musicEnds = false;
    public void ClearStage()
    {
        while (notes.Count > 0) notes[0].ForceReturn();
        while (tGrids.Count > 0) tGrids[0].ForceReturn();
        prevNoteID = -1;
        returnNoteID = -1;
        prevLineID = new TGridID(0, -1, editor.tGrid);
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
        notePool.Initialize();
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
        Utility.GetInGameNoteIDs(initProj.charts[difficulty], ref inGameNoteIDs);
        musicLength = musicSource.clip.length;
        timeSlider.minValue = 0.0f;
        timeSlider.maxValue = musicLength;
        timeSlider.value = 0.0f;
        noteSpeedIntText.text = "" + chartPlaySpeed / 2;
        noteSpeedDeciText.text = "" + chartPlaySpeed % 2 * 5;
        musicSpeedIntText.text = "" + musicPlaySpeed / 10;
        musicSpeedDeciText.text = "" + musicPlaySpeed % 10;
        OnSliderValueChanged();
    }
    public void OnSliderValueChanged()
    {
        musicSource.time = timeSlider.value;
        musicEnds = false;
        ResetStage();
    }
    private void InitNoteObject(int noteID)
    {
        NoteController note;
        note = notePool.GetObject();
        notes.Add(note);
        note.gameObject.SetActive(true);
        note.gameObject.transform.SetParent(noteParentTransform);
        note.Activate(noteID, chart.notes[noteID], this, pianoSoundsLoader);
    }
    private TGridController InitTGridObject(TGridID id)
    {
        TGridController tGrid;
        tGrid = tGridPool.GetObject();
        tGrids.Add(tGrid);
        tGrid.grid.SetActive(true);
        tGrid.Activate(id, TGridTime(id), this);
        return tGrid;
    }
    private float TGridTime(TGridID id)
    {
        float curLineTime, dTime = 0.0f;
        curLineTime = chart.beats[id.id];
        if (id.sub != 0)
        {
            dTime = chart.beats[id.id + 1] - curLineTime;
            curLineTime += dTime / editor.tGrid * id.sub;
        }
        return curLineTime;
    }
    public void ReturnNote(NoteController note)
    {
        note.gameObject.SetActive(false);
        note.gameObject.transform.localPosition = Vector3.zero;
        if (returnNoteID < note.id) returnNoteID = note.id;
        notePool.ReturnObject(note);
        notes.Remove(note);
    }
    public void ReturnLine(TGridController tGrid)
    {
        tGrid.grid.SetActive(false);
        SetPrevLineID(tGrid.id);
        tGridPool.ReturnObject(tGrid);
        tGrids.Remove(tGrid);
    }
    public void SetPrevNoteID(int id)
    {
        if (id > prevNoteID) prevNoteID = id;
    }
    public void SetPrevLineID(TGridID id)
    {
        if (id > prevLineID) prevLineID = id;
    }
    private void PlaceNewObjects()
    {
        {
            int prev, cur, i;
            if (notes.Count > 0)
                prev = notes[notes.Count - 1].id;
            else
                prev = returnNoteID;
            for (cur = prev + 1; cur < chart.notes.Count; cur++)
                if (chart.notes[cur].time > musicSource.time + Parameters.NoteFallTime(chartPlaySpeed))
                    break;
            for (i = prev + 1; i < cur; i++) InitNoteObject(i);
        }
        {
            TGridID prev, cur, i;
            if (tGrids.Count > 0) prev = tGrids[tGrids.Count - 1].id;
            else prev = prevLineID;
            cur = prev; cur++;
            for (; cur <= new TGridID(chart.beats.Count - 1, 0, editor.tGrid); cur++)
                if (TGridTime(cur) > musicSource.time + Parameters.NoteFallTime(chartPlaySpeed))
                    break;
            i = prev; i++;
            for (; i < cur; i++) InitTGridObject(i);
        }
        ChangeScoreText();
    }
    public void ResetStage()
    {
        ClearStage();
        for (prevNoteID = 0; prevNoteID < chart.notes.Count; prevNoteID++)
            if (chart.notes[prevNoteID].time > musicSource.time)
                break;
        prevNoteID--;
        for (returnNoteID = prevNoteID; returnNoteID >= 0; returnNoteID--)
            if (chart.notes[returnNoteID].time + Parameters.noteReturnTime <= musicSource.time)
                break;
        for (prevLineID = new TGridID(0, 0, editor.tGrid); prevLineID <= new TGridID(chart.beats.Count - 1, 0, editor.tGrid); prevLineID++)
            if (TGridTime(prevLineID) > musicSource.time)
                break;
        prevLineID--;
        PlaceNewObjects();
    }
    private void ChangeScoreText()
    {
        if (chart.notes.Count > 1) //score calculation
        {
            int ttl = inGameNoteIDs[chart.notes.Count - 1] + 1, cur = 0;
            if (prevNoteID > 0) cur = inGameNoteIDs[prevNoteID] + 1;
            int score = (int)((800000.0 * cur * (ttl - 1) + 200000.0 * cur * (cur - 1)) / (ttl * (ttl - 1)));
            int intPart = score / 10000, decPart = (score / 100) % 100;
            string str = "" + intPart;
            if (decPart >= 10) str += "." + decPart + " %"; else str += ".0" + decPart + " %";
            scoreText.text = str;
        }
    }
    public void NoteSpeedChange(bool increase)
    {
        if (increase) chartPlaySpeed++; else chartPlaySpeed--;
        if (chartPlaySpeed == 1)
            noteSpeedLeftButton.interactable = false;
        else
            noteSpeedLeftButton.interactable = true;
        if (chartPlaySpeed == 19)
            noteSpeedRightButton.interactable = false;
        else
            noteSpeedRightButton.interactable = true;
        noteSpeedIntText.text = "" + chartPlaySpeed / 2;
        noteSpeedDeciText.text = "" + chartPlaySpeed % 2 * 5;
        ResetStage();
    }
    public void MusicSpeedChange(bool increase)
    {
        if (increase) musicPlaySpeed++; else musicPlaySpeed--;
        if (musicPlaySpeed == 1)
            musicSpeedLeftButton.interactable = false;
        else
            musicSpeedLeftButton.interactable = true;
        if (musicPlaySpeed == 30)
            musicSpeedRightButton.interactable = false;
        else
            musicSpeedRightButton.interactable = true;
        musicSpeedIntText.text = "" + musicPlaySpeed / 10;
        musicSpeedDeciText.text = "" + musicPlaySpeed % 10;
        musicSource.pitch = musicPlaySpeed / 10.0f;
    }
    public void MusicVolSliderChange()
    {
        musicVolume = (int)musicVolSlider.value;
        musicSource.volume = musicVolume / 100.0f;
        musicVolInputField.text = "" + (int)musicVolSlider.value;
    }
    public void EffectVolSliderChange()
    {
        effectVolume = (int)effectVolSlider.value;
        effectVolInputField.text = "" + (int)effectVolSlider.value;
    }
    public void PianoVolSliderChange()
    {
        pianoVolume = (int)pianoVolSlider.value;
        pianoVolInputField.text = "" + (int)pianoVolSlider.value;
    }
    public void MusicVolInput()
    {
        int vol;
        vol = Utility.GetInt(musicVolInputField.text);
        if (vol > 100 || vol < 0) { musicVolInputField.text = "" + musicVolume; return; }
        musicVolInputField.text = "" + vol;
        musicVolSlider.value = vol;
        musicVolume = vol;
        musicSource.volume = musicVolume / 100.0f;
    }
    public void EffectVolInput()
    {
        int vol;
        vol = Utility.GetInt(effectVolInputField.text);
        if (vol > 100 || vol < 0) { effectVolInputField.text = "" + effectVolume; return; }
        effectVolInputField.text = "" + vol;
        effectVolSlider.value = vol;
        effectVolume = vol;
    }
    public void PianoVolInput()
    {
        int vol;
        vol = Utility.GetInt(pianoVolInputField.text);
        if (vol > 100 || vol < 0) { pianoVolInputField.text = "" + pianoVolume; return; }
        pianoVolInputField.text = "" + vol;
        pianoVolSlider.value = vol;
        pianoVolume = vol;
    }
    public void MouseSensInput()
    {
        int mSens;
        mSens = Utility.GetInt(mouseSensInputField.text);
        if (mSens >= 100 || mSens <= 0) { mouseSensInputField.text = "" + mouseSens; return; }
        mouseSensInputField.text = "" + mSens;
        mouseSens = mSens;
    }
    public void ToggleFPS()
    {
        showFPS = fpsToggle.isOn;
        fpsText.gameObject.SetActive(showFPS);
    }
    public void ToggleLinkLine(bool state)
    {
        Utility.linkLineParent.gameObject.SetActive(state);
    }
    private void OnApplicationFocus(bool focus)
    {
        if (musicPlayState) ToggleMusicPlayState();
    }
    private void Shortcuts()
    {
        if (stageActivated && !ignoreAllInput)
        {
            if (Utility.DetectKeys(KeyCode.Return)) //Enter
                ToggleMusicPlayState();
            if (Utility.DetectKeys(KeyCode.Space)) //Space
            {
                if (musicSource.time >= musicLength)
                    antiZureTime = 0.0f;
                else
                    antiZureTime = musicSource.time;
                if (!musicPlayState) ToggleMusicPlayState();
            }
            if (Utility.ReleaseKeys(KeyCode.Space)) //Space(Release)
            {
                timeSlider.value = antiZureTime;
                StopPlaying();
                ResetStage();
            }
            if(Utility.DetectKeys(KeyCode.Home))
            {
                timeSlider.value = 0.0f;
                OnSliderValueChanged();
            }
            if(Utility.DetectKeys(KeyCode.End))
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
                if (musicSource.time > Time.deltaTime * Parameters.slowScrollSpeed)
                { timeSlider.value -= Time.deltaTime * Parameters.slowScrollSpeed; OnSliderValueChanged(); }
            if (Utility.HeldKeys(KeyCode.DownArrow)) //Down(Hold)
                if (musicSource.time < musicLength - Time.deltaTime * Parameters.slowScrollSpeed)
                { timeSlider.value += Time.deltaTime * Parameters.slowScrollSpeed; OnSliderValueChanged(); }
            if (Utility.HeldKeys(KeyCode.UpArrow, Utility.SHIFT)) //Shift+Up(Hold)
                if (musicSource.time > Time.deltaTime * Parameters.fastScrollSpeed)
                { timeSlider.value -= Time.deltaTime * Parameters.fastScrollSpeed; OnSliderValueChanged(); }
            if (Utility.HeldKeys(KeyCode.DownArrow, Utility.SHIFT)) //Shift+Down(Hold)
                if (musicSource.time < musicLength - Time.deltaTime * Parameters.fastScrollSpeed)
                { timeSlider.value += Time.deltaTime * Parameters.fastScrollSpeed; OnSliderValueChanged(); }
            float mWheel = Input.GetAxis("Mouse ScrollWheel");
            float difTime = mWheel * mouseSens * 0.1f;
            if (difTime != 0 && Input.mousePosition.x < Utility.stageWidth)
            {
                timeSlider.value = Mathf.Clamp(timeSlider.value - difTime, 0, musicLength);
                OnSliderValueChanged();
            }
        }
    }
    private void Start()
    {
        //Utility changes
        Utility.stageCamera = stageCamera;
        Utility.stageHeight = cameraUICanvas.rect.height;
        Utility.stageWidth = stageCamera.pixelWidth;
        Utility.emptyImage = emptyImage;
        Utility.cylinder = cylinder;
        Utility.cylinderAlpha = cylinderAlpha;
        Utility.cameraUICanvas = cameraUICanvas;
        Utility.xGridParent = xGridParent;
        Utility.linkLineParent = linkLineParent;
        Utility.mouseHitDetector = mouseDetector;
        //Draw border
        UILine line;
        line = Utility.DrawLineInWorldSpace(new Vector3(-15, 0, 32 + Parameters.alpha1NoteRange), new Vector3(-15, 0, 32), new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f), cylinder, 4);
        line.rectTransform.SetParent(editor.border.transform);
        line = Utility.DrawLineInWorldSpace(new Vector3(15, 0, 32 + Parameters.alpha1NoteRange), new Vector3(15, 0, 32), new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f), cylinder, 4);
        line.rectTransform.SetParent(editor.border.transform);
        line = Utility.DrawLineInWorldSpace(new Vector3(-15, 0, 32 + Parameters.maximumNoteRange), new Vector3(-15, 0, 32 + Parameters.alpha1NoteRange), new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f), cylinderAlpha, 4);
        line.rectTransform.SetParent(editor.border.transform);
        line = Utility.DrawLineInWorldSpace(new Vector3(15, 0, 32 + Parameters.maximumNoteRange), new Vector3(15, 0, 32 + Parameters.alpha1NoteRange), new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f), cylinderAlpha, 4);
        line.rectTransform.SetParent(editor.border.transform);
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
                if (stageActivated) fpsText.color = Color.white; else fpsText.color = Color.black;
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
