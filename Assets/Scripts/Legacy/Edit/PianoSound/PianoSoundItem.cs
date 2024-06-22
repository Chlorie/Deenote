using UnityEngine;
using UnityEngine.UI;

public class PianoSoundItem : MonoBehaviour
{
    public PianoSoundEditor editor;
    public float duration;
    public InputField durationField;
    public float delay;
    public InputField delayField;
    public int pitch;
    public Text pitchText;
    public int volume;
    public InputField volumeField;
    public void DurationFieldChanged()
    {
        float value = System.Convert.ToSingle(durationField.text);
        durationField.text = value.ToString("F3");
        duration = value;
    }
    public void DelayFieldChanged()
    {
        float value = System.Convert.ToSingle(delayField.text);
        delayField.text = value.ToString("F3");
        delay = value;
    }
    public void VolumeFieldChanged()
    {
        int value = System.Convert.ToInt32(volumeField.text);
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
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        pitchText.text = noteNames[pit % 12] + octave;
    }
    public void Delete()
    {
        editor.DeleteItem(this);
        editor.itemPool.ReturnObject(this);
    }
}
