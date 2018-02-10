using System;
using System.IO;

public class WavEncoder
{
    public float[] sampleData = { };
    public int frequency = 0;
    public int channel = 0;
    public int length = 0; //Length is in samples
    // Code below mostly by darktable, modified by myself for my own use
    private const int HEADER_SIZE = 44;
    public void EncodeToWav(out byte[] wav)
    {
        MemoryStream memStream = CreateEmpty();
        ConvertAndWrite(memStream);
        WriteHeader(memStream);
        wav = memStream.GetBuffer();
        memStream.Close();
    }
    private MemoryStream CreateEmpty()
    {
        MemoryStream memStream = new MemoryStream();
        byte emptyByte = new byte();
        for (int i = 0; i < HEADER_SIZE; i++) memStream.WriteByte(emptyByte); // Preparing the header
        return memStream;
    }
    private void ConvertAndWrite(MemoryStream memStream)
    {
        short[] intData = new System.Int16[sampleData.Length];
        // Converting in 2 float[] steps to short[], then short[] to byte[]
        byte[] bytesData = new byte[sampleData.Length * 2];
        // bytesData array is twice the size of dataSource array because a float converted in short is 2 bytes.
        int rescaleFactor = 32767; // To convert float to short
        for (int i = 0; i < sampleData.Length; i++)
        {
            intData[i] = (short)(sampleData[i] * rescaleFactor);
            byte[] byteArr = new byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        memStream.Write(bytesData, 0, bytesData.Length);
    }
    private void WriteHeader(MemoryStream memStream)
    {
        memStream.Seek(0, SeekOrigin.Begin);
        byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        memStream.Write(riff, 0, 4);
        byte[] chunkSize = BitConverter.GetBytes(memStream.Length - 8);
        memStream.Write(chunkSize, 0, 4);
        byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        memStream.Write(wave, 0, 4);
        byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        memStream.Write(fmt, 0, 4);
        byte[] subChunk1 = BitConverter.GetBytes(16);
        memStream.Write(subChunk1, 0, 4);
        // ushort two = 2;
        ushort one = 1;
        byte[] audioFormat = BitConverter.GetBytes(one);
        memStream.Write(audioFormat, 0, 2);
        byte[] numChannels = BitConverter.GetBytes(channel);
        memStream.Write(numChannels, 0, 2);
        byte[] sampleRate = BitConverter.GetBytes(frequency);
        memStream.Write(sampleRate, 0, 4);
        byte[] byteRate = BitConverter.GetBytes(frequency * channel * 2); // sampleRate * bytesPerSample * number of channels, here 44100*2*2
        memStream.Write(byteRate, 0, 4);
        ushort blockAlign = (ushort)(channel * 2);
        memStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);
        ushort bps = 16;
        byte[] bitsPerSample = BitConverter.GetBytes(bps);
        memStream.Write(bitsPerSample, 0, 2);
        byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        memStream.Write(datastring, 0, 4);
        byte[] subChunk2 = BitConverter.GetBytes(length * channel * 2);
        memStream.Write(subChunk2, 0, 4);
    }
}
