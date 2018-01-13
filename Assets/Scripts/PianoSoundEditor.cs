using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoSoundEditor : MonoBehaviour
{
    public PianoSoundsLoader sounds;
    public PianoSoundItemPool itemPool;
    private List<PianoSoundItem> items = new List<PianoSoundItem>();
    private EditorController editor;
    public PianoSoundsLoader piano;
    public void SelectKey(int key)
    {
        sounds.PlayNote(key, 95, 1.0f);
        PianoSoundItem item = itemPool.GetObject();
        item.Initialize(0.0f, 0.0f, key, 0, this);
        items.Add(item);
    }
    public void Activate(EditorController edit, List<PianoSound> sounds)
    {
        editor = edit;
        CurrentState.ignoreAllInput = true;
        foreach (PianoSound sound in sounds)
        {
            PianoSoundItem item = itemPool.GetObject();
            item.Initialize(sound.duration, sound.delay, sound.pitch, sound.volume, this);
            items.Add(item);
        }
    }
    public void DeleteItem(PianoSoundItem item)
    {
        items.Remove(item);
    }
    public void Deactivate(bool save)
    {
        CurrentState.ignoreAllInput = false;
        if (save)
        {
            List<PianoSound> pianoSounds = new List<PianoSound>();
            foreach (PianoSoundItem item in items)
            {
                PianoSound pianoSound = new PianoSound
                {
                    delay = item.delay,
                    duration = item.duration,
                    pitch = item.pitch,
                    volume = item.volume
                };
                pianoSounds.Add(pianoSound);
                itemPool.ReturnObject(item);
            }
            while (items.Count > 0) items.RemoveAt(0);
            editor.PianoSoundFinishedEdit(pianoSounds);
        }
        else
        {
            foreach (PianoSoundItem item in items) itemPool.ReturnObject(item);
            while (items.Count > 0) items.RemoveAt(0);
            if (editor != null) editor.PianoSoundFinishedEdit(null);
        }
    }
    public void PlaySound()
    {
        List<PianoSound> pianoSounds = new List<PianoSound>();
        foreach (PianoSoundItem item in items)
        {
            PianoSound pianoSound = new PianoSound
            {
                delay = item.delay,
                duration = item.duration,
                pitch = item.pitch,
                volume = item.volume
            };
            pianoSounds.Add(pianoSound);
        }
        foreach (PianoSound sound in pianoSounds)
            if (editor.stage.pianoVolume > 0)
                piano.PlayNote(sound.pitch, sound.volume * editor.stage.pianoVolume / 100, editor.stage.musicPlaySpeed / 10.0f,
                    sound.duration, sound.delay);
    }
    public void Start()
    {
        itemPool.Initialize();
    }
}
