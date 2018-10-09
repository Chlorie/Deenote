using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageSetter : Window
{
    public static BackgroundImageSetter Instance { get; private set; }
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _transform;
    private int _width;
    private int _height;
    private int _displayWidth;
    private int _displayHeight;
    public enum Position
    {
        Top,
        Bottom,
        Left,
        Right,
        Center
    }
    public enum StretchMode
    {
        Fill,
        FitWidth,
        FitHeight
    }
    private Position _position = Position.Center;
    private StretchMode _stretch = StretchMode.FitHeight;
    public new void Open()
    {
        base.Open();
        LanguageController.Refresh();
    }
    public void SelectFile()
    {
        FileExplorer.SetTagContent("Change background image", "更改背景图");
        FileExplorer.Instance.Open(FileExplorer.Mode.SelectFile, () => SetBackgroundImage(FileExplorer.Result), ".png", ".jpg");
    }
    public void SetBackgroundImage(string path) => StartCoroutine(SetBackgroundImageCoroutine(path));
    private IEnumerator SetBackgroundImageCoroutine(string path)
    {
        Texture2D texture = new Texture2D(0, 0, TextureFormat.DXT1, false);
        Sprite sprite;
        if (!string.IsNullOrWhiteSpace(path))
            using (WWW www = new WWW("file:///" + path))
            {
                yield return www;
                if (!string.IsNullOrEmpty(www.error))
                {
                    sprite = null;
                    MessageBox.Instance.Activate(new[] { "Error", "错误" }, new[] { "Cannot find file " + path, "未找到文件 " + path },
                        new MessageBox.ButtonInfo { texts = new[] { "OK", "好的" } });
                    path = "";
                }
                else
                {
                    www.LoadImageIntoTexture(texture);
                    sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        else
            sprite = null;
        _image.sprite = sprite;
        _image.type = Image.Type.Simple;
        _image.preserveAspect = true;
        _width = texture.width;
        _height = texture.height;
        _displayWidth = Screen.width;
        _displayHeight = Screen.height - 20;
        if (sprite == null)
        {
            _transform.anchorMin = _transform.anchorMax = new Vector2(0.5f, 0.5f);
            _transform.offsetMax = new Vector2((float)_displayWidth / 2, (float)_displayHeight / 2);
            _transform.offsetMin = -_transform.offsetMax;
        }
        SetStretchMode((int)_stretch);
        AppConfig.config.backgroundImage = path;
    }
    public void SetPosition(int value)
    {
        if (_width == 0 || _height == 0) { _position = (Position)value; return; }
        if (_stretch == StretchMode.Fill) return;
        Position position = (Position)value;
        switch (position)
        {
            case Position.Top:
                _transform.anchorMin = _transform.anchorMax = new Vector2(0.5f, 1.0f);
                _transform.offsetMax = new Vector2((float)_displayWidth / 2, 0.0f);
                _transform.offsetMin = new Vector2(-(float)_displayWidth / 2, -_displayHeight);
                break;
            case Position.Bottom:
                _transform.anchorMin = _transform.anchorMax = new Vector2(0.5f, 0.0f);
                _transform.offsetMax = new Vector2((float)_displayWidth / 2, _displayHeight);
                _transform.offsetMin = new Vector2(-(float)_displayWidth / 2, 0.0f);
                break;
            case Position.Left:
                _transform.anchorMin = _transform.anchorMax = new Vector2(0.0f, 0.5f);
                _transform.offsetMax = new Vector2(_displayWidth, (float)_displayHeight / 2);
                _transform.offsetMin = new Vector2(0.0f, -(float)_displayHeight / 2);
                break;
            case Position.Right:
                _transform.anchorMin = _transform.anchorMax = new Vector2(1.0f, 0.5f);
                _transform.offsetMax = new Vector2(0.0f, (float)_displayHeight / 2);
                _transform.offsetMin = new Vector2(-_displayWidth, -(float)_displayHeight / 2);
                break;
            case Position.Center:
                _transform.anchorMin = _transform.anchorMax = new Vector2(0.5f, 0.5f);
                _transform.offsetMax = new Vector2((float)_displayWidth / 2, (float)_displayHeight / 2);
                _transform.offsetMin = -_transform.offsetMax;
                break;
        }
        _position = position;
        AppConfig.config.backgroundPosition = (int)position;
    }
    public void SetStretchMode(int value)
    {
        if (_width == 0 || _height == 0) { _stretch = (StretchMode)value; return; }
        StretchMode stretch = (StretchMode)value;
        if (stretch == StretchMode.Fill)
        {
            _image.preserveAspect = false;
            _transform.anchorMax = new Vector2(1.0f, 1.0f);
            _transform.anchorMin = new Vector2(0.0f, 0.0f);
            _transform.offsetMax = new Vector2();
            _transform.offsetMin = new Vector2();
        }
        else
        {
            _image.preserveAspect = true;
            if (stretch == StretchMode.FitHeight)
            {
                _displayHeight = Screen.height - 20;
                _displayWidth = (int)((float)_displayHeight / _height * _width) + 1;
            }
            else
            {
                _displayWidth = Screen.width;
                _displayHeight = (int)((float)_displayWidth / _width * _height) + 1;
            }
        }
        _stretch = stretch;
        AppConfig.config.backgroundStretch = (int)stretch;
        SetPosition((int)_position);
    }
    protected override void Start()
    {
        base.Start();
        ResolutionController.OnResolutionChange += () => SetStretchMode((int)_stretch);
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of BackgroundImageSetter");
        }
    }
}
