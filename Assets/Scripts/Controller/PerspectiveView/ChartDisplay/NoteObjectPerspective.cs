using UnityEngine;

public class NoteObjectPerspective : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _noteSprite;
    [SerializeField] private SpriteRenderer _waveSprite;
    [SerializeField] private SpriteRenderer _circleSprite;
    [SerializeField] private SpriteRenderer _glowSprite;
    [SerializeField] private AudioSource _hitEffectSource;
    private Color _noteColor = new Color(1.0f, 1.0f, 1.0f);
    private Color _waveColor;
    private bool _soundPlayed;
    private int _id;
    private float _x;
    private Note _note;
    public int Id
    {
        get { return _id; }
        set
        {
            _id = value;
            _note = ChartDisplayController.Instance.Chart.notes[value];
            _x = _note.position * Parameters.Params.perspectiveHorizontalScale;
            UpdateNoteProperties();
        }
    }
    public bool IsShown => ChartDisplayController.Instance.NoteShownInPerspectiveView(_note.time);
    public Color NoteColor
    {
        get { return _noteColor; }
        set
        {
            _noteColor = value;
            UpdateColor(transform.localPosition.z);
        }
    }
    private void UpdateNoteProperties()
    {
        Vector3 rawScale = new Vector3(_note.size, 1.0f, 1.0f);
        _waveColor = Color.black;
        _waveSprite.transform.localScale = Vector3.zero;
        _circleSprite.transform.localScale = Vector3.zero;
        _glowSprite.transform.localScale = Vector3.zero;
        if (_note.isLink)
        {
            _noteSprite.sprite = Parameters.Params.slideNoteSprite;
            _noteSprite.transform.localScale = Parameters.Params.slideNoteScale * rawScale;
            _hitEffectSource.volume = 0.5f;
            _waveColor = Parameters.Params.slideNoteWaveColor;
        }
        else if (_note.sounds.Count > 0)
        {
            _noteSprite.sprite = Parameters.Params.pianoNoteSprite;
            _noteSprite.transform.localScale = Parameters.Params.pianoNoteScale * rawScale;
            _hitEffectSource.volume = 1.0f;
        }
        else
        {
            _noteSprite.sprite = Parameters.Params.otherNoteSprite;
            _noteSprite.transform.localScale = Parameters.Params.otherNoteScale * rawScale;
            _hitEffectSource.volume = 1.0f;
        }
    }
    public void Activate()
    {
        gameObject.SetActive(true);
        _soundPlayed = ChartDisplayController.Instance.PerspectiveTime(_note.time) < 0;
        Update();
    }
    private void UpdatePosition()
    {
        float z = ChartDisplayController.Instance.PerspectiveTime(_note.time);
        Vector3 position = transform.localPosition;
        if (z < 0.0f)
        {
            if (!_soundPlayed)
            {
                _hitEffectSource.Play();
                _soundPlayed = true;
            }
            ShowDisappearFrame();
            UpdateWave();
            UpdateCircle();
            UpdateGlow();
            z = 0.0f;
        }
        position.x = _x;
        position.z = z;
        transform.localPosition = position;
        UpdateColor(z);
    }
    private void UpdateColor(float z)
    {
        float alpha = z > Parameters.Params.perspectiveOpaqueDistance
            ? (Parameters.Params.perspectiveMaxDistance - z) / (Parameters.Params.perspectiveMaxDistance -
                Parameters.Params.perspectiveOpaqueDistance)
            : 1.0f;
        Color color = _noteColor;
        color.a = alpha;
        _noteSprite.color = color;
    }
    private void ShowDisappearFrame()
    {
        _noteSprite.transform.localScale = Parameters.Params.disappearingSpriteScale * new Vector3(_note.size, 1.0f, 1.0f);
        int frameIndex = Mathf.FloorToInt((AudioPlayer.Instance.Time - _note.time) /
            Parameters.Params.disappearingSpriteTimePerFrame);
        frameIndex = frameIndex >= 0 ? frameIndex : 0;
        _noteSprite.sprite = frameIndex < 15 ? Parameters.Params.noteDisappearingSprites[frameIndex] : null;
    }
    private void UpdateWave()
    {
        float deltaTime = AudioPlayer.Instance.Time - _note.time;
        if (deltaTime >= 0.0f && deltaTime <= Parameters.Params.waveExpandTime)
        {
            float rate = deltaTime / Parameters.Params.waveExpandTime;
            float height = rate * Parameters.Params.waveMaxScale;
            _waveSprite.transform.localScale = _note.size * new Vector3(Parameters.Params.waveWidth, height, height);
            _waveSprite.color = new Color(_waveColor.r, _waveColor.g, _waveColor.b, Mathf.Pow(rate, 0.5f));
        }
        else if (deltaTime > Parameters.Params.waveExpandTime && deltaTime <= Parameters.Params.waveShrinkTime)
        {
            float rate = 1 - (deltaTime - Parameters.Params.waveExpandTime) / (Parameters.Params.waveShrinkTime - Parameters.Params.waveExpandTime);
            float height = rate * Parameters.Params.waveMaxScale;
            _waveSprite.transform.localScale = _note.size * new Vector3(Parameters.Params.waveWidth, height, height);
            _waveSprite.color = new Color(_waveColor.r, _waveColor.g, _waveColor.b, Mathf.Pow(rate, 0.5f));
        }
        else
        {
            _waveSprite.transform.localScale = Vector3.zero;
            _waveSprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }
    private void UpdateCircle()
    {
        float deltaTime = AudioPlayer.Instance.Time - _note.time;
        if (deltaTime <= Parameters.Params.circleIncreaseTime)
        {
            float alpha = Mathf.Pow(1.0f - deltaTime / Parameters.Params.circleIncreaseTime, 0.33f);
            _circleSprite.transform.localScale = Mathf.Pow(deltaTime / Parameters.Params.circleIncreaseTime, 0.60f) *
                Parameters.Params.circleMaxScale * Vector3.one;
            _circleSprite.color = new Color(0.0f, 0.0f, 0.0f, alpha);
        }
        else
            _circleSprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }
    private void UpdateGlow()
    {
        float size = _note.size;
        float deltaTime = AudioPlayer.Instance.Time - _note.time;
        if (deltaTime >= 0.0f && deltaTime <= Parameters.Params.glowExpandTime)
        {
            float rate = deltaTime / Parameters.Params.glowExpandTime;
            float height = rate * Parameters.Params.glowMaxScale;
            _glowSprite.transform.localScale = size * new Vector3(Parameters.Params.glowWidth, height, height);
            Color glowColor = Parameters.Params.glowColor;
            glowColor.a *= rate;
            _glowSprite.color = glowColor;
        }
        else if (deltaTime > Parameters.Params.glowExpandTime && deltaTime <= Parameters.Params.glowShrinkTime)
        {
            float rate = 1 - (deltaTime - Parameters.Params.glowExpandTime) /
                (Parameters.Params.glowShrinkTime - Parameters.Params.glowExpandTime);
            float height = rate * Parameters.Params.glowMaxScale;
            _glowSprite.transform.localScale = size * new Vector3(Parameters.Params.glowWidth, height, height);
            Color glowColor = Parameters.Params.glowColor;
            glowColor.a *= rate;
            _glowSprite.color = glowColor;
        }
        else
        {
            _glowSprite.transform.localScale = Vector3.zero;
            _glowSprite.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
    }
    public void Update() => UpdatePosition();
}
