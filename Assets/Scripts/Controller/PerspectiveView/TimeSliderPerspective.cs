using UnityEngine;
using UnityEngine.UI;

public class TimeSliderPerspective : MonoBehaviour
{
    public static TimeSliderPerspective Instance { get; private set; }
    [SerializeField] private Slider _displaySlider;
    [SerializeField] private Slider _controlSlider;
    public delegate void UserMoveSliderHandler();
    public event UserMoveSliderHandler UserMoveSliderEvent;
    private void ChangeSliderLength(float length) => _displaySlider.maxValue = _controlSlider.maxValue = length;
    private void ChangeSliderValue(float value) => _displaySlider.value = _controlSlider.value = value;
    public void OnUserMoveSlider()
    {
        AudioPlayer.Instance.Time = _controlSlider.value;
        UserMoveSliderEvent?.Invoke();
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of TimeSliderPerspective");
        }
    }
    private void Start()
    {
        _displaySlider.value = _controlSlider.value = 0.0f;
        AudioPlayer.Instance.AudioClipChangeEvent += ChangeSliderLength;
        AudioPlayer.Instance.AudioTimeChangeEvent += ChangeSliderValue;
    }
}
