using UnityEngine;
using UnityEngine.UI;

public class ComboEffectPerspective : MonoBehaviour
{
    public static ComboEffectPerspective Instance { get; private set; }
    [SerializeField] private Image _charming;
    [SerializeField] private Image _strike;
    [SerializeField] private Image _shockWave;
    [SerializeField] private Text _number;
    [SerializeField] private Text _shadow;
    private float _noteTime;
    private float _time;
    private int _combo;
    public void UpdateCombo(int combo, float time)
    {
        _combo = combo;
        _noteTime = time;
        if (_combo < 5)
            NoComboEffect();
        else
        {
            _time = AudioPlayer.Instance.Time;
            UpdateNumberAndShadow();
            UpdateShockWaveAndStrike();
            UpdateCharmingSize();
        }
    }
    private void NoComboEffect()
    {
        _number.text = _shadow.text = "";
        _charming.rectTransform.offsetMax = new Vector2(-4.0f, 0.0f);
        _charming.rectTransform.offsetMin = new Vector2(-124.0f, 0.0f);
        _strike.rectTransform.offsetMax = new Vector2(0.0f, 0.0f);
        _strike.rectTransform.offsetMin = new Vector2(0.0f, 0.0f);
        _shockWave.rectTransform.localScale = Vector3.zero;
    }
    private void UpdateNumberAndShadow()
    {
        float shadowSize = 70.0f;
        float dTime = _time - _noteTime;
        float shadowAlpha = 1.0f;
        float numberGrey = 1.0f;
        _number.text = _shadow.text = _combo.ToString();
        // Number
        if (dTime > Parameters.Params.comboNoNumberLength + Parameters.Params.comboNumberBlackOutTime)
            numberGrey = 0.0f;
        else if (dTime > Parameters.Params.comboNoNumberLength && dTime < Parameters.Params.comboNoNumberLength + Parameters.Params.comboNumberBlackOutTime)
        {
            float rate = (dTime - Parameters.Params.comboNoNumberLength) / Parameters.Params.comboNumberBlackOutTime;
            numberGrey = Mathf.Pow(1 - rate, 0.67f);
        }
        else if (dTime > 0 && dTime <= Parameters.Params.comboNoNumberLength)
        {
            _number.text = "";
            numberGrey = 0.0f;
        }
        // Shadow
        if (dTime > Parameters.Params.comboShadowMaxTime)
        {
            shadowAlpha = Parameters.Params.comboShadowMinAlpha;
            shadowSize = 120.0f;
        }
        else if (dTime > 0 && dTime <= Parameters.Params.comboShadowMaxTime)
        {
            float rate = dTime / Parameters.Params.comboShadowMaxTime;
            shadowAlpha = Parameters.Params.comboShadowMinAlpha * rate + (1 - rate);
            shadowSize = 70.0f + 50.0f * rate;
        }
        _shadow.fontSize = (int)shadowSize;
        _shadow.color = new Color(0.0f, 0.0f, 0.0f, shadowAlpha);
        _number.color = new Color(numberGrey, numberGrey, numberGrey, 1.0f);
    }
    private void UpdateShockWaveAndStrike()
    {
        float dTime = _time - _noteTime;
        float alpha = 1.0f;
        // Shock wave
        if (dTime > Parameters.Params.comboShockWaveMaxTime)
            _shockWave.rectTransform.localScale = Vector3.zero;
        else
        {
            float rate = dTime / Parameters.Params.comboShockWaveMaxTime;
            float scale = 1.0f + 2.0f * rate;
            if (rate > 0.5f) alpha = 2 * (1.0f - rate);
            _shockWave.rectTransform.localScale = new Vector3(scale, scale, scale);
        }
        _shockWave.color = new Color(1.0f, 1.0f, 1.0f, alpha);
        // Strike
        if (dTime > Parameters.Params.comboStrikeShowTime)
        {
            _strike.rectTransform.offsetMax = new Vector2(0.0f, 0.0f);
            _strike.rectTransform.offsetMin = new Vector2(0.0f, 0.0f);
        }
        else
        {
            float rate = dTime / Parameters.Params.comboStrikeShowTime;
            _strike.rectTransform.offsetMax = new Vector2(0.0f - rate * 72.0f, 60.5f);
            _strike.rectTransform.offsetMin = new Vector2(-72.0f - rate * 72.0f, 25.5f);
        }
    }
    private void UpdateCharmingSize()
    {
        float size = 50.0f;
        float dTime = _time - _noteTime;
        float alpha = 1.0f;
        if (dTime > Parameters.Params.comboCharmingExpandTime + Parameters.Params.comboCharmingShrinkTime)
            size = 0.0f;
        else if (dTime > Parameters.Params.comboCharmingExpandTime && dTime <= Parameters.Params.comboCharmingExpandTime + Parameters.Params.comboCharmingShrinkTime)
        {
            float rate = (dTime - Parameters.Params.comboCharmingExpandTime) / Parameters.Params.comboCharmingShrinkTime;
            size = 55.0f - rate * 10.0f;
            if (rate > 0.5f) alpha = 2 * (1.0f - rate);
        }
        else if (dTime > 0.0f && dTime <= Parameters.Params.comboCharmingExpandTime)
        {
            float rate = dTime / Parameters.Params.comboCharmingExpandTime;
            size = 45.0f + rate * 10.0f;
        }
        _charming.rectTransform.offsetMax = new Vector2(-4.0f, size / 2);
        _charming.rectTransform.offsetMin = new Vector2(-124.0f, -size / 2);
        _charming.color = new Color(1.0f, 1.0f, 1.0f, alpha);
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ComboEffectPerspective");
        }
    }
}
