#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Entities.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Deenote.Entities.Storage
{
    public static partial class ProjectIO
    {
        public const ushort DeenoteProjectFileHeader = 0xDEE0;
        public const byte DeenoteProjectFileVersionMark = 1;

        public static Task<ProjectModel?> LoadAsync(string projectFilePath, CancellationToken cancellationToken = default)
            => Task.Run(() => Load(projectFilePath), cancellationToken);

        public static Task SaveAsync(ProjectModel project, string saveFilePath, string? backupDir, CancellationToken cancellationToken = default)
        {
            project.ProjectFilePath = saveFilePath;
            var saveProj = project.CloneForSave();
            return Task.Run(() => Save(saveProj, saveFilePath, backupDir), cancellationToken);
        }

        private static ProjectModel? Load(string projectFilePath)
        {
            if (projectFilePath.EndsWith(".dsproj")) {
                if (DsprojLoader.TryLoadDsproj(projectFilePath, out var dsproj))
                    return dsproj;
            }

            using var fs = File.OpenRead(projectFilePath);
            using var br = new BinaryReader(fs);

            var header = br.ReadUInt16();
            if (header != DeenoteProjectFileHeader)
                return null;

            var version = br.ReadByte();
            if (version != DeenoteProjectFileVersionMark)
                return null;

            var proj = ReadProject(br, projectFilePath);
            return proj;
        }

        private static void Save(ProjectModel project, string saveFilePath, string? backupDir)
        {
            var tmpPath = $"{saveFilePath}.tmp";
            using (var fs = File.OpenWrite(tmpPath)) {
                using var bw = new BinaryWriter(fs);
                bw.Write(DeenoteProjectFileHeader);
                bw.Write(DeenoteProjectFileVersionMark);
                WriteProject(bw, project);
                project.ProjectFilePath = saveFilePath;
            }

            if (File.Exists(saveFilePath)) {
                // HACK: Backup original file before overwrite
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);
                var tempSavePath = Path.Combine(backupDir, $"{Path.GetFileName(saveFilePath)}-{System.DateTime.Now:yyyyMMddHHmmss}.bak");
                File.Move(saveFilePath, tempSavePath);
                File.Delete(saveFilePath);
            }
            File.Move(tmpPath, saveFilePath);
        }
    }
}