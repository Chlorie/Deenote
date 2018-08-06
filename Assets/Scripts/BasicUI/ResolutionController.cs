using UnityEngine;

public class ResolutionController : MonoBehaviour
{
    public static ResolutionController Instance { get; private set; }
    public delegate void ResolutionChangeHandler();
    public static event ResolutionChangeHandler OnResolutionChange = null;
    private int _lastWidth = 0;
    private int _lastHeight = 0;
    public static int Width => Instance._lastWidth;
    public static int Height => Instance._lastHeight;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of ResolutionController");
        }
    }
    private void Update()
    {
        if (_lastWidth != Screen.width || _lastHeight != Screen.height) OnResolutionChange?.Invoke();
        _lastWidth = Screen.width; _lastHeight = Screen.height;
    }
}
