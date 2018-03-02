using UnityEngine;
using UnityEngine.UI;

public class BPMCalculatorController : MonoBehaviour
{
    public GameObject canvas;
    public GameObject panel;
    public StageController stage;
    public Text nrText;
    public LocalizedText bpmText, offsetText;
    public Slider slider;
    public AudioSource source;
    private float x, x2, xy, y, y2, lxx, lxy, lyy, r, r2, m, bpm, time;
    //private float offset;
    //private int intOffset;
    private int n;
    //lxx = x2-x^2/n
    //lxy = xy-x*y/n
    //lyy = y2-y^2/n
    //m = lxy/lxx
    //b = (y-mx)/n
    //offset = b%m*1k
    //bpm = 60/m
    //r = lxy/sqrt(lxx*lyy)
    public void Activate()
    {
        CurrentState.ignoreAllInput = true;
        stage.StopPlaying();
        panel.SetActive(true);
        ResetValues();
    }
    public void ResetValues()
    {
        source.clip = stage.musicSource.clip;
        slider.minValue = 0.0f;
        slider.maxValue = source.clip.length;
        slider.value = 0.0f;
        bpmText.SetStrings("Tap Space", "按空格键");
        offsetText.SetStrings("");
        nrText.text = "";
        n = 0;
        x = x2 = xy = y = y2 = lxx = lxy = lyy = r = bpm /*= offset*/ = m = 0.0f;
    }
    public void AddValue()
    {
        x += n;
        x2 += n * n;
        y += time;
        xy += n * time;
        y2 += time * time;
        n++;
        lxx = x2 - x * x / n;
        lxy = xy - x * y / n;
        lyy = y2 - y * y / n;
        if (n == 1) nrText.text = "n = 1";
        if (n > 1)
        {
            m = lxy / lxx;
            //b = (y - m * x) / n;
            bpm = 60.0f / m;
            //offset = b - Mathf.Floor(b / m) * m;
            //intOffset = (int)(offset * 1000);
            r2 = lxy * lxy / lxx / lyy;
            r = Mathf.Sqrt(r2);
            r = (r > 1.0f) ? 1.0f : r;
            bpmText.SetStrings(bpm.ToString());
            //offsetText.text = "Offset: " + intOffset + "ms"; //The offset is like crap, forget about this
            offsetText.SetStrings("");
            nrText.text = "n = " + n + " | r = " + r;
        }
    }
    public void UseValues()
    {
        //stage.editor.fillFromInputField.text = offset.ToString();
        stage.editor.fillFromInputField.text = "0";
        stage.editor.fillToInputField.text = source.clip.length.ToString();
        stage.editor.fillWithBPMInputField.text = bpm.ToString();
        stage.editor.FillFromChange();
        stage.editor.FillToChange();
        stage.editor.FillWithBPMChange();
        Deactive();
    }
    public void Deactive()
    {
        CurrentState.ignoreAllInput = false;
        source.Stop();
        panel.SetActive(false);
    }
    public void Play()
    {
        source.Play();
        source.time = slider.value;
    }
    public void Pause()
    {
        source.Stop();
        source.time = slider.value;
    }
    private void Start()
    {
        canvas.SetActive(false);
        canvas.SetActive(true);
    }
    public void OnSliderValueChange()
    {
        source.time = slider.value;
    }
    private void Update()
    {
        if (!panel.activeInHierarchy) return;
        time = source.time;
        slider.value = time;
        if (time >= source.clip.length) source.Stop();
        if (Utility.DetectKeys(KeyCode.Space)) AddValue();
        if (Utility.DetectKeys(KeyCode.Escape)) Deactive();
    }
}
