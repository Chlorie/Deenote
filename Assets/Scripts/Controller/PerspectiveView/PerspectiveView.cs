using UnityEngine;
using UnityEngine.UI;

public class PerspectiveView : Window
{
    private static readonly string[] _levelTexts = new[] { "Easy", "Normal", "Hard", "Extra" };
    public static PerspectiveView Instance { get; private set; }
    [SerializeField] private TextMesh _floorSongName;
    [SerializeField] private Text _uiSongName;
    [SerializeField] private Text _difficultyText;
    [SerializeField] private Image _difficultyImage;
    [SerializeField] private Text _scoreText;
    [SerializeField] private Image _sliderHandleImage;
    public int CurrentDifficulty { get; private set; } = 4;
    public void SetSongName(string name) => _floorSongName.text = _uiSongName.text = name;
    public void SetDifficulty(int difficulty, string level)
    {
        _difficultyText.text = _levelTexts[difficulty] + " LV" + level;
        _difficultyText.color = uiParameters.difficultyColors[difficulty];
        Color alphaColor = uiParameters.difficultyColors[difficulty];
        alphaColor.a = 0.5f;
        _sliderHandleImage.color = alphaColor;
        _difficultyImage.sprite = uiParameters.difficultySprites[difficulty];
        CurrentDifficulty = difficulty;
    }
    // Expecting an integral value score, with a maximum of 10000 meaning 100.00%
    public void SetScore(int score) => _scoreText.text = (score / 100) + "." + (score % 100).ToString("D2") + " %";
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of PerspectiveView");
        }
    }
}
