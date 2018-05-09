using System.IO;

public static class IOExtensions
{
    public static byte[] ReadByteArray(this BinaryReader reader)
    {
        int length = reader.ReadInt32();
        if (length == -1) return null;
        byte[] result = reader.ReadBytes(length);
        return result;
    }
    public static void WriteArray(this BinaryWriter writer, byte[] data)
    {
        if (data == null)
        {
            writer.Write(-1);
            return;
        }
        writer.Write(data.Length);
        writer.Write(data);
    }
}
