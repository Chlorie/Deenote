using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class SerializableProjectData //Saves all the project data including the audio clip data
{
    public Project project = null; //Other project data
    //Audio clip data
    public float[] sampleData = { };
    public int frequency = 0;
    public int channel = 0;
    public int length = 0; //Length is in samples
}
