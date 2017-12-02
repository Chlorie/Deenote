using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FileButtonPressed : MonoBehaviour
{
    private DirectorySelectorController controller;
    private void Start()
    {
        controller = FindObjectOfType<DirectorySelectorController>();
    }
    public void ButtonPressed()
    {
        controller.ChangeSelectedItemText(transform.Find("FileText").gameObject.GetComponent<Text>().text);
        controller.confirmButton.interactable = true;
    }
}
