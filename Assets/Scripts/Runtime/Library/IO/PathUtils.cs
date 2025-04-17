#nullable enable

using System.IO;

namespace Deenote.Library.IO;
public static class PathUtils
{
    private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

    public static bool IsValidFileName(string? fileName)
        => fileName is not null && fileName.IndexOfAny(_invalidFileNameChars) < 0;

    public static bool IsValidPath(string path)
        => path.IndexOfAny(_invalidPathChars) < 0;
}