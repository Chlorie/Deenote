using System;
using System.IO;

namespace Deenote.Utilities
{
    public static class Utils
    {
        private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

        public static bool IsValidFileName(string fileName)
            => fileName.IndexOfAny(_invalidFileNameChars) < 0;

        public static bool IsValidPath(string path)
            => path.IndexOfAny(_invalidPathChars) < 0;

        public static bool EndsWithOneOf(this string str, ReadOnlySpan<string> ends)
        {
            foreach (var end in ends) {
                if (str.EndsWith(end))
                    return true;
            }
            return false;
        }
    }
}