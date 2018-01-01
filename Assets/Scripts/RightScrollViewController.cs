using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RightScrollViewController : MonoBehaviour
{
    public DirectorySelectorController directorySelectorController;
    public GameObject quitScreen; //The quit screen to be activated
    public GameObject scrollView;
    public void OpenQuitScreen()
    {
        quitScreen.SetActive(true);
    }
    public void QuitScreenYes() //The *I'm sure! Quit now!* thing
    {
        FindObjectOfType<ProjectController>().SavePlayerPrefs();
        DragAndDropUnity.Disable();
        Application.Quit();
    }
    public void QuitScreenNo() //The *take me back to my project* thing
    {
        quitScreen.SetActive(false);
    }
    public void OnButtonClick(GameObject subPanel) //Just a simple function to toggle the state of the panel
    {
        subPanel.SetActive(!(subPanel.activeSelf));
    }
    private void Start()
    {
        directorySelectorController = FindObjectOfType<DirectorySelectorController>();
        scrollView.SetActive(false);
        scrollView.SetActive(true);
    }
}
