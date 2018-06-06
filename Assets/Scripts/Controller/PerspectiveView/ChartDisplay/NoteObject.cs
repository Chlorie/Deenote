using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    [SerializeField] private readonly static Sprite[] frames;
    private Note _note;
    public void SetNote(Note note)
    {
        _note = note;
    }
    public void UpdateState()
    {

    }
}
