using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChartProperties
{
    public static void GetInGameNoteIds(Chart chart, out List<int> noteIDs)
    {
        int id = -1;
        noteIDs = new List<int>();
        for (int i = 0; i < chart.notes.Count; i++)
        {
            Note note = chart.notes[i];
            float pos = note.position;
            if (pos <= 2.0f && pos >= -2.0f) id++;
            noteIDs.Add(id);
        }
    }
    public static void GetCollidedNotes(Chart chart, out List<bool> collided)
    {
        int noteCount = chart.notes.Count;
        collided = new List<bool>(new bool[noteCount]);
        for (int i = 0; i < noteCount; i++)
            for (int j = i + 1; j < noteCount; j++)
            {
                if (chart.notes[j].time - chart.notes[i].time > 1e-3f) break;
                if (chart.notes[j].isLink) continue;
                if (Mathf.Abs(chart.notes[i].position - chart.notes[j].position) < 0.01f)
                {
                    collided[i] = collided[j] = true;
                    break;
                }
            }
    }
}
