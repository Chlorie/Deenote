#nullable enable

using System;
using System.IO;
using System.Text;
using UnityEditor;

namespace Deenote.Unity.Editor
{
    public class CSharpNewBehaviourScriptTemplateFixer : AssetModificationProcessor
    {
        private const string NamespacePlaceholder = "#NAMESPACE#";

        // Invoked when about to create .meta file
        public static void OnWillCreateAsset(string assetName)
        {
            if (!assetName.EndsWith(".cs.meta"))
                return;

            var filePath = assetName[..^5];
            var contents = File.ReadAllText(filePath)
                .Replace(NamespacePlaceholder, CreateNamespace(filePath));

            File.WriteAllText(filePath, contents);
            AssetDatabase.Refresh();
        }

        private static string CreateNamespace(ReadOnlySpan<char> filePath)
        {
            var sb = new StringBuilder("Deenote");
            var dir = Path.GetDirectoryName(filePath);

            // start with 'Assets/Scripts/'
            if (dir.Length > 15) {
                var relatedPath = dir[15..];
                sb.Append(ToValidNamespace(relatedPath));
            }

            return sb.ToString();

            static string ToValidNamespace(ReadOnlySpan<char> relatedPath)
            {
                Span<char> chars = stackalloc char[relatedPath.Length + 1];
                chars[0] = '.';
                int index = 1;
                foreach (var c in relatedPath) {
                    if (c is ' ') continue;
                    chars[index++] = c == Path.DirectorySeparatorChar ? '.' : c;
                }
                return chars[..index].ToString();
            }
        }
    }
}