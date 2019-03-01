using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : Window
{
    public static PlayerSettings Instance { get; private set; }
    [SerializeField] private Toggle _vSyncToggle;
    private bool _vSyncOn;
    public bool VSyncOn
    {
        get => _vSyncOn;
        set
        {
            _vSyncOn = value;
            QualitySettings.vSyncCount = value ? 1 : 0;
            _vSyncToggle.isOn = value;
        }
    }
    public new void Open() => base.Open();
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of PlayerSettings");
        }
    }
}
