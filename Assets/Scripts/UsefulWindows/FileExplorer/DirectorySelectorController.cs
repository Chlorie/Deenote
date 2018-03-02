using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DirectorySelectorController : MonoBehaviour
{
    public LocalizedText titleText;
    public Text directoryText;
    public Text selectedItemText;
    public InputField directoryInputField;
    public InputField fileNameInputField;
    public DirectoryInfo currentDirectory;
    public GameObject scrollView;
    public GameObject directorySelectorCanvas;
    public GameObject directorySeletorWindow;
    public Transform scrollViewContent;
    public Button fileButtonPrefab;
    public Button subDirectoryButtonPrefab;
    public Button confirmButton;
    public string selectedItemFullName;
    public bool selectedItemType; // true for directory - false for file
    public bool fileNameNeeded;
    public string fileName;
    private string[] extensions = { };
    private List<Button> fileButtons = new List<Button>();
    private List<Button> subDirectoryButtons = new List<Button>();
    private ProjectController projectController;
    public delegate void CallBack();
    private bool callRoutine = false;
    private CallBack callBack = null;
    private IEnumerator callBackRoutine = null;
    public void BackToParentDirectory()
    {
        if (currentDirectory.Parent != null)
        {
            currentDirectory = currentDirectory.Parent;
            directoryInputField.text = currentDirectory.FullName;
            RemoveAndGenerateButtons();
            if (extensions.GetLength(0) == 0 && !fileNameNeeded)
            {
                ChangeSelectedItemText(currentDirectory.Name);
                confirmButton.interactable = true;
            }
            else if (extensions.GetLength(0) != 0)
            {
                ChangeSelectedItemText("");
                confirmButton.interactable = false;
            }
        }
    }
    public void ForwardToSubDirectory(Text subDirectoryText)
    {
        DirectoryInfo[] subDirectories = currentDirectory.GetDirectories();
        foreach (DirectoryInfo i in subDirectories)
            if (i.Name == subDirectoryText.text)
            {
                currentDirectory = i;
                directoryInputField.text = currentDirectory.FullName;
                RemoveAndGenerateButtons();
                if (extensions.GetLength(0) == 0 && !fileNameNeeded)
                {
                    ChangeSelectedItemText(currentDirectory.Name);
                    confirmButton.interactable = true;
                }
                else if (extensions.GetLength(0) != 0)
                {
                    ChangeSelectedItemText("");
                    confirmButton.interactable = false;
                }
                break;
            }
    }
    public void ChangeSelectedItemText(string newText)
    {
        selectedItemText.text = newText;
    }
    public void DirectoryChangedByInputing(Text directoryInput)
    {
        DirectoryInfo changedDirectory = new DirectoryInfo(directoryInput.text);
        if (changedDirectory.Exists)
        {
            currentDirectory = changedDirectory;
            RemoveAndGenerateButtons();
        }
        directoryInputField.text = currentDirectory.FullName;
        if (extensions.GetLength(0) == 0)
        {
            ChangeSelectedItemText(currentDirectory.Name);
            confirmButton.interactable = true;
        }
        else
        {
            ChangeSelectedItemText("");
            confirmButton.interactable = false;
        }
    }
    public void FileNameInputed()
    {
        string newName = fileNameInputField.text;
        newName = newName.Replace("\\", "");
        newName = newName.Replace("/", "");
        newName = newName.Replace(":", "");
        newName = newName.Replace("?", "");
        newName = newName.Replace("\"", "");
        newName = newName.Replace("<", "");
        newName = newName.Replace(">", "");
        newName = newName.Replace("|", "");
        newName = newName.Replace("*", "");
        while (newName.Length > 0 && newName[0] == ' ')
            newName = newName.Remove(0, 1);
        while (newName.Length > 0 && newName[newName.Length - 1] == ' ')
            newName = newName.Remove(newName.Length - 1, 1);
        fileNameInputField.text = newName;
        if (newName.Length > 0) fileName = newName;
        else fileName = "";
        if (fileName != "") confirmButton.interactable = true;
        else confirmButton.interactable = false;
    }
    private void InitializeSelection(string[] allowedExtensions, bool needFileName)
    {
        directorySelectorCanvas.SetActive(false);
        directorySelectorCanvas.SetActive(true);
        selectedItemFullName = currentDirectory.FullName;
        if (!selectedItemFullName.EndsWith("\\")) selectedItemFullName += "\\";
        CurrentState.ignoreScroll = true;
        CurrentState.ignoreAllInput = true;
        extensions = allowedExtensions;
        fileName = "";
        directorySeletorWindow.SetActive(true);
        if (extensions.GetLength(0) == 0)
        {
            titleText.SetStrings("Select the folder", "选择文件夹");
            selectedItemType = true;
        }
        else
        {
            titleText.SetStrings("Select the file", "选择文件");
            selectedItemType = false;
        }
        confirmButton.interactable = false;
        if (extensions.GetLength(0) == 0 && !needFileName)
        {
            ChangeSelectedItemText(currentDirectory.Name);
            confirmButton.interactable = true;
        }
        fileNameInputField.gameObject.SetActive(needFileName);
        fileNameInputField.text = "";
        fileNameNeeded = needFileName;
        RemoveAndGenerateButtons();
    }
    public void ActivateSelection(string[] allowedExtensions, CallBack callback, bool needFileName = false)
    {
        callRoutine = false;
        InitializeSelection(allowedExtensions, needFileName);
        callBack = callback;
    }
    public void ActivateSelection(string[] allowedExtensions, IEnumerator callback, bool needFileName = false)
    {
        callRoutine = true;
        InitializeSelection(allowedExtensions, needFileName);
        callBackRoutine = callback;
    }
    public void SetInitialFileName(string initFileName)
    {
        fileNameInputField.text = initFileName;
        FileNameInputed();
    }
    public void DeactivateSelection()
    {
        CurrentState.ignoreScroll = false;
        CurrentState.ignoreAllInput = false;
        directorySeletorWindow.SetActive(false);
    }
    public void ConfirmButtonPressed()
    {
        int fullNameLength = currentDirectory.FullName.Length;
        if (currentDirectory.FullName[fullNameLength - 1] == '\\')
            selectedItemFullName = currentDirectory.FullName;
        else
            selectedItemFullName = currentDirectory.FullName + '\\';
        if (selectedItemType == false && !fileNameNeeded) //The item to select is a file
            selectedItemFullName += selectedItemText.text; //Add the file name after the directory
        if (callRoutine && callBackRoutine != null)
            StartCoroutine(callBackRoutine);
        else if (!callRoutine && callBack != null)
            callBack();
    }
    private void RemoveAndGenerateButtons()
    {
        //Remove all the buttons in the scroll view first
        while (fileButtons.Count > 0)
        {
            Destroy(fileButtons[0].gameObject);
            fileButtons.RemoveAt(0);
        }
        while (subDirectoryButtons.Count > 0)
        {
            Destroy(subDirectoryButtons[0].gameObject);
            subDirectoryButtons.RemoveAt(0);
        }
        //Generate new buttons
        Button newButton;
        DirectoryInfo[] subDirectories = currentDirectory.GetDirectories();
        foreach (DirectoryInfo i in subDirectories)
        {
            newButton = Instantiate(subDirectoryButtonPrefab, scrollViewContent);
            subDirectoryButtons.Add(newButton);
            newButton.GetComponentInChildren<Text>().text = i.Name;
        }
        FileInfo[] files = currentDirectory.GetFiles();
        foreach (FileInfo i in files)
        {
            bool flag = true;
            foreach (string j in extensions)
                if (i.Extension == j)
                {
                    flag = false;
                    break;
                }
            if (flag) continue; //Extension of file i isn't in the preferred extensions
            newButton = Instantiate(fileButtonPrefab, scrollViewContent);
            fileButtons.Add(newButton);
            newButton.GetComponentInChildren<Text>().text = i.Name;
            newButton.interactable = !fileNameNeeded;
        }
    }
    private void Awake()
    {
        string currentDirectoryString = Directory.GetCurrentDirectory();
        currentDirectory = new DirectoryInfo(currentDirectoryString);
        projectController = FindObjectOfType<ProjectController>();
        titleText.SetStrings("Select the folder", "选择文件夹");
        directoryInputField.text = currentDirectory.FullName;
        selectedItemText.text = "";
        scrollView.SetActive(false);
        scrollView.SetActive(true);
        directorySeletorWindow.SetActive(false);
    }
}
