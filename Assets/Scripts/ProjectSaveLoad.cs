using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class ProjectSaveLoad : MonoBehaviour
{
    public GameObject saveCompleteText;
    public GameObject loadCompleteText;
    public GameObject backGroundImageLeft;
    public GameObject savingText;
    public GameObject loadingText;
    public StageController stage;
    private FullProjectDataV2 projectData = null;
    private void Save(FullProjectDataV2 projectData, string fileFullName)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(fileFullName, FileMode.Create);
        binaryFormatter.Serialize(fileStream, projectData);
        fileStream.Close();
    }
    private void Load(string fileFullName)
    {
        projectData = null;
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(fileFullName, FileMode.Open);
        object deserializedData = binaryFormatter.Deserialize(fileStream);
        if (deserializedData is SerializableProjectData)
            projectData = ProjectVersionConversion.Version1To2((SerializableProjectData)deserializedData);
        else if (deserializedData is FullProjectDataV2)
            projectData = (FullProjectDataV2)deserializedData;
        fileStream.Close();
    }
    public IEnumerator SaveProjectIntoFile(Project project, byte[] audio, string fileFullName) //Save the project in file fileFullName
    {
        stage.forceToPlaceNotes = true;
        if (backGroundImageLeft.activeInHierarchy == true)
            savingText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            savingText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        savingText.SetActive(true);
        projectData = new FullProjectDataV2
        {
            project = project,
            audio = audio,
            audioType = Path.GetExtension(project.songName)
        };
        Thread saveThread = new Thread(() => Save(projectData, fileFullName));
        saveThread.Start();
        while (saveThread.IsAlive) yield return null;
        savingText.SetActive(false);
        if (backGroundImageLeft.activeInHierarchy == true)
            saveCompleteText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            saveCompleteText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        saveCompleteText.SetActive(true);
        stage.forceToPlaceNotes = false;
        yield return new WaitForSeconds(3.0f);
        saveCompleteText.SetActive(false);
    }
    public IEnumerator LoadProjectFromFile(Action<Project> project, Action<byte[]> audio,
        Action<string> audioType, string fileFullName) //Load a project from a project file
    {
        stage.forceToPlaceNotes = true;
        if (backGroundImageLeft.activeInHierarchy == true)
            loadingText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            loadingText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        loadingText.SetActive(true);
        Thread loadThread = new Thread(() => Load(fileFullName));
        loadThread.Start();
        while (loadThread.IsAlive) yield return null;
        project(projectData.project); audio(projectData.audio); audioType(projectData.audioType);
        loadingText.SetActive(false);
        if (backGroundImageLeft.activeInHierarchy == true)
            loadCompleteText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            loadCompleteText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        loadCompleteText.SetActive(true);
        stage.forceToPlaceNotes = false;
    }
}
