using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteIndicatorController : MonoBehaviour
{
    public int id;
    public SpriteRenderer noteSprite;
    public Sprite pianoNoteSprite;
    public Sprite blankNoteSprite;
    public Sprite slideNoteSprite;
    public EditorController editor;
    private Note note;
    private Note nextLink;
    private float pianoNoteScale = 7.0f;
    private float blankNoteScale = 4.5f;
    private float slideNoteScale = 4.5f;
    private float musicLength;
    private float time;
    public LinkLine linkLine;
    public float placeTime;
    public float placePos;
    public void Initialize(EditorController controller, Note cur, Note next, float length)
    {
        if (linkLine == null) linkLine = new LinkLine();
        linkLine.SetActive(false);
        gameObject.SetActive(true);
        editor = controller;
        note = cur;
        nextLink = next;
        musicLength = length;
        if (cur.isLink)
        {
            noteSprite.sprite = slideNoteSprite;
            noteSprite.transform.localScale = slideNoteScale * new Vector3(cur.size, 1.0f, 1.0f);
        }
        else if (cur.sounds.Count > 0)
        {
            noteSprite.sprite = pianoNoteSprite;
            noteSprite.transform.localScale = pianoNoteScale * new Vector3(cur.size, 1.0f, 1.0f);
        }
        else
        {
            noteSprite.sprite = blankNoteSprite;
            noteSprite.transform.localScale = blankNoteScale * new Vector3(cur.size, 1.0f, 1.0f);
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
        float x, z, x2, z2;
        x = Parameters.maximumNoteWidth * curPos;
        z = Parameters.maximumNoteRange / Parameters.NoteFallTime(editor.stage.chartPlaySpeed) * (curTime - time);
        x2 = Parameters.maximumNoteWidth * nextPos;
        z2 = Parameters.maximumNoteRange / Parameters.NoteFallTime(editor.stage.chartPlaySpeed) * (nextTime - time);
        float alpha = z < Parameters.alpha1NoteRange ? 1.0f : (Parameters.maximumNoteRange - z) / (Parameters.maximumNoteRange - Parameters.alpha1NoteRange);
        noteSprite.color = new Color(1.0f, 1.0f, 1.0f, alpha * 0.4f);
        gameObject.transform.localPosition = new Vector3(x, 0.0f, z);
        if (note.isLink)
        {
            linkLine.SetActive(true);
            linkLine.MoveTo(new Vector3(x, 0.0f, z), new Vector3(x2, 0.0f, z2));
            linkLine.AlphaMultiply(0.4f);
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
