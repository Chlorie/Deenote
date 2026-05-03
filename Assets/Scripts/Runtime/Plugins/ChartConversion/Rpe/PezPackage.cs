#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Library.IO;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Deenote.Runtime.Plugins.ChartConversion.Rpe
{
    public class PezPackage
    {
        private const string InfoFileName = "info.yml";
        public Info Info { get; set; }

        public RpeChart Chart { get; set; }

        public byte[] AudioData { get; set; }

        public string CoverPath { get; set; }

        public PezPackage(Info info, RpeChart chart, byte[] audioData, string coverPath)
        {
            Info = info;
            Chart = chart;
            AudioData = audioData;
            CoverPath = coverPath;
        }

        public async UniTask SaveToFileAsync(string filePath)
        {
            await using FileStream zipFile = File.Create(filePath);
            using ZipArchive archive = new(zipFile, ZipArchiveMode.Create);

            // 1. info.txt
            byte[] infoData = Encoding.UTF8.GetBytes(Info.ToYamlString());
            await using MemoryStream infoStream = new(infoData, false);
            await WriteToZipAsync(archive, InfoFileName, infoStream);

            // 2. chart.json
            await using MemoryStream chartStream = new();
            await using StreamWriter chartWriter = new(chartStream, new UTF8Encoding(false), 1024, leaveOpen: true);
            using JsonTextWriter jsonWriter = new(chartWriter);
            new JsonSerializer().Serialize(jsonWriter, Chart);
            await jsonWriter.FlushAsync();
            chartStream.Position = 0;
            await WriteToZipAsync(archive, Info.Chart, chartStream);

            // 3. music
            await using MemoryStream songStream = new(AudioData, false);
            await WriteToZipAsync(archive, Info.Music, songStream);

            // 4. cover
            if (!IsValidFile(CoverPath)) return;
            await using FileStream coverStream = File.OpenRead(CoverPath);
            await WriteToZipAsync(archive, Info.Illustration, coverStream);
        }

        private static bool IsValidFile(string input)
            => !string.IsNullOrWhiteSpace(input) && PathUtils.IsValidPath(input) &&
               Path.IsPathFullyQualified(input) && File.Exists(input);

        private static async UniTask WriteToZipAsync(ZipArchive archive, string entry, Stream stream)
        {
            ZipArchiveEntry zipEntry = archive.CreateEntry(entry);
            await using var entryStream = zipEntry.Open();
            await stream.CopyToAsync(entryStream);
        }
    }
}