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
    private FullProjectDataV2 projectData;
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
        try
        {
            object deserializedData = binaryFormatter.Deserialize(fileStream);
            switch (deserializedData)
            {
                case SerializableProjectData v1:
                    projectData = ProjectVersionConversion.Version1To2(v1);
                    break;
                case FullProjectDataV2 v2:
                    projectData = v2;
                    break;
            }
        }
        catch (Exception exc)
        {
            Debug.LogError(exc.Message);
        }
        fileStream.Close();
    }
    public IEnumerator SaveProjectIntoFile(Project project, byte[] audio, string fileFullName) //Save the project in file fileFullName
    {
        stage.forceToPlaceNotes = true;
        savingText.GetComponent<Text>().color = backGroundImageLeft.activeInHierarchy ? Color.black : Color.white;
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
        saveCompleteText.GetComponent<Text>().color = backGroundImageLeft.activeInHierarchy ? Color.black : Color.white;
        saveCompleteText.SetActive(true);
        stage.forceToPlaceNotes = false;
        yield return new WaitForSeconds(3.0f);
        saveCompleteText.SetActive(false);
    }
    public IEnumerator LoadProjectFromFile(Action<Project> project, Action<byte[]> audio,
        Action<string> audioType, string fileFullName) //Load a project from a project file
    {
        stage.forceToPlaceNotes = true;
        loadingText.GetComponent<Text>().color = backGroundImageLeft.activeInHierarchy ? Color.black : Color.white;
        loadingText.SetActive(true);
        Thread loadThread = new Thread(() => Load(fileFullName));
        loadThread.Start();
        while (loadThread.IsAlive) yield return null;
        project(projectData.project); audio(projectData.audio); audioType(projectData.audioType);
        loadingText.SetActive(false);
        loadCompleteText.GetComponent<Text>().color = backGroundImageLeft.activeInHierarchy ? Color.black : Color.white;
        loadCompleteText.SetActive(true);
        stage.forceToPlaceNotes = false;
    }
}
