using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorController : MonoBehaviour
{
    //-Editor activation-
    public bool activated = false;
    //Grid
    public int xGrid = 0, tGrid = 0;
    public float xGridOffset = 0.0f;
    private List<XGrid> xGrids = new List<XGrid>();
    public InputField xGridInputField;
    public InputField tGridInputField;
    public InputField xGridOffsetInputField;
    public Slider xGridOffsetSlider;
    public Toggle borderToggle;
    //Beat lines
    private float fillFrom, fillTo, fillWithBPM;
    public InputField fillFromInputField;
    public InputField fillToInputField;
    public InputField fillWithBPMInputField;
    //Chart
    public ChartOld chart;
    public int maxUndoStep = 100;
    private List<ChartOld> undoCharts = new List<ChartOld>();
    private int currentStep = 0;
    //Note Selection
    public List<NoteSelect> noteSelect = new List<NoteSelect>();
    private List<List<NoteSelect>> undoNoteSelect = new List<List<NoteSelect>>();
    private int amountSelected = 0;
    public LocalizedText amountSel;
    public InputField noteIdSel;
    public InputField positionSel;
    public InputField timeSel;
    public InputField sizeSel;
    public InputField shiftSel;
    public LocalizedText isLinkSel;
    public LocalizedText pianoSoundsSel;
    public Button pianoSoundsButton;
    public RectTransform dragIndicator;
    //Note Placement
    public bool snapToGrid = false;
    public Toggle snapToGridToggle;
    private List<Note> clipBoard = new List<Note>();
    public GameObject noteIndicatorsToggler;
    private List<NoteIndicatorController> noteIndicators = new List<NoteIndicatorController>();
    public Transform noteIndicatorParent;
    public Toggle noteIndicatorToggle;
    public NoteIndicatorPool noteIndicatorPool;
    private bool pasteMode = false;
    //Interpolate
    private Spline positionSpline;
    private Spline sizeSpline;
    public Line curve = null;
    public bool interpolateMode = false;
    public InputField fillAmountField;
    private int fillAmount;
    //Mouse Actions
    public Vector2 dragStartPoint = new Vector2();
    public Vector2 dragEndPoint = new Vector2();
    private bool dropCurrentDrag = false;
    //Other scripts
    public BPMCalculatorController bpmCalc;
    public StageController stage;
    public PianoSoundEditor pianoSoundEditor;
    //GameObjects
    public GameObject border;
    public GameObject editPanel;
    public GameObject piano;
    //General
    public void ActivateEditor()
    {
        noteIndicatorPool.Initialize();

        while (undoCharts.Count > 0) undoCharts.RemoveAt(0);
        while (noteSelect.Count > 0) noteSelect.RemoveAt(0);
        while (undoNoteSelect.Count > 0) undoNoteSelect.RemoveAt(0);
        for (int i = 0; i < chart.notes.Count; i++) noteSelect.Add(new NoteSelect { editor = this, note = chart.notes[i] });
        currentStep = 0;
    }
    public void ToggleEditPanelState()
    {
        bool activated = !editPanel.activeSelf;
        editPanel.SetActive(activated);
    }
    //Settings
    public void ToggleBorder(bool isOn)
    {
        border.SetActive(isOn);
    }
    public void XGridNumber(string input)
    {
        int gridNumber = Utility.GetInt(input);
        int i;
        float position;
        if (gridNumber > 40) gridNumber = 40;
        if (gridNumber < 0) gridNumber = 0;
        xGrid = gridNumber;
        xGridInputField.text = "" + xGrid;
        for (i = 0; i < xGrid; i++)
        {
            xGrids[i].SetActive(true);
            position = (i + 0.5f) / xGrid * 4 - 2 + xGridOffset;
            if (position < -2.0f) position += 4.0f;
            if (position > 2.0f) position -= 4.0f;
            xGrids[i].MoveTo(position * Parameters.maximumNoteWidth);
        }
        for (; i < 25; i++)
        {
            xGrids[i].SetActive(false);
            xGrids[i].MoveTo(0.0f);
        }
    }
    public void TGridNumber(string input)
    {
        int gridNumber = Utility.GetInt(input);
        if (gridNumber > 64) gridNumber = 64;
        if (gridNumber < 0) gridNumber = 0;
        tGrid = gridNumber;
        tGridInputField.text = "" + tGrid;
        stage.ResetStage();
    }
    public void XGridOffsetInput(string input)
    {
        float offset = Utility.GetFloat(input);
        float position;
        int i;
        if (offset > 1.0f) offset = 1.0f; if (offset < -1.0f) offset = -1.0f;
        xGridOffset = offset;
        xGridOffsetInputField.text = offset.ToString("F2");
        xGridOffsetSlider.value = xGridOffset;
        for (i = 0; i < xGrid; i++)
        {
            xGrids[i].SetActive(true);
            position = (i + 0.5f) / xGrid * 4 - 2 + xGridOffset;
            if (position < -2.0f) position += 4.0f;
            if (position > 2.0f) position -= 4.0f;
            xGrids[i].MoveTo(position * Parameters.maximumNoteWidth);
        }
        for (; i < 25; i++)
        {
            xGrids[i].SetActive(false);
            xGrids[i].MoveTo(0.0f);
        }
    }
    public void XGridOffsetSlider()
    {
        int i;
        float position, value = xGridOffsetSlider.value;
        xGridOffset = value;
        xGridOffsetInputField.text = xGridOffset.ToString("F2");
        for (i = 0; i < xGrid; i++)
        {
            xGrids[i].SetActive(true);
            position = (i + 0.5f) / xGrid * 4 - 2 + xGridOffset;
            if (position < -2.0f) position += 4.0f;
            if (position > 2.0f) position -= 4.0f;
            xGrids[i].MoveTo(position * Parameters.maximumNoteWidth);
        }
        for (; i < 25; i++)
        {
            xGrids[i].SetActive(false);
            xGrids[i].MoveTo(0.0f);
        }
    }
    public void ToggleSnapToGrid(bool isOn)
    {
        snapToGrid = isOn;
    }
    public void ToggleNoteIndicator(bool isOn)
    {
        noteIndicatorsToggler.SetActive(isOn);
    }
    //-Barlines-
    public void FillBeatLines()
    {
        int i = 0, time = 0;
        float cur = fillFrom;
        RegisterUndoStep();
        for (i = 0; i < chart.beats.Count && chart.beats[i] + Parameters.minBeatLength < fillFrom; i++) ;
        while (chart.beats.Count > i && chart.beats[i] - Parameters.minBeatLength < fillTo) chart.beats.RemoveAt(i);
        for (i = 0; i < chart.beats.Count && chart.beats[i] < fillFrom; i++) ;
        if (fillWithBPM > 0.0f)
            while (cur + (60 * time) / fillWithBPM < fillTo)
            {
                chart.beats.Insert(i, cur + (60 * time) / fillWithBPM);
                i++; time++;
            }
        stage.ResetStage();
    }
    public void FillFromChange()
    {
        float from = Utility.GetFloat(fillFromInputField.text);
        if (from < 0.0f) from = 0.0f;
        if (from > stage.musicLength) from = stage.musicLength;
        fillFrom = from; fillFromInputField.text = from.ToString("F3");
    }
    public void FillToChange()
    {
        float to = Utility.GetFloat(fillToInputField.text);
        if (to < 0.0f) to = 0.0f;
        if (to > stage.musicLength) to = stage.musicLength;
        fillTo = to; fillToInputField.text = to.ToString("F3");
    }
    public void FillWithBPMChange()
    {
        float bpm = Utility.GetFloat(fillWithBPMInputField.text);
        if (bpm < 0.0f) bpm = 0.0f;
        if (bpm > 1200.0f) bpm = 1200.0f;
        fillWithBPM = bpm; fillWithBPMInputField.text = bpm.ToString("F3");
    }
    //-Undo/Redo-
    public void RegisterUndoStep()
    {
        while (undoCharts.Count > currentStep)
        {
            undoCharts.RemoveAt(currentStep);
            undoNoteSelect.RemoveAt(currentStep);
        }
        undoCharts.Add(Utility.CopyChart(chart));
        undoNoteSelect.Add(new List<NoteSelect>(noteSelect));
        currentStep++;
        if (undoCharts.Count > maxUndoStep)
        {
            undoCharts.RemoveAt(0);
            undoNoteSelect.RemoveAt(0);
            currentStep--;
        }
    }
    public void Undo()
    {
        if (currentStep > 0)
        {
            if (undoCharts.Count == currentStep)
            {
                undoCharts.Add(Utility.CopyChart(chart));
                undoNoteSelect.Add(new List<NoteSelect>(noteSelect));
            }
            currentStep--;
            chart = undoCharts[currentStep];
            noteSelect = undoNoteSelect[currentStep];
            stage.chart = chart;
            stage.projectController.project.charts[stage.diff] = chart;
            SyncStage();
            SyncSelectedAmount();
            SyncSelectedNotes();
        }
    }
    public void Redo()
    {
        if (undoCharts.Count > currentStep + 1)
        {
            currentStep++;
            chart = undoCharts[currentStep];
            noteSelect = undoNoteSelect[currentStep];
            stage.chart = chart;
            stage.projectController.project.charts[stage.diff] = chart;
            SyncStage();
            SyncSelectedAmount();
            SyncSelectedNotes();
        }
    }
    //Selection Panel
    public void UpdateSelectedAmount(int value, bool mode) //mode=false: add by value | mode=true: change to value
    {
        if (mode)
            amountSelected = value;
        else
            amountSelected += value;
        ChangeSelectionPanelValues();
        if (amountSelected != 0) BPMFieldAutoComplete();
    }
    private void SyncSelectedAmount()
    {
        int count = 0;
        foreach (NoteSelect i in noteSelect)
            if (i.prevSelected != i.selected)
                count++;
        amountSelected = count;
        ChangeSelectionPanelValues();
        if (amountSelected != 0) BPMFieldAutoComplete();
    }
    private void BPMFieldAutoComplete()
    {
        int i;
        for (i = 0; i < noteSelect.Count; i++) if (noteSelect[i].prevSelected != noteSelect[i].selected) break;
        fillFrom = chart.notes[i].time; fillFromInputField.text = fillFrom.ToString("F3");
        if (amountSelected >= 2)
        {
            int j;
            for (j = noteSelect.Count - 1; j >= 0; j--) if (noteSelect[j].prevSelected != noteSelect[j].selected) break;
            fillTo = chart.notes[j].time; fillToInputField.text = fillTo.ToString("F3");
            if (fillTo - fillFrom > Parameters.minBeatLength)
            {
                fillWithBPM = 60.0f / (chart.notes[j].time - chart.notes[i].time);
                fillWithBPMInputField.text = fillWithBPM.ToString("F3");
            }
        }
    }
    public void ChangeSelectionPanelValues()
    {
        float pos = 0, time = 0, size = 0, shift = 0;
        bool isLink = true, flag = true;
        List<PianoSound> sounds = null;
        int i, j;
        amountSelected = 0;
        for (i = 0; i < chart.notes.Count; i++) if (noteSelect[i].prevSelected != noteSelect[i].selected) amountSelected++;
        amountSel.SetStrings("Selected " + amountSelected + " note" + (amountSelected < 2 ? "" : "s"),
            "已选中" + amountSelected + "个note");
        bool selectedAny = amountSelected > 0;
        if (selectedAny)
        {
            positionSel.interactable = sizeSel.interactable = shiftSel.interactable = true;
            timeSel.interactable = pianoSoundsButton.interactable = true;
            flag = false;
            int id = -1;
            for (i = 0; i < noteSelect.Count; i++)
                if (noteSelect[i].prevSelected != noteSelect[i].selected) //note i is selected
                {
                    id = i + 1;
                    pos = chart.notes[i].position;
                    flag = true;
                    break;
                }
            if (id != -1 && amountSelected == 1) noteIdSel.text = id.ToString();
            else if (amountSelected > 1) noteIdSel.text = "-";
            while (flag && i < noteSelect.Count)
            {
                if (pos != chart.notes[i].position && noteSelect[i].prevSelected != noteSelect[i].selected) flag = false;
                i++;
            }
            positionSel.text = flag ? pos.ToString("F3") : "-";
            flag = false;
            for (i = 0; i < noteSelect.Count; i++)
                if (noteSelect[i].prevSelected != noteSelect[i].selected)
                {
                    time = chart.notes[i].time;
                    flag = true;
                    break;
                }
            while (flag && i < noteSelect.Count)
            {
                if (time != chart.notes[i].time && noteSelect[i].prevSelected != noteSelect[i].selected) flag = false;
                i++;
            }
            timeSel.text = flag ? time.ToString("F3") : "-";
            flag = false;
            for (i = 0; i < noteSelect.Count; i++)
                if (noteSelect[i].prevSelected != noteSelect[i].selected)
                {
                    size = chart.notes[i].size;
                    flag = true;
                    break;
                }
            while (flag && i < noteSelect.Count)
            {
                if (size != chart.notes[i].size && noteSelect[i].prevSelected != noteSelect[i].selected) flag = false;
                i++;
            }
            sizeSel.text = flag ? size.ToString("F3") : "-";
            flag = false;
            for (i = 0; i < noteSelect.Count; i++)
                if (noteSelect[i].prevSelected != noteSelect[i].selected)
                {
                    shift = chart.notes[i].shift;
                    flag = true;
                    break;
                }
            while (flag && i < noteSelect.Count)
            {
                if (shift != chart.notes[i].shift && noteSelect[i].prevSelected != noteSelect[i].selected) flag = false;
                i++;
            }
            shiftSel.text = flag ? shift.ToString("F3") : "-";
            flag = false;
            for (i = 0; i < noteSelect.Count; i++)
                if (noteSelect[i].prevSelected != noteSelect[i].selected)
                {
                    isLink = chart.notes[i].isLink;
                    flag = true;
                    break;
                }
            while (flag && i < noteSelect.Count)
            {
                if (isLink != chart.notes[i].isLink && noteSelect[i].prevSelected != noteSelect[i].selected) flag = false;
                i++;
            }
            isLinkSel.SetStrings(flag ? (isLink ? "True" : "False") : "-",
                flag ? (isLink ? "是" : "否") : "-");
            flag = false;
            for (i = 0; i < noteSelect.Count; i++)
                if (noteSelect[i].prevSelected != noteSelect[i].selected)
                {
                    sounds = chart.notes[i].sounds;
                    flag = true;
                    break;
                }
            while (flag && i < noteSelect.Count)
            {
                if (sounds.Count != chart.notes[i].sounds.Count && noteSelect[i].prevSelected != noteSelect[i].selected)
                    flag = false;
                else
                    for (j = 0; j < sounds.Count; j++)
                        if (noteSelect[i].prevSelected != noteSelect[i].selected &&
                            (sounds[j].delay != chart.notes[i].sounds[j].delay ||
                            sounds[j].duration != chart.notes[i].sounds[j].duration ||
                            sounds[j].pitch != chart.notes[i].sounds[j].pitch ||
                            sounds[j].volume != chart.notes[i].sounds[j].volume))
                        {
                            flag = false;
                            break;
                        }
                i++;
            }
            pianoSoundsSel.SetStrings(flag ? "Click to view" : "-", flag ? "点击查看" : "-");
        }
        else
        {
            positionSel.interactable = sizeSel.interactable = shiftSel.interactable = false;
            timeSel.interactable = pianoSoundsButton.interactable = false;
            noteIdSel.text = positionSel.text = sizeSel.text = shiftSel.text = timeSel.text = "-";
            isLinkSel.SetStrings("-");
            pianoSoundsSel.SetStrings("-");
        }
    }
    public void ChangeSelectedNote()
    {
        int id = Utility.GetInt(noteIdSel.text);
        DeselectAll();
        if (id < 1 || id > noteSelect.Count)
        {
            ChangeSelectionPanelValues();
            return;
        }
        noteIdSel.text = id.ToString();
        id--;
        noteSelect[id].prevSelected = true; noteSelect[id].selected = false;
        ChangeSelectionPanelValues();
    }
    public void ChangeSelectedNoteSize()
    {
        float size = Utility.GetFloat(sizeSel.text);
        if (size <= 0.1f) size = 0.1f; if (size >= 5.0f) size = 5.0f;
        sizeSel.text = size.ToString("F3");
        RegisterUndoStep();
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
                chart.notes[i].size = size;
        SyncStage();
    }
    public void ChangeSelectedNotePosition()
    {
        float pos = Utility.GetFloat(positionSel.text);
        if (pos < -2.0f) pos = -2.0f; if (pos > 2.0f) pos = 2.0f;
        positionSel.text = pos.ToString("F3");
        RegisterUndoStep();
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
                chart.notes[i].position = pos;
        SyncStage();
    }
    public void ChangeSelectedNoteShift()
    {
        float shift = Utility.GetFloat(shiftSel.text);
        shiftSel.text = shift.ToString("F3");
        RegisterUndoStep();
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
                chart.notes[i].shift = shift;
        SyncStage();
    }
    public void ChangeSelectedNoteTime()
    {
        float time = Utility.GetFloat(timeSel.text);
        if (time < 0.0f) time = 0.0f;
        if (time > stage.musicLength) time = stage.musicLength;
        timeSel.text = time.ToString("F3");
        RegisterUndoStep();
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
                chart.notes[i].time = time;
        SortNotes();
        SyncStage();
    }
    public void ChangeSelectedNotePianoSound()
    {
        pianoSoundsButton.interactable = false;
        bool flag = false;
        int i, j;
        List<PianoSound> sounds = null;
        for (i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                sounds = chart.notes[i].sounds;
                flag = true;
                break;
            }
        while (flag && i < noteSelect.Count)
        {
            if (sounds.Count != chart.notes[i].sounds.Count && noteSelect[i].prevSelected != noteSelect[i].selected)
                flag = false;
            else
                for (j = 0; j < sounds.Count; j++)
                    if (noteSelect[i].prevSelected != noteSelect[i].selected &&
                        (sounds[j].delay != chart.notes[i].sounds[j].delay ||
                        sounds[j].duration != chart.notes[i].sounds[j].duration ||
                        sounds[j].pitch != chart.notes[i].sounds[j].pitch ||
                        sounds[j].volume != chart.notes[i].sounds[j].volume))
                    {
                        flag = false;
                        break;
                    }
            i++;
        }
        piano.SetActive(true);
        pianoSoundEditor.Activate(this, flag ? sounds : new List<PianoSound>());
    }
    public void PianoSoundFinishedEdit(List<PianoSound> sounds)
    {
        piano.SetActive(false);
        if (sounds != null)
        {
            RegisterUndoStep();
            for (int i = 0; i < noteSelect.Count; i++)
                if (noteSelect[i].prevSelected != noteSelect[i].selected)
                    chart.notes[i].sounds = sounds;
            SyncStage();
        }
        pianoSoundsButton.interactable = true;
    }
    //Mouse Actions
    private void SendDragUpdate()
    {
        foreach (NoteSelect i in noteSelect) i.UpdateState();
    }
    private void SendDragEnd()
    {
        foreach (NoteSelect i in noteSelect) i.EndDrag();
    }
    private void DeselectAll()
    {
        foreach (NoteSelect i in noteSelect) i.prevSelected = i.selected = false;
        UpdateSelectedAmount(0, true);
    }
    private Vector2 SetDragPosition(Vector2 original)
    {
        Vector2 point = new Vector2(0.0f, 0.0f);
        Vector3 pos = Utility.GetMouseWorldPos();
        if (pos.y <= -1.0f) return original;
        point.x = pos.x / Parameters.maximumNoteWidth;
        if (pos.x < -2 * Parameters.maximumNoteWidth) point.x = -2;
        if (pos.x > 2 * Parameters.maximumNoteWidth) point.x = 2;
        pos.z -= 32;
        if (pos.z < 0.0f) pos.z = 0.0f;
        if (pos.z > Parameters.maximumNoteRange) pos.z = Parameters.maximumNoteRange;
        point.y = stage.timeSlider.value + pos.z * Parameters.NoteFallTime(stage.chartPlaySpeed) / Parameters.maximumNoteRange;
        if (point.y > stage.musicLength) point.y = stage.musicLength;
        return point;
    }
    private void UpdateDragIndicator()
    {
        float xMax = dragStartPoint.x > dragEndPoint.x ? dragStartPoint.x : dragEndPoint.x;
        float xMin = dragStartPoint.x + dragEndPoint.x - xMax;
        xMax *= Parameters.maximumNoteWidth; xMin *= Parameters.maximumNoteWidth;
        float yMax = dragStartPoint.y > dragEndPoint.y ? dragStartPoint.y : dragEndPoint.y;
        float yMin = dragStartPoint.y + dragEndPoint.y - yMax;
        yMax = (yMax - stage.timeSlider.value) / Parameters.NoteFallTime(stage.chartPlaySpeed) * Parameters.maximumNoteRange;
        yMin = (yMin - stage.timeSlider.value) / Parameters.NoteFallTime(stage.chartPlaySpeed) * Parameters.maximumNoteRange;
        yMax = yMax < Parameters.maximumNoteRange ? yMax : Parameters.maximumNoteRange;
        yMin = yMin > 0.0f ? yMin : 0.0f;
        dragIndicator.offsetMax = new Vector2(xMax, yMax);
        dragIndicator.offsetMin = new Vector2(xMin, yMin);
    }
    private void CloseDragIndicator()
    {
        dragIndicator.offsetMax = new Vector2(0.0f, 0.0f);
        dragIndicator.offsetMin = new Vector2(0.0f, 0.0f);
    }
    //Grid & Note Placement
    private Vector2 QuantizePosition(Vector2 pos)
    {
        if (pos.x > 2.0f || pos.x < -2.0f) return pos;
        float position;
        float minDistance = 4.0f, snapX = pos.x, snapT = pos.y;
        bool interpolated = false;
        //Snap to T grid
        if (tGrid != 0 && chart.beats.Count > 0 && pos.y >= chart.beats[0] && pos.y <= chart.beats[chart.beats.Count - 1])
        {
            int i;
            for (i = 0; i < chart.beats.Count; i++)
                if (chart.beats[i] == pos.y)
                {
                    snapT = pos.y;
                    break;
                }
                else if (chart.beats[i] > pos.y)
                    break;
            if (chart.beats[i] > pos.y)
            {
                float dif = chart.beats[i] - chart.beats[i - 1], min = chart.beats[i], max;
                while (min > pos.y) min -= dif / tGrid;
                max = min + dif / tGrid;
                if (pos.y > (max + min) / 2)
                    snapT = max;
                else
                    snapT = min;
            }
        }
        //Snap to the curve if interpolating
        if (interpolateMode)
            if (snapT < positionSpline.Max && snapT > positionSpline.Min)
            {
                interpolated = true;
                snapX = positionSpline.Value(snapT);
                if (snapX > 2.0f) snapX = 2.0f;
                if (snapX < -2.0f) snapX = -2.0f;
            }
        //Snap to X grid if not interpolating
        if (!interpolated)
        {
            if (Mathf.Abs(pos.x + 2.0f) < minDistance) { minDistance = Mathf.Abs(pos.x + 2.0f); snapX = -2.0f; }
            if (Mathf.Abs(pos.x - 2.0f) < minDistance) { minDistance = Mathf.Abs(pos.x - 2.0f); snapX = 2.0f; }
            for (int i = 0; i < xGrid; i++)
            {
                position = (i + 0.5f) / xGrid * 4 - 2 + xGridOffset;
                if (position < -2.0f) position += 4.0f;
                if (position > 2.0f) position -= 4.0f;
                if (Mathf.Abs(pos.x - position) < minDistance)
                {
                    minDistance = Mathf.Abs(pos.x - position);
                    snapX = position;
                }
            }
            if (xGrid == 0) snapX = pos.x;
        }
        return new Vector2(snapX, snapT);
    }
    //Note Manipulations
    private void AddNote(Note note) //Be aware that by default Note is a reference
    {
        int i, j;
        for (i = 0; i < chart.notes.Count; i++)
            if (chart.notes[i].time > note.time || (chart.notes[i].time == note.time && chart.notes[i].position > note.position))
                break;
        for (j = 0; j < chart.notes.Count; j++)
        {
            if (chart.notes[j].nextLink >= i) chart.notes[j].nextLink++;
            if (chart.notes[j].prevLink >= i) chart.notes[j].prevLink++;
        }
        chart.notes.Insert(i, note);
        noteSelect.Insert(i, new NoteSelect { note = note, editor = this, prevSelected = false, selected = false });
    }
    private void NoteIndexQuickSort(List<int> index, int low, int high)
    {
        int pivot = index[low];
        int i = low, j = high;
        float pivotKey = chart.notes[pivot].time;
        while (i < j)
        {
            while (i < j && chart.notes[index[j]].time >= pivotKey) j--;
            index[i] = index[j];
            while (i < j && chart.notes[index[i]].time <= pivotKey) i++;
            index[j] = index[i];
        }
        index[i] = pivot;
        if (low < i - 1) NoteIndexQuickSort(index, low, i - 1);
        if (i + 1 < high) NoteIndexQuickSort(index, i + 1, high);
    }
    private void SortNotes() //Must be called after note property changes
    {
        List<int> index = new List<int>();
        List<int> inv = new List<int>(new int[chart.notes.Count]);
        for (int i = 0; i < chart.notes.Count; i++) index.Add(i);
        NoteIndexQuickSort(index, 0, chart.notes.Count - 1);
        for (int i = 0; i < chart.notes.Count; i++) inv[index[i]] = i;
        foreach (Note note in chart.notes)
        {
            if (note.prevLink != -1) note.prevLink = inv[note.prevLink];
            if (note.nextLink != -1) note.nextLink = inv[note.nextLink];
        }
        List<Note> notes = new List<Note>(new Note[chart.notes.Count]);
        for (int i = 0; i < chart.notes.Count; i++) notes[i] = chart.notes[index[i]];
        List<bool> selected = new List<bool>(new bool[chart.notes.Count]);
        for (int i = 0; i < chart.notes.Count; i++) selected[i] = noteSelect[i].prevSelected != noteSelect[i].selected;
        for (int i = 0; i < chart.notes.Count; i++)
        {
            noteSelect[i].note = notes[i];
            noteSelect[i].prevSelected = selected[index[i]];
            noteSelect[i].selected = false;
        }
        chart.notes = notes;
        int swapCount;
        do
        {
            swapCount = 0;
            for (int i = 0; i < chart.notes.Count; i++)
                if (chart.notes[i].nextLink != -1 && i > chart.notes[i].nextLink)
                {
                    swapCount++;
                    int next = chart.notes[i].nextLink;
                    chart.notes[i].nextLink = chart.notes[next].nextLink;
                    chart.notes[next].nextLink = i;
                    chart.notes[next].prevLink = chart.notes[i].prevLink;
                    chart.notes[i].prevLink = next;
                    next = chart.notes[i].nextLink;
                    int prev = chart.notes[i].prevLink;
                    if (next != -1) chart.notes[next].prevLink = i;
                    if (prev != -1 && chart.notes[prev].prevLink != -1)
                    {
                        int prev2 = chart.notes[prev].prevLink;
                        chart.notes[prev2].nextLink = prev;
                    }
                }
        } while (swapCount > 0);
    }
    private void SyncSelectedNotes()
    {
        for (int i = 0; i < chart.notes.Count; i++) noteSelect[i].note = chart.notes[i];
    }
    private void SyncStage()
    {
        ChartProperties.GetInGameNoteIds(chart, out stage.inGameNoteIDs);
        ChartProperties.GetCollidedNotes(chart, out stage.collided);
        stage.ResetStage();
    }
    private void DeleteSelectedNotes()
    {
        List<int> deleted = new List<int>();
        int d = 0;
        RegisterUndoStep();
        for (int i = 0; i < noteSelect.Count; i++)
        {
            deleted.Add(d);
            if (noteSelect[i].prevSelected != noteSelect[i].selected) d++;
        }
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                int prev = chart.notes[i].prevLink, next = chart.notes[i].nextLink;
                if (prev != -1) chart.notes[prev].nextLink = next;
                if (next != -1) chart.notes[next].prevLink = prev;
            }
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected == noteSelect[i].selected && chart.notes[i].isLink)
            {
                int prev = chart.notes[i].prevLink, next = chart.notes[i].nextLink;
                if (prev != -1) chart.notes[i].prevLink -= deleted[prev];
                if (next != -1) chart.notes[i].nextLink -= deleted[next];
            }
        for (int i = noteSelect.Count - 1; i >= 0; i--)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                noteSelect.RemoveAt(i);
                chart.notes.RemoveAt(i);
            }
        UpdateSelectedAmount(0, true);
        SyncStage();
    }
    public void LinkSelectedNotes()
    {
        RegisterUndoStep();
        UnlinkSelectedNotes(false);
        int prev = -1;
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                chart.notes[i].prevLink = prev;
                if (prev != -1) chart.notes[prev].nextLink = i;
                chart.notes[i].isLink = true;
                prev = i;
            }
        if (prev != -1) chart.notes[prev].nextLink = -1;
        stage.ResetStage();
        ChangeSelectionPanelValues();
    }
    public void UnlinkSelectedNotes(bool undo) //undo=true: register undo step
    {
        if (undo) RegisterUndoStep();
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                if (chart.notes[i].prevLink != -1) chart.notes[chart.notes[i].prevLink].nextLink = chart.notes[i].nextLink;
                if (chart.notes[i].nextLink != -1) chart.notes[chart.notes[i].nextLink].prevLink = chart.notes[i].prevLink;
                chart.notes[i].nextLink = chart.notes[i].prevLink = -1;
                chart.notes[i].isLink = false;
            }
        if (undo) stage.ResetStage();
        ChangeSelectionPanelValues();
    }
    public void QuantizeSelectedNotes()
    {
        RegisterUndoStep();
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                Vector2 pos = QuantizePosition(new Vector2(chart.notes[i].position, chart.notes[i].time));
                chart.notes[i].position = pos.x;
                chart.notes[i].time = pos.y;
            }
        SortNotes();
        SyncStage();
        ChangeSelectionPanelValues();
    }
    public void MirrorSelectedNotes()
    {
        RegisterUndoStep();
        for (int i = 0; i < chart.notes.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                if (chart.notes[i].position < -2.0f || chart.notes[i].position > 2.0f) continue;
                chart.notes[i].position = -chart.notes[i].position;
            }
        SyncStage();
        ChangeSelectionPanelValues();
    }
    private void AdjustSelectedNoteTime(float time)
    {
        RegisterUndoStep();
        for (int i = 0; i < chart.notes.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                chart.notes[i].time += time;
                if (chart.notes[i].time < 0.0f) chart.notes[i].time = 0.0f;
                if (chart.notes[i].time > stage.musicLength) chart.notes[i].time = stage.musicLength;
            }
        SortNotes();
        SyncStage();
        ChangeSelectionPanelValues();
    }
    private void AdjustSelectedNotePosition(float pos)
    {
        RegisterUndoStep();
        for (int i = 0; i < chart.notes.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                if (chart.notes[i].position < -2.0f || chart.notes[i].position > 2.0f) continue;
                chart.notes[i].position += pos;
                if (chart.notes[i].position < -2.0f) chart.notes[i].position = -2.0f;
                if (chart.notes[i].position > 2.0f) chart.notes[i].position = 2.0f;
            }
        SyncStage();
        ChangeSelectionPanelValues();
    }
    private void AdjustSelectedNoteTimeByGrid(int amount)
    {
        if (tGrid == 0 || chart.beats.Count == 0) return;
        RegisterUndoStep();
        int beatIndex = 0, i = 0;
        while (i < chart.notes.Count && chart.notes[i].time <= chart.beats[0]) i++;
        for (i = 0; i < chart.notes.Count && chart.notes[i].time <= chart.beats[chart.beats.Count - 1]; i++)
        {
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                float minDistance = stage.musicLength;
                while (chart.beats[beatIndex + 1] < chart.notes[i].time) beatIndex++;
                if (amount == -1 && chart.notes[i].time - chart.beats[0] < 1e-4)
                {
                    chart.notes[i].time = chart.beats[0];
                    continue;
                }
                else if (amount == 1 && chart.beats[chart.beats.Count - 1] - chart.notes[i].time < 1e-4)
                {
                    chart.notes[i].time = chart.beats[chart.beats.Count - 1];
                    continue;
                }
                else if (amount == 1 && chart.beats[beatIndex + 1] - chart.notes[i].time < 1e-4) beatIndex++;
                float beatTime = chart.beats[beatIndex + 1] - chart.beats[beatIndex];
                float adjusted = 0.0f;
                for (int j = 0; j <= tGrid; j++)
                {
                    float time = chart.beats[beatIndex] + beatTime / tGrid * j;
                    float diff = amount * (time - chart.notes[i].time);
                    if (diff > 1e-4f && diff < minDistance) { minDistance = diff; adjusted = time; }
                }
                chart.notes[i].time = adjusted;
            }
        }
        SortNotes();
        SyncStage();
        ChangeSelectionPanelValues();
    }
    private void AdjustSelectedNotePositionByGrid(int amount)
    {
        if (xGrid == 0) return;
        RegisterUndoStep();
        for (int i = 0; i < chart.notes.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                if (chart.notes[i].position < -2.0f || chart.notes[i].position > 2.0f) continue;
                float minDistance = 4.0f;
                float adjusted = 0.0f;
                float diff;
                diff = amount * (2.0002f - chart.notes[i].position);
                if (diff > 1e-4f) { minDistance = diff; adjusted = 2.0f; }
                diff = amount * (-2.0002f - chart.notes[i].position);
                if (diff > 1e-4f) { minDistance = diff; adjusted = -2.0f; }
                for (int j = 0; j < xGrid; j++)
                {
                    float position = (j + 0.5f) / xGrid * 4 - 2 + xGridOffset;
                    if (position > 2.0f) position -= 4.0f;
                    if (position < -2.0f) position += 4.0f;
                    diff = amount * (position - chart.notes[i].position);
                    if (diff > 1e-4f && diff < minDistance) { minDistance = diff; adjusted = position; }
                }
                chart.notes[i].position = adjusted;
            }
        SyncStage();
        ChangeSelectionPanelValues();
    }
    private void AdjustSelectedNoteSize(float amount)
    {
        RegisterUndoStep();
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                float newSize = chart.notes[i].size + amount;
                if (newSize > 5.0f) newSize = 5.0f;
                if (newSize < 0.1f) newSize = 0.1f;
                chart.notes[i].size = newSize;
            }
        SyncStage();
        ChangeSelectionPanelValues();
    }
    public void CopySelectedNotes()
    {
        pasteMode = false;
        while (clipBoard.Count > 0) clipBoard.RemoveAt(0);
        List<int> index = new List<int>(new int[chart.notes.Count]);
        int count = 0, firstPos = -1;
        for (int i = 0; i < chart.notes.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                index[i] = count++;
                if (count == 1) firstPos = i;
            }
        if (firstPos == -1) return;
        List<int> prevLink = new List<int>(new int[chart.notes.Count]);
        List<int> nextLink = new List<int>(new int[chart.notes.Count]);
        for (int i = 0; i < chart.notes.Count; i++) prevLink[i] = nextLink[i] = -1;
        for (int i = 0; i < chart.notes.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected && chart.notes[i].isLink)
            {
                int prev = chart.notes[i].prevLink;
                while (prev != -1 && noteSelect[prev].prevSelected == noteSelect[prev].selected) prev = chart.notes[prev].prevLink;
                prevLink[i] = prev == -1 ? -1 : index[prev];
                if (prev != -1) nextLink[prev] = index[i];
            }
        for (int i = 0; i < chart.notes.Count; i++)
            if (noteSelect[i].prevSelected != noteSelect[i].selected)
            {
                Note note = new Note
                {
                    isLink = chart.notes[i].isLink,
                    prevLink = prevLink[i],
                    nextLink = nextLink[i],
                    position = chart.notes[i].position,
                    time = chart.notes[i].time - chart.notes[firstPos].time,
                    shift = chart.notes[i].shift,
                    size = chart.notes[i].size,
                    sounds = chart.notes[i].sounds
                };
                clipBoard.Add(note);
            }
    }
    public void CutSelectedNotes()
    {
        CopySelectedNotes();
        DeleteSelectedNotes();
    }
    public void PasteNotes()
    {
        if (clipBoard.Count != 0) pasteMode = true;
    }
    private void PlaceNotes()
    {
        if (!pasteMode)
        {
            RegisterUndoStep();
            AddNote(noteIndicators[0].Note);
            SyncStage();
        }
        else
        {
            RegisterUndoStep();
            int noteCount = chart.notes.Count;
            for (int i = 0; i < noteIndicators.Count; i++)
            {
                Note note = noteIndicators[i].Note;
                if (note.prevLink != -1) note.prevLink += noteCount;
                if (note.nextLink != -1) note.nextLink += noteCount;
                chart.notes.Add(note);
                noteSelect.Add(new NoteSelect { editor = this, note = note, prevSelected = true, selected = false });
            }
            SortNotes();
            SyncStage();
            ChangeSelectionPanelValues();
        }
        pasteMode = false;
    }
    //Interpolate
    public void InitSpline(bool linear)
    {
        int firstIndex = -1, lastIndex = -1;
        List<float> size = new List<float>();
        List<float> position = new List<float>();
        List<float> time = new List<float>();
        for (int i = 0; i < noteSelect.Count; i++)
            if (noteSelect[i].selected != noteSelect[i].prevSelected)
            {
                float p = chart.notes[i].position;
                if (p > 2.01f || p < -2.01f) continue;
                if (firstIndex == -1) firstIndex = i;
                lastIndex = i;
                float t = chart.notes[i].time;
                float s = chart.notes[i].size;
                size.Add(s);
                position.Add(p);
                time.Add(t);
            }
        if (time.Count < 2) return;
        noteSelect[firstIndex].selected = !noteSelect[firstIndex].selected;
        noteSelect[lastIndex].selected = !noteSelect[lastIndex].selected;
        DeleteSelectedNotes();
        positionSpline = new Spline(time, position, linear);
        sizeSpline = new Spline(time, size, linear);
        if (curve == null)
        {
            curve = Utility.DrawLineInWorldSpace(Vector3.zero, Vector3.up, Parameters.linkLineColor, 0.035f);
            curve.transform.SetParent(stage.linkLineParent);
        }
        curve.SetActive(true);
        curve.Color = new Color(85.0f / 255, 192.0f / 255, 1.0f);
        interpolateMode = true;
        fillAmountField.interactable = true;
    }
    public void UpdateCurve()
    {
        List<Vector3> worldPoints = new List<Vector3>();
        float time = stage.timeSlider.value, fallTime = Parameters.NoteFallTime(stage.chartPlaySpeed);
        float min = Mathf.Max(positionSpline.Min, time), max = Mathf.Min(positionSpline.Max, time + fallTime);
        if (min > time + fallTime || max < time) { curve.CurveMoveTo(worldPoints); return; }
        for (int i = 0; i < 400; i++)
        {
            float t = min + i * (max - min) / 400, p = positionSpline.Value(t);
            float x = Parameters.maximumNoteWidth * p;
            float z = Parameters.maximumNoteRange / fallTime * (t - time);
            worldPoints.Add(new Vector3(x, 0, z + 32.0f));
        }
        curve.CurveMoveTo(worldPoints);
        curve.Layer = 1;
    }
    public void ExitInterpolation()
    {
        curve.SetActive(false);
        interpolateMode = false;
        fillAmountField.interactable = false;
    }
    public void FillAmountFieldChanged()
    {
        int amount = Utility.GetInt(fillAmountField.text);
        if (amount < 0) amount = 0;
        fillAmountField.text = amount.ToString();
        fillAmount = amount;
    }
    public void FillNotesOnCurve()
    {
        if (interpolateMode)
        {
            RegisterUndoStep();
            float min = positionSpline.Min, max = positionSpline.Max;
            for (int i = 0; i < fillAmount; i++)
            {
                float time = min + (max - min) / (fillAmount + 1) * (i + 1);
                float pos = positionSpline.Value(time), size = sizeSpline.Value(time);
                if (pos > 2.0f) pos = 2.0f;
                if (pos < -2.0f) pos = -2.0f;
                if (size > 5.0f) size = 5.0f;
                if (size < 0.1f) size = 0.1f;
                Note note = new Note
                {
                    isLink = false,
                    prevLink = -1,
                    nextLink = -1,
                    position = pos,
                    size = size,
                    time = time,
                    shift = 0.0f,
                    sounds = new List<PianoSound>()
                };
                NoteSelect noteSelect = new NoteSelect
                {
                    editor = this,
                    note = note,
                    prevSelected = true,
                    selected = false
                };
                chart.notes.Add(note);
                this.noteSelect.Add(noteSelect);
            }
            SortNotes();
            UpdateSelectedAmount(0, true);
            SyncStage();
        }
    }
    //Note indicators
    public void UpdateNoteIndicators()
    {
        // Check for destroying and instantiating
        if (pasteMode)
        {
            int indicatorCount = clipBoard.Count;
            while (noteIndicators.Count > indicatorCount) ReturnIndicator(indicatorCount);
            for (int i = 0; i < noteIndicators.Count; i++) UpdateNoteIndicatorValue(i);
            for (int i = noteIndicators.Count; i < indicatorCount; i++) GetNoteIndicator(i);
        }
        else
        {
            while (noteIndicators.Count > 1) ReturnIndicator(1);
            NoteIndicatorController indicator;
            if (noteIndicators.Count == 0)
            {
                indicator = noteIndicatorPool.GetObject();
                noteIndicators.Add(indicator);
            }
            else
                indicator = noteIndicators[0];
            indicator.gameObject.SetActive(true);
            indicator.gameObject.transform.SetParent(noteIndicatorParent);
            Note note = new Note
            {
                isLink = false,
                prevLink = -1,
                nextLink = -1,
                position = 0.0f,
                shift = 0.0f,
                size = 1.0f,
                sounds = new List<PianoSound>(),
                time = 0.0f
            };
            indicator.Initialize(this, note, note, stage.musicLength);
        }
        // Update
        float stageTime = stage.timeSlider.value;
        if (Utility.GetMouseWorldPos().y < -1.0f || Input.mousePosition.x > Utility.stageWidth)
            foreach (NoteIndicatorController indicator in noteIndicators) indicator.NoColor();
        else
        {
            Vector2 pos = SetDragPosition(new Vector2(0.0f, 0.0f));
            if (snapToGrid) pos = QuantizePosition(pos);
            foreach (NoteIndicatorController indicator in noteIndicators)
            {
                float offset = pos.x;
                if (pasteMode)
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) // Shift held
                        offset = 0.0f;
                    else
                        offset -= clipBoard[0].position;
                indicator.Move(pos.y, offset, stageTime);
            }
            // Reminder: what if the first note is out of stage
        }
    }
    private void GetNoteIndicator(int id)
    {
        NoteIndicatorController indicator;
        indicator = noteIndicatorPool.GetObject();
        noteIndicators.Add(indicator);
        UpdateNoteIndicatorValue(id);
    }
    private void UpdateNoteIndicatorValue(int id)
    {
        NoteIndicatorController indicator = noteIndicators[id];
        indicator.gameObject.SetActive(true);
        indicator.gameObject.transform.SetParent(noteIndicatorParent);
        Note nextLink = clipBoard[id].isLink && clipBoard[id].nextLink != -1 ? clipBoard[clipBoard[id].nextLink] : clipBoard[id];
        indicator.Initialize(this, clipBoard[id], nextLink, stage.musicLength);
    }
    public void ReturnIndicator(int id)
    {
        noteIndicators[id].gameObject.SetActive(false);
        noteIndicators[id].linkLine.SetActive(false);
        noteIndicators[id].gameObject.transform.localPosition = Vector3.zero;
        noteIndicatorPool.ReturnObject(noteIndicators[id]);
        noteIndicators.RemoveAt(id);
    }
    private void LeftMouseButtonPressed()
    {
        //If the mouse is out of range when pressed, ignoreCurrentDrag
        dragStartPoint = SetDragPosition(dragStartPoint);
        if (Utility.GetMouseWorldPos().y < -1.0f || Input.mousePosition.x > Utility.stageWidth)
            dropCurrentDrag = true;
        else
        {
            if (!Utility.FunctionalKeysHeld(Utility.CTRL))
                DeselectAll();
            dragEndPoint = SetDragPosition(dragEndPoint);
            SendDragUpdate();
            UpdateDragIndicator();
        }
    }
    private void LeftMouseButtonHeld()
    {
        foreach (NoteIndicatorController indicator in noteIndicators) indicator.NoColor();
        dragEndPoint = SetDragPosition(dragEndPoint);
        SendDragUpdate();
        UpdateDragIndicator();
    }
    private void LeftMouseButtonReleased()
    {
        if (dropCurrentDrag)
            dropCurrentDrag = false;
        else
        {
            SendDragEnd();
            CloseDragIndicator();
        }
    }
    private void RightMouseButtonReleased()
    {
        if (!(Utility.GetMouseWorldPos().y < -1.0f || Input.mousePosition.x > Utility.stageWidth))
        {
            DeselectAll();
            PlaceNotes();
        }
    }
    //Updated Every Frame
    private void MouseActions()
    {
        //Select button
        if (Input.GetMouseButtonDown(Parameters.selectButton) && !CurrentState.ignoreAllInput && !dropCurrentDrag && activated) //Select button pressed
            LeftMouseButtonPressed();
        else if (Input.GetMouseButtonUp(Parameters.selectButton) && activated) //Select button released
            LeftMouseButtonReleased();
        else if (Input.GetMouseButton(Parameters.selectButton) && !CurrentState.ignoreAllInput && !dropCurrentDrag && activated) //Select button being held
            LeftMouseButtonHeld();
        //Place button
        if (Input.GetMouseButtonUp(Parameters.placeButton) && !CurrentState.ignoreAllInput && !dropCurrentDrag && !Input.GetMouseButton(Parameters.selectButton))
            RightMouseButtonReleased();
    }
    private void Shortcuts()
    {
        if (activated && !CurrentState.ignoreAllInput)
        {
            if (Utility.DetectKeys(KeyCode.Z, Utility.CTRL)) //Ctrl+Z
                Undo();
            if (Utility.DetectKeys(KeyCode.Z, Utility.CTRL + Utility.SHIFT)) //Ctrl+Shift+Z
                Redo();
            if (Utility.DetectKeys(KeyCode.Y, Utility.CTRL)) //Ctrl+Y
                Redo();
            if (Utility.DetectKeys(KeyCode.Delete)) //Delete
                DeleteSelectedNotes();
            if (Utility.DetectKeys(KeyCode.G)) //G
            {
                snapToGrid = !snapToGrid;
                snapToGridToggle.isOn = snapToGrid;
            }
            if (Utility.DetectKeys(KeyCode.A, Utility.CTRL)) //Ctrl+A
            {
                foreach (NoteSelect i in noteSelect) i.prevSelected = true;
                SyncSelectedAmount();
            }
            if (Utility.DetectKeys(KeyCode.L)) //L
                LinkSelectedNotes();
            if (Utility.DetectKeys(KeyCode.U)) //U
                UnlinkSelectedNotes(true);
            if (Utility.DetectKeys(KeyCode.Q)) //Q
                QuantizeSelectedNotes();
            if (Utility.DetectKeys(KeyCode.W)) //W
                AdjustSelectedNoteTime(0.001f);
            if (Utility.DetectKeys(KeyCode.S)) //S
                AdjustSelectedNoteTime(-0.001f);
            if (Utility.DetectKeys(KeyCode.A)) //A
                AdjustSelectedNotePosition(-0.01f);
            if (Utility.DetectKeys(KeyCode.D)) //D
                AdjustSelectedNotePosition(0.01f);
            if (Utility.DetectKeys(KeyCode.W, Utility.ALT)) //Alt+W
                AdjustSelectedNoteTime(0.01f);
            if (Utility.DetectKeys(KeyCode.S, Utility.ALT)) //Alt+S
                AdjustSelectedNoteTime(-0.01f);
            if (Utility.DetectKeys(KeyCode.A, Utility.ALT)) //Alt+A
                AdjustSelectedNotePosition(-0.1f);
            if (Utility.DetectKeys(KeyCode.D, Utility.ALT)) //Alt+D
                AdjustSelectedNotePosition(0.1f);
            if (Utility.DetectKeys(KeyCode.W, Utility.SHIFT)) //Shift+W
                AdjustSelectedNoteTimeByGrid(1);
            if (Utility.DetectKeys(KeyCode.S, Utility.SHIFT)) //Shift+S
                AdjustSelectedNoteTimeByGrid(-1);
            if (Utility.DetectKeys(KeyCode.A, Utility.SHIFT)) //Shift+A
                AdjustSelectedNotePositionByGrid(-1);
            if (Utility.DetectKeys(KeyCode.D, Utility.SHIFT)) //Shift+D
                AdjustSelectedNotePositionByGrid(1);
            if (Utility.DetectKeys(KeyCode.Z)) //Z
                AdjustSelectedNoteSize(-0.01f);
            if (Utility.DetectKeys(KeyCode.X)) //X
                AdjustSelectedNoteSize(0.01f);
            if (Utility.DetectKeys(KeyCode.Z, Utility.SHIFT)) //Shift+Z
                AdjustSelectedNoteSize(-0.1f);
            if (Utility.DetectKeys(KeyCode.X, Utility.SHIFT)) //Shift+X
                AdjustSelectedNoteSize(0.1f);
            if (Utility.DetectKeys(KeyCode.M)) //M
                MirrorSelectedNotes();
            if (Utility.DetectKeys(KeyCode.C, Utility.CTRL)) //Ctrl+C
                CopySelectedNotes();
            if (Utility.DetectKeys(KeyCode.X, Utility.CTRL)) //Ctrl+X
                CutSelectedNotes();
            if (Utility.DetectKeys(KeyCode.V, Utility.CTRL)) //Ctrl+V
                PasteNotes();
            if (Utility.DetectKeys(KeyCode.I)) //I
                InitSpline(false);
            if (Utility.DetectKeys(KeyCode.I, Utility.SHIFT)) //Shift+I
                InitSpline(true);
            if (Utility.DetectKeys(KeyCode.I, Utility.CTRL)) //Ctrl+I
                ExitInterpolation();
            if (Utility.DetectKeys(KeyCode.F)) //F
                FillNotesOnCurve();
        }
    }
    private void LoadPlayerPrefs()
    {
        xGridInputField.text = PlayerPrefs.GetInt("XGrid Count", xGrid).ToString();
        XGridNumber(xGridInputField.text);
        tGridInputField.text = PlayerPrefs.GetInt("TGrid Count", tGrid).ToString();
        TGridNumber(tGridInputField.text);
        xGridOffsetInputField.text = PlayerPrefs.GetFloat("XGrid Offset", xGridOffset).ToString();
        XGridOffsetInput(xGridOffsetInputField.text);
        snapToGridToggle.isOn = Utility.PlayerPrefsGetBool("Snap To Grid", snapToGrid);
        ToggleSnapToGrid(snapToGridToggle.isOn);
        noteIndicatorToggle.isOn = Utility.PlayerPrefsGetBool("Show Indicator", noteIndicatorsToggler.activeSelf);
        ToggleNoteIndicator(noteIndicatorToggle.isOn);
        borderToggle.isOn = Utility.PlayerPrefsGetBool("Show Border", border.activeSelf);
        ToggleBorder(borderToggle.isOn);
    }
    private void Start()
    {
        fillFromInputField.text = fillToInputField.text = fillWithBPMInputField.text = "0.000";
        fillFrom = fillTo = fillWithBPM = 0.0f;
    }
    public void Initialize()
    {
        for (int i = 0; i < 40; i++) xGrids.Add(new XGrid());
        LoadPlayerPrefs();
    }
    private void Update()
    {
        if (activated)
        {
            Shortcuts();
            UpdateNoteIndicators();
            if (interpolateMode) UpdateCurve();
            MouseActions();
        }
    }
}
