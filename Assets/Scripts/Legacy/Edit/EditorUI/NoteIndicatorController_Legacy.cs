using UnityEngine;

public class NoteIndicatorController_Legacy : MonoBehaviour
{
    public int id;
    public SpriteRenderer noteSprite;
    public Sprite pianoNoteSprite;
    public Sprite blankNoteSprite;
    public Sprite slideNoteSprite;
    public EditorController_Legacy editor;
    private Note note;
    private Note nextLink;
    private const float PianoNoteScale = 7.0f;
    private const float BlankNoteScale = 4.5f;
    private const float SlideNoteScale = 4.5f;
    private float musicLength;
    private float time;
    public Line linkLine;
    public float placeTime;
    public float placePos;
    public Note Note => new Note
    {
        isLink = note.isLink,
        prevLink = note.prevLink,
        nextLink = note.nextLink,
        position = placePos,
        time = placeTime,
        shift = note.shift,
        size = note.size,
        sounds = note.sounds
    };
    public void Initialize(EditorController_Legacy controller, Note cur, Note next, float length)
    {
        editor = controller;
        if (linkLine == null)
        {
            linkLine = Utility.DrawLineInWorldSpace(Vector3.zero, Vector3.up, Parameters.linkLineColor, 0.035f, 0.4f);
            linkLine.transform.SetParent(editor.stage.linkLineParent);
        }
        linkLine.SetActive(false);
        gameObject.SetActive(true);
        note = cur;
        nextLink = next;
        musicLength = length;
        if (cur.isLink)
        {
            noteSprite.sprite = slideNoteSprite;
            noteSprite.transform.localScale = SlideNoteScale * new Vector3(cur.size, 1.0f, 1.0f);
        }
        else if (cur.sounds.Count > 0)
        {
            noteSprite.sprite = pianoNoteSprite;
            noteSprite.transform.localScale = PianoNoteScale * new Vector3(cur.size, 1.0f, 1.0f);
        }
        else
        {
            noteSprite.sprite = blankNoteSprite;
            noteSprite.transform.localScale = BlankNoteScale * new Vector3(cur.size, 1.0f, 1.0f);
        }
    }
    public void Move(float timeOffset, float posOffset, float stageTime)
    {
        float curTime = note.time + timeOffset;
        if (curTime > musicLength) curTime = musicLength;
        if (!InStage(note.position))
        {
            noteSprite.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            placePos = note.position;
            placeTime = curTime;
            return;
        }
        time = stageTime;
        float curPos = note.position + posOffset;
        float nextPos = nextLink.position + (InStage(nextLink.position) ? posOffset : 0.0f);
        float nextTime = nextLink.time + timeOffset;
        if (curPos > 2.0f) curPos = 2.0f;
        if (curPos < -2.0f) curPos = -2.0f;
        if (nextPos > 2.0f && InStage(nextLink.position)) nextPos = 2.0f;
        if (nextPos < -2.0f && InStage(nextLink.position)) nextPos = -2.0f;
        if (nextTime > musicLength) nextTime = musicLength;
        placePos = curPos;
        placeTime = curTime;
        float x = Parameters.maximumNoteWidth * curPos;
        float z = Parameters.maximumNoteRange / Parameters.NoteFallTime(editor.stage.chartPlaySpeed) * (curTime - time);
        float x2 = Parameters.maximumNoteWidth * nextPos;
        float z2 = Parameters.maximumNoteRange / Parameters.NoteFallTime(editor.stage.chartPlaySpeed) * (nextTime - time);
        float alpha = z < Parameters.alpha1NoteRange ? 1.0f : (Parameters.maximumNoteRange - z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
        noteSprite.color = new Color(1.0f, 1.0f, 1.0f, alpha * 0.4f);
        gameObject.transform.localPosition = new Vector3(x, 0.0f, z);
        if (note.isLink)
        {
            linkLine.SetActive(true);
            linkLine.MoveTo(new Vector3(x, 0.0f, z + 32.0f), new Vector3(x2, 0.0f, z2 + 32.0f));
            linkLine.Layer = 1;
            linkLine.AlphaMultiplier = 0.4f;
        }
        else
            linkLine.SetActive(false);
    }
    public void NoColor()
    {
        noteSprite.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        linkLine.SetActive(false);
    }
    private bool InStage(float position)
    {
        if (position > 2.0f || position < -2.0f)
            return false;
        else
            return true;
    }
}
