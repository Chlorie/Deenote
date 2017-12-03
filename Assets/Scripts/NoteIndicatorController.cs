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
    private float pianoNoteScale = 7.0f;
    private float blankNoteScale = 4.5f;
    private float slideNoteScale = 4.5f;
    private bool inRange = true;
    private LinkLine linkLine;
    public void Return()
    {
        gameObject.SetActive(false);
        linkLine.SetActive(false);

    }

}