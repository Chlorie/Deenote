using UnityEngine;

public class ResolutionController : MonoBehaviour
{
    public static ResolutionController Instance { get; private set; }
    public delegate void ResolutionChangeHandler();
    public static event ResolutionChangeHandler OnResolutionChange = null;
    private int lastWidth = 0;
    private int lastHeight = 0;
    public static int Width => Instance.lastWidth;
    public static int Height => Instance.lastHeight;
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
        if (lastWidth != Screen.width || lastHeight != Screen.height) OnResolutionChange?.Invoke();
        lastWidth = Screen.width; lastHeight = Screen.height;
    }
}
