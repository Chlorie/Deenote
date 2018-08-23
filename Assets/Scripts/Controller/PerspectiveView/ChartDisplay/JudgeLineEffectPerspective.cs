using UnityEngine;

public class JudgeLineEffectPerspective : MonoBehaviour
{
    public static JudgeLineEffectPerspective Instance { get; private set; }
    [SerializeField] private SpriteRenderer _sprite;
    public void ChangeScale(float time)
    {
        float alpha;
        if (LightEffectPerspective.Instance.IsActive)
            alpha = 1.0f + 1.0f * Mathf.Sin(2 * Time.time);
        else
            alpha = 1.0f;
        float deltaTime = AudioPlayer.Instance.Time - time;
        if (deltaTime > 0 && deltaTime <= Parameters.Params.judgeLineEffectShrinkTime)
        {
            float rate = 1 - deltaTime / Parameters.Params.judgeLineEffectShrinkTime;
            rate = Mathf.Pow(rate, 0.5f);
            alpha = (8.0f * rate > alpha) ? (6.0f * rate) : alpha;
        }
        _sprite.color = new Color(1.0f, 1.0f, 1.0f, alpha / 6.0f);
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of JudgeLineEffectPerspective");
        }
    }
}
