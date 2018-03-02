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

[System.Serializable]
public class FullProjectDataV2 // Version 2, saves audio in a byte array
{
    public Project project = null;
    public byte[] audio = { };
    public string audioType;
}
