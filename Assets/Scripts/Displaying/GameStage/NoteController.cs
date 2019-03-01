using UnityEngine;

public class NoteController : MonoBehaviour
{
    public int id;
    private float time;
    public SpriteRenderer noteSprite;
    public SpriteRenderer frameSprite;
    public SpriteRenderer waveSprite;
    public SpriteRenderer lightSprite;
    public SpriteRenderer circleSprite;
    public StageController stage;
    private Note curNote;
    public Sprite pianoNoteSprite;
    public Sprite blankNoteSprite;
    public Sprite slideNoteSprite;
    public Sprite cirSprite;
    public Sprite wSprite;
    public Sprite lSprite;
    public Sprite[] noteEffectFrames;
    private float pianoNoteScale = 7.0f;
    private float blankNoteScale = 4.5f;
    private float slideNoteScale = 4.5f;
    private float noteEffectScale = 8.5f;
    private bool soundPlayed;
    private bool inRange = true;
    public AudioSource audioSource;
    public PianoSoundsLoader piano;
    public AudioClip clickClip;
    private Color waveColor;
    private Line linkLine;
    private void CheckForReturn()
    {
        if (time > curNote.time + Parameters.noteReturnTime)
            ForceReturn();
    }
    public void ForceReturn()
    {
        gameObject.SetActive(false);
        linkLine.SetActive(false);
        stage.SetPrevNoteId(id);
        stage.ReturnNote(this);
    }
    private void PositionUpdate()
    {
        float alpha;
        float x = Parameters.maximumNoteWidth * curNote.position;
        float z = Parameters.maximumNoteRange / Parameters.NoteFallTime(stage.chartPlaySpeed) * (curNote.time - time);
        if (inRange)
        {
            if (z <= Parameters.maximumNoteRange && z >= Parameters.alpha1NoteRange)
            {
                alpha = (Parameters.maximumNoteRange - z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
                noteSprite.color = new Color(1.0f, 1.0f, 1.0f, alpha);
            }
            else if (z <= Parameters.alpha1NoteRange)
                noteSprite.color = Color.white;
            else
                noteSprite.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
        else
            noteSprite.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        if (stage.editor.noteSelect[id].prevSelected != stage.editor.noteSelect[id].selected) //Note selected
        {
            alpha = noteSprite.color.a;
            noteSprite.color = new Color(85.0f / 255, 192.0f / 255, 1.0f, alpha);
        }
        else if (stage.collided[id] && !curNote.isLink)
        {
            alpha = noteSprite.color.a;
            noteSprite.color = new Color(1.0f, 85.0f / 255, 85.0f / 255, alpha);
        }
        if (z <= 0.0f)
        {
            z = 0.0f;
            if (!soundPlayed)
            {
                soundPlayed = true;
                if (inRange)
                {
                    stage.SetPrevNoteId(id);
                    if (curNote.isLink)
                        audioSource.PlayOneShot(clickClip, 0.5f * stage.effectVolume / 100);
                    else
                        audioSource.PlayOneShot(clickClip, 1.0f * stage.effectVolume / 100);
                }
                foreach (PianoSound sound in curNote.sounds)
                {
                    if (stage.pianoVolume > 0)
                        piano.PlayNote(sound.pitch, sound.volume * stage.pianoVolume / 100, stage.musicPlaySpeed / 10.0f,
                            sound.duration, sound.delay);
                }
            }
        }
        else
        {
            soundPlayed = false;
            frameSprite.sprite = null;
        }
        LinkLineUpdate();
        EffectFrameUpdate();
        CircleUpdate();
        WaveUpdate();
        LightUpdate();
        gameObject.transform.localPosition = new Vector3(x, 0.0f, z);
    }
    private void LinkLineUpdate()
    {
        if (curNote.nextLink < 0 || curNote.time <= time || !inRange) { linkLine.SetActive(false); return; }
        linkLine.SetActive(true);
        Note nextNote = stage.chart.notes[curNote.nextLink];
        float x1, z1, x2, z2;
        x2 = Parameters.maximumNoteWidth * curNote.position;
        z2 = Parameters.maximumNoteRange / Parameters.NoteFallTime(stage.chartPlaySpeed) * (curNote.time - time);
        x1 = Parameters.maximumNoteWidth * nextNote.position;
        z1 = Parameters.maximumNoteRange / Parameters.NoteFallTime(stage.chartPlaySpeed) * (nextNote.time - time);
        linkLine.MoveTo(new Vector3(x1, 0, z1 + 32.0f), new Vector3(x2, 0, z2 + 32.0f));
        linkLine.Layer = 1;
    }
    private void EffectFrameUpdate()
    {
        int frame = Mathf.FloorToInt((time - curNote.time) / Parameters.frameSpeed);
        if (frame >= 15 || !inRange) { frameSprite.sprite = null; noteSprite.sprite = null; }
        else if (frame >= 0)
        {
            noteSprite.sprite = null;
            frameSprite.sprite = noteEffectFrames[frame];
            frameSprite.transform.localScale = noteEffectScale * new Vector3(curNote.size, 1.0f, 1.0f);
        }
    }
    private void CircleUpdate()
    {
        float dTime = time - curNote.time;
        if (dTime >= 0.0f && dTime <= Parameters.circleTime)
        {
            float size = Mathf.Pow(dTime / Parameters.circleTime, 0.60f) * Parameters.circleSize;
            float alpha = Mathf.Pow(1.0f - dTime / Parameters.circleTime, 0.33f);
            circleSprite.transform.localScale = size * new Vector3(1.0f, 1.0f, 1.0f);
            circleSprite.color = new Color(0.0f, 0.0f, 0.0f, alpha);
        }
        else
        {
            circleSprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }
    private void WaveUpdate()
    {
        float dTime = time - curNote.time;
        if (dTime >= 0.0f && dTime <= Parameters.waveIncTime)
        {
            float rate = dTime / Parameters.waveIncTime;
            float height = rate * Parameters.waveHeight;
            waveSprite.transform.localScale = curNote.size * new Vector3(Parameters.waveSize, height, height);
            waveSprite.color = new Color(waveColor.r, waveColor.g, waveColor.b, Mathf.Pow(rate, 0.5f));
        }
        else if (dTime > Parameters.waveIncTime && dTime <= Parameters.waveIncTime + Parameters.waveDecTime)
        {
            float rate = (1 - (dTime - Parameters.waveIncTime) / Parameters.waveDecTime);
            float height = rate * Parameters.waveHeight;
            waveSprite.transform.localScale = curNote.size * new Vector3(Parameters.waveSize, height, height);
            waveSprite.color = new Color(waveColor.r, waveColor.g, waveColor.b, Mathf.Pow(rate, 0.5f));
        }
        else
        {
            waveSprite.transform.localScale = Vector3.zero;
            waveSprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }
    private void LightUpdate()
    {
        float dTime = time - curNote.time;
        if (dTime >= 0.0f && dTime <= Parameters.lightIncTime)
        {
            float rate = dTime / Parameters.lightIncTime;
            float height = rate * Parameters.lightHeight;
            lightSprite.transform.localScale = curNote.size * new Vector3(Parameters.lightSize, height, height);
            lightSprite.color = new Color(1.0f, 1.0f, 1.0f, rate);
        }
        else if (dTime > Parameters.lightIncTime && dTime <= Parameters.lightIncTime + Parameters.lightDecTime)
        {
            float rate = (1 - (dTime - Parameters.lightIncTime) / Parameters.lightDecTime);
            float height = rate * Parameters.lightHeight;
            lightSprite.transform.localScale = curNote.size * new Vector3(Parameters.lightSize, height, height);
            lightSprite.color = new Color(1.0f, 1.0f, 1.0f, rate);
        }
        else
        {
            lightSprite.transform.localScale = Vector3.zero;
            lightSprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }
    public void Activate(int noteID, Note note, StageController stageController, PianoSoundsLoader pianoSoundsLoader)
    {
        piano = pianoSoundsLoader;
        stage = stageController;
        if (linkLine == null)
        {
            linkLine = Utility.DrawLineInWorldSpace(Vector3.zero, Vector3.up, Parameters.linkLineColor, 0.035f);
            linkLine.transform.SetParent(stage.linkLineParent);
            linkLine.SetActive(false);
        }
        id = noteID;
        soundPlayed = true;
        curNote = note;
        if (curNote.isLink)
        {
            noteSprite.sprite = slideNoteSprite;
            noteSprite.transform.localScale = slideNoteScale * new Vector3(curNote.size, 1.0f, 1.0f);
            waveColor = new Color(1.0f, 1.0f, 0.6f);
        }
        else if (curNote.sounds.Count > 0)
        {
            noteSprite.sprite = pianoNoteSprite;
            noteSprite.transform.localScale = pianoNoteScale * new Vector3(curNote.size, 1.0f, 1.0f);
            waveColor = Color.black;
        }
        else
        {
            noteSprite.sprite = blankNoteSprite;
            noteSprite.transform.localScale = blankNoteScale * new Vector3(curNote.size, 1.0f, 1.0f);
            waveColor = Color.black;
        }
        frameSprite.sprite = null;
        if (curNote.position > 2.0f || curNote.position < -2.0f) inRange = false; else inRange = true;
        if (!inRange)
        {
            circleSprite.sprite = null;
            waveSprite.sprite = null;
            lightSprite.sprite = null;
        }
        else
        {
            circleSprite.sprite = cirSprite;
            waveSprite.transform.localScale = Vector3.zero;
            waveSprite.sprite = wSprite;
            lightSprite.sprite = lSprite;
        }
        noteSprite.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        Update();
    }
    private void Update()
    {
        time = stage.timeSlider.value;
        CheckForReturn();
        PositionUpdate();
    }
}
