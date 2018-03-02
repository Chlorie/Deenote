using UnityEngine;
using UnityEngine.UI;

public class SubDirectoryButtonPressed : MonoBehaviour
{
    private DirectorySelectorController controller;
    private void Start()
    {
        controller = FindObjectOfType<DirectorySelectorController>();
    }
    public void ButtonPressed()
    {
        controller.ForwardToSubDirectory(transform.Find("SubDirectoryText").gameObject.GetComponent<Text>());
    }
}
