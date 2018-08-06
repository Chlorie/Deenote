using System;
using UnityEngine;
using UnityEngine.UI;

public class PerspectiveView : Window
{
    private static readonly string[] LevelTexts = { "Easy", "Normal", "Hard", "Extra" };
    public static PerspectiveView Instance { get; private set; }
    [SerializeField] private TextMesh _floorSongName;
    [SerializeField] private Text _uiSongName;
    [SerializeField] private Text _difficultyText;
    [SerializeField] private Image _difficultyImage;
    [SerializeField] private Text _scoreText;
    [SerializeField] private Image _sliderHandleImage;
    public int CurrentDifficulty { get; private set; } = 4;
    public void SetSongName(string songName) => _floorSongName.text = _uiSongName.text = songName;
    public void SetDifficulty(int difficulty, string level)
    {
        _difficultyText.text = LevelTexts[difficulty] + " LV" + level;
        _difficultyText.color = Parameters.Params.difficultyColors[difficulty];
        Color alphaColor = Parameters.Params.difficultyColors[difficulty];
        alphaColor.a = 0.5f;
        _sliderHandleImage.color = alphaColor;
        _difficultyImage.sprite = Parameters.Params.difficultySprites[difficulty];
        CurrentDifficulty = difficulty;
    }
    // Expecting an integral value score, with a maximum of 10000 meaning 100.00%
    public void SetScore(int score) => _scoreText.text = (score / 100) + "." + (score % 100).ToString("D2") + " %";
    protected override void Start()
    {
        base.Start();
        operations.Add(new Operation
        {
            callback = AudioPlayer.Instance.TogglePlayState,
            shortcut = new Shortcut { key = KeyCode.Return }
        });
        operations.Add(new Operation
        {
            callback = () => { AudioPlayer.Instance.Time = 0; },
            shortcut = new Shortcut { key = KeyCode.Home }
        });
        operations.Add(new Operation
        {
            callback = () => { AudioPlayer.Instance.Time = AudioPlayer.Instance.Length; },
            shortcut = new Shortcut { key = KeyCode.End }
        });
    }
    public new void Open()
    {
        base.Open();
        LanguageController.Refresh();
    }
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
