public class ProjectVersionConversion
{
    public static FullProjectDataV2 Version1To2(SerializableProjectData dataV1)
    {
        FullProjectDataV2 dataV2 = new FullProjectDataV2
        {
            project = dataV1.project,
            audioType = ".wav"
        };
        WavEncoder_Legacy wavEncoder = new WavEncoder_Legacy
        {
            channel = dataV1.channel,
            frequency = dataV1.frequency,
            length = dataV1.length,
            sampleData = dataV1.sampleData
        };
        wavEncoder.EncodeToWav(out dataV2.audio);
        dataV2.project.songName = "converted audio.wav";
        return dataV2;
    }
}
