using System.IO;
using System.Xml;
using UnityEditor;

namespace Deenote.Editor
{
    public sealed class CsprojPostprocessor : AssetPostprocessor
    {
        private static string OnGeneratedCSProject(string path, string content)
        {
            XmlDocument doc = new();
            doc.LoadXml(content);
            XmlNamespaceManager nsManager = new(doc.NameTable);
            string nsUri = doc.DocumentElement!.GetAttribute("xmlns");
            nsManager.AddNamespace("ns", nsUri);

            XmlNode propGroup;
            if (doc.SelectSingleNode("/ns:Project/ns:PropertyGroup/ns:LangVersion", nsManager) is { } langVersion)
            {
                langVersion.InnerText = "11.0";
                propGroup = langVersion.ParentNode!;
            }
            else
            {
                propGroup = doc.CreateElement("PropertyGroup", nsUri);
                doc.SelectSingleNode("/Project", nsManager)!.PrependChild(propGroup);
            }

            XmlNode nullable = doc.CreateElement("Nullable", nsUri);
            nullable.InnerText = "enable";
            propGroup.AppendChild(nullable);

            return FormatXml(doc);
        }

        private static string FormatXml(XmlDocument doc)
        {
            using StringWriter stringWriter = new();
            XmlWriterSettings settings = new()
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings);
            doc.Save(xmlWriter);
            return stringWriter.ToString();
        }
    }
}
