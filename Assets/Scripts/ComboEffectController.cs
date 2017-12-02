using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComboEffectController : MonoBehaviour
{
    public StageController stage = null;
    public Image charming;
    public Image strike;
    public Image shockWave;
    public Text number;
    public Text shadow;
    private float time;
    private float noteTime;
    private int combo;
    private void NoComboEffect()
    {
        number.text = shadow.text = "";
        charming.rectTransform.offsetMax = new Vector2(-4.0f, 0.0f);
        charming.rectTransform.offsetMin = new Vector2(-124.0f, 0.0f);
        strike.rectTransform.offsetMax = new Vector2(0.0f, 0.0f);
        strike.rectTransform.offsetMin = new Vector2(0.0f, 0.0f);
        shockWave.rectTransform.localScale = Vector3.zero;
    }
    private void NumberAndShadow()
    {
        float shadowSize = 70.0f;
        float dTime = time - noteTime;
        float shadowAlpha = 1.0f;
        float numberGrey = 1.0f;
        number.text = shadow.text = "" + combo;
        //Number
        if (dTime > Parameters.noNumberFrameLength + Parameters.numberWhiteToBlackTime)
            numberGrey = 0.0f;
        else if (dTime > Parameters.noNumberFrameLength && dTime < Parameters.noNumberFrameLength + Parameters.numberWhiteToBlackTime)
        {
            float rate = (dTime - Parameters.noNumberFrameLength) / Parameters.numberWhiteToBlackTime;
            numberGrey = Mathf.Pow(1 - rate, 0.67f);
        }
        else if (dTime > 0 && dTime <= Parameters.noNumberFrameLength)
        {
            number.text = "";
            numberGrey = 0.0f;
        }
        //Shadow
        if (dTime > Parameters.shadowMaxTime)
        {
            shadowAlpha = Parameters.shadowMinAlpha;
            shadowSize = 120.0f;
        }
        else if (dTime > 0 && dTime <= Parameters.shadowMaxTime)
        {
            float rate = dTime / Parameters.shadowMaxTime;
            shadowAlpha = Parameters.shadowMinAlpha * rate + (1 - rate);
            shadowSize = 70.0f + 50.0f * rate;
        }
        shadow.fontSize = (int)shadowSize;
        shadow.color = new Color(0.0f, 0.0f, 0.0f, shadowAlpha);
        number.color = new Color(numberGrey, numberGrey, numberGrey, 1.0f);
    }
    private void ShockWaveAndStrike()
    {
        float dTime = time - noteTime;
        float alpha = 1.0f;
        //shock wave
        if (dTime > Parameters.shockWaveMaxTime)
            shockWave.rectTransform.localScale = Vector3.zero;
        else
        {
            float rate = dTime / Parameters.shockWaveMaxTime;
            float scale = 1.0f + 2.0f * rate;
            if (rate > 0.5f) alpha = 2 * (1.0f - rate);
            shockWave.rectTransform.localScale = new Vector3(scale, scale, scale);
        }
        shockWave.color = new Color(1.0f, 1.0f, 1.0f, alpha);
        //strike
        if (dTime > Parameters.strikeDisappearTime)
        {
            strike.rectTransform.offsetMax = new Vector2(0.0f, 0.0f);
            strike.rectTransform.offsetMin = new Vector2(0.0f, 0.0f);
        }
        else
        {
            float rate = dTime / Parameters.strikeDisappearTime;
            strike.rectTransform.offsetMax = new Vector2(0.0f - rate * 72.0f, 60.5f);
            strike.rectTransform.offsetMin = new Vector2(-72.0f - rate * 72.0f, 25.5f);
        }
    }
    private void CharmingSize()
    {
        float size = 50.0f;
        float dTime = time - noteTime;
        float alpha = 1.0f;
        if (dTime > Parameters.charmingIncTime + Parameters.charmingDecTime)
            size = 0.0f;
        else if (dTime > Parameters.charmingIncTime && dTime <= Parameters.charmingIncTime + Parameters.charmingDecTime)
        {
            float rate = (dTime - Parameters.charmingIncTime) / Parameters.charmingDecTime;
            size = 55.0f - rate * 10.0f;
            if (rate > 0.5f) alpha = 2 * (1.0f - rate);
        }
        else if (dTime > 0.0f && dTime <= Parameters.charmingIncTime)
        {
            float rate = dTime / Parameters.charmingIncTime;
            size = 45.0f + rate * 10.0f;
        }
        charming.rectTransform.offsetMax = new Vector2(-4.0f, size / 2);
        charming.rectTransform.offsetMin = new Vector2(-124.0f, -size / 2);
        charming.color = new Color(1.0f, 1.0f, 1.0f, alpha);
    }
    private void Start()
    {
        NoComboEffect();
    }
    private void Update()
    {
        if (stage == null) return;
        if (stage.prevNoteID >= 0) combo = stage.inGameNoteIDs[stage.prevNoteID] + 1;
        else combo = 0;
        if (combo < 5)
            NoComboEffect();
        else
        {
            time = stage.timeSlider.value;
            noteTime = stage.chart.notes[stage.prevNoteID].time;
            NumberAndShadow();
            ShockWaveAndStrike();
            CharmingSize();
        }
    }
}
