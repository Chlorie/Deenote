using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
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
    private float saveStartTime;
    private float loadStartTime;
    public void SaveProjectIntoFile(Project project, AudioClip clip, string fileFullName) //Save the project in file fileFullName
    {
        stage.forceToPlaceNotes = true;
        if (backGroundImageLeft.activeInHierarchy == true)
            savingText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            savingText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        savingText.SetActive(true);
        SerializableProjectData projectData = new SerializableProjectData();
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(fileFullName, FileMode.Create);
        projectData.project = project;
        projectData.length = clip.samples;
        projectData.frequency = clip.frequency;
        projectData.channel = clip.channels;
        projectData.sampleData = new float[projectData.length * projectData.channel];
        clip.GetData(projectData.sampleData, 0);
        binaryFormatter.Serialize(fileStream, projectData);
        fileStream.Close();
        savingText.SetActive(false);
        saveStartTime = Time.time;
        if (backGroundImageLeft.activeInHierarchy == true)
            saveCompleteText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            saveCompleteText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        saveCompleteText.SetActive(true);
        stage.forceToPlaceNotes = false;
    }
    public void LoadProjectFromFile(out Project project, out AudioClip clip, string fileFullName) //Load a project from a project file
    {
        stage.forceToPlaceNotes = true;
        if (backGroundImageLeft.activeInHierarchy == true)
            loadingText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            loadingText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        loadingText.SetActive(true);
        SerializableProjectData projectData = null;
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = new FileStream(fileFullName, FileMode.Open);
        AudioClip newClip = null;
        projectData = (SerializableProjectData)binaryFormatter.Deserialize(fileStream);
        fileStream.Close();
        newClip = AudioClip.Create("SongAudioClip", projectData.length, projectData.channel, projectData.frequency, false);
        newClip.SetData(projectData.sampleData, 0);
        clip = newClip;
        project = projectData.project;
        loadingText.SetActive(false);
        loadStartTime = Time.time;
        if (backGroundImageLeft.activeInHierarchy == true)
            loadCompleteText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            loadCompleteText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        loadCompleteText.SetActive(true);
        stage.forceToPlaceNotes = false;
    }
    private void Update()
    {
        if (Time.time > saveStartTime + 3.0f)
            saveCompleteText.SetActive(false);
        if (Time.time > loadStartTime + 3.0f)
            loadCompleteText.SetActive(false);
        return;
    }
}
