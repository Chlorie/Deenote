using UnityEngine;

public class JudgeLineEffectController : MonoBehaviour
{
    private StageController stage;
    public SpriteRenderer sprite;
    private void ChangeScale()
    {
        float alpha;
        if (stage.lightEffectState)
            alpha = 1.0f + 1.0f * Mathf.Sin(2 * Time.time);
        else
            alpha = 1.0f;
        if (stage.chart != null && stage.stageActivated)
        {
            Chart chart = stage.chart;
            float time = stage.timeSlider.value;
            if (stage.prevNoteID >= 0)
            {
                float dTime = time - chart.notes[stage.prevNoteID].time;
                if (dTime > 0 && dTime <= Parameters.jlEffectDecTime)
                {
                    float rate = 1 - dTime / Parameters.jlEffectDecTime;
                    rate = Mathf.Pow(rate, 0.50f);
                    alpha = (8.0f * rate > alpha) ? (6.0f * rate) : alpha;
                }
            }
        }
        sprite.color = new Color(1.0f, 1.0f, 1.0f, alpha / 6.0f);
    }
    private void Start()
    {
        stage = FindObjectOfType<StageController>();
    }
    private void Update()
    {
        ChangeScale();
    }
}
