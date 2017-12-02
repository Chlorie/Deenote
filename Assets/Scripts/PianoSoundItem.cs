using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PianoSoundItem : MonoBehaviour
{
    public PianoSoundEditor editor;
    public float duration = 0.0f;
    public InputField durationField;
    public float delay = 0.0f;
    public InputField delayField;
    public int pitch = 0;
    public Text pitchText;
    public int volume = 0;
    public InputField volumeField;
    public void DurationFieldChanged()
    {
        float value = duration;
        try
        {
            value = System.Convert.ToSingle(durationField.text);
        }
        catch (System.FormatException)
        {
            throw;
        }
        durationField.text = value.ToString("F3");
        duration = value;
    }
    public void DelayFieldChanged()
    {
        float value = delay;
        try
        {
            value = System.Convert.ToSingle(delayField.text);
        }
        catch (System.FormatException)
        {
            throw;
        }
        delayField.text = value.ToString("F3");
        delay = value;
    }
    public void VolumeFieldChanged()
    {
        int value = volume;
        try
        {
            value = System.Convert.ToInt32(volumeField.text);
        }
        catch (System.FormatException)
        {
            throw;
        }
        volumeField.text = value.ToString();
        volume = value;
    }
    public void Initialize(float dur, float del, int pit, int vol, PianoSoundEditor pianoSoundEditor)
    {
        gameObject.SetActive(true);
        editor = pianoSoundEditor;
        duration = dur;
        delay = del;
        pitch = pit;
        volume = vol;
        durationField.text = duration.ToString("F3");
        delayField.text = delay.ToString("F3");
        volumeField.text = volume.ToString();
        int octave = pit / 12 - 2;
        string[] name = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        pitchText.text = name[pit % 12] + octave;
    }
    public void Delete()
    {
        editor.DeleteItem(this);
        editor.itemPool.ReturnObject(this);
    }
}
