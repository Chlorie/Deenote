using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
    private SerializableProjectData projectData = null;
    private void Save(SerializableProjectData projectData, string fileFullName)
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
        projectData = (SerializableProjectData)binaryFormatter.Deserialize(fileStream);
        fileStream.Close();
    }
    public IEnumerator SaveProjectIntoFile(Project project, AudioClip clip, string fileFullName) //Save the project in file fileFullName
    {
        stage.forceToPlaceNotes = true;
        if (backGroundImageLeft.activeInHierarchy == true)
            savingText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            savingText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        savingText.SetActive(true);
        projectData = new SerializableProjectData();
        projectData.project = project;
        projectData.length = clip.samples;
        projectData.frequency = clip.frequency;
        projectData.channel = clip.channels;
        projectData.sampleData = new float[projectData.length * projectData.channel];
        clip.GetData(projectData.sampleData, 0);
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
    public IEnumerator LoadProjectFromFile(Action<Project> project, Action<AudioClip> clip, string fileFullName) //Load a project from a project file
    {
        stage.forceToPlaceNotes = true;
        if (backGroundImageLeft.activeInHierarchy == true)
            loadingText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            loadingText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        loadingText.SetActive(true);
        AudioClip newClip = null;
        Thread loadThread = new Thread(() => Load(fileFullName));
        loadThread.Start();
        while (loadThread.IsAlive) yield return null;
        newClip = AudioClip.Create("SongAudioClip", projectData.length, projectData.channel, projectData.frequency, false);
        newClip.SetData(projectData.sampleData, 0);
        project(projectData.project); clip(newClip);
        loadingText.SetActive(false);
        if (backGroundImageLeft.activeInHierarchy == true)
            loadCompleteText.GetComponent<Text>().color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        else
            loadCompleteText.GetComponent<Text>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        loadCompleteText.SetActive(true);
        stage.forceToPlaceNotes = false;
    }
}
