using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XGrid
{
    public UILine far;
    public UILine near;
    public XGrid()
    {
        near = Utility.DrawLineInWorldSpace(new Vector3(0, 0, 32 + Parameters.alpha1NoteRange), new Vector3(0, 0, 32), new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f, 0.75f), Utility.cylinder, 3);
        far = Utility.DrawLineInWorldSpace(new Vector3(0, 0, 32 + Parameters.maximumNoteRange), new Vector3(0, 0, 32 + Parameters.alpha1NoteRange), new Color(42 / 255.0f, 42 / 255.0f, 42 / 255.0f, 0.75f), Utility.cylinderAlpha, 3);
        near.rectTransform.SetParent(Utility.xGridParent);
        far.rectTransform.SetParent(Utility.xGridParent);
        SetActive(false);
    }
    public void MoveTo(float x)
    {
        Utility.MoveLineInWorldSpace(near, new Vector3(x, 0, 32 + Parameters.alpha1NoteRange), new Vector3(x, 0, 32));
        Utility.MoveLineInWorldSpace(far, new Vector3(x, 0, 32 + Parameters.maximumNoteRange), new Vector3(x, 0, 32 + Parameters.alpha1NoteRange));
    }
    public void SetActive(bool status)
    {
        near.gameObject.SetActive(status);
        far.gameObject.SetActive(status);
    }
}
