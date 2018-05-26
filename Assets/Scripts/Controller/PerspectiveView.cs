using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveView : Window
{
    public static PerspectiveView Instance { get; private set; }
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
