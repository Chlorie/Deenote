using UnityEngine;

public class LightEffectPerspective : MonoBehaviour
{
    public static LightEffectPerspective Instance { get; private set; }
    private bool _isActive;
    public bool IsActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (value) return;
            Vector3 scale = _lightEffectMask.localScale;
            scale.y = _shift;
            _lightEffectMask.localScale = scale;
        }
    }
    [SerializeField] private RectTransform _lightEffectMask;
    private float _amplitude;
    private float _shift;
    private float _lightMaskMaxScale;
    private float _angularFrequency;
    // TODO: Add app config for isActive
    private void Update()
    {
        if (!_isActive) return;
        Vector3 scale = _lightEffectMask.localScale;
        float yScale = _shift + _amplitude * Mathf.Sin(_angularFrequency * Time.time);
        scale.y = yScale;
        _lightEffectMask.localScale = scale;
    }
    private void Start()
    {
        float lightMaskMinScale = Parameters.Params.lightMaskMinScale;
        float lightMaskMaxScale = Parameters.Params.lightMaskMaxScale;
        _amplitude = (lightMaskMaxScale - lightMaskMinScale) / 2;
        _shift = (lightMaskMaxScale + lightMaskMinScale) / 2;
        _angularFrequency = Parameters.Params.lightEffectAngularFrequency;
        IsActive = false;
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of LightEffectPerspective");
        }
    }
}
