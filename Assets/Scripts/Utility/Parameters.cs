using UnityEngine;

public class Parameters : MonoBehaviour
{
    public static Parameters Instance { get; private set; }
    public UIParameters parameters;
    public static UIParameters Params => Instance.parameters;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of Parameters");
        }
    }
}
