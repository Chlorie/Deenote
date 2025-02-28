#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Entities.Models;
using System.IO;
using System.Threading;

namespace Deenote.Entities.Storage
{
    public static partial class ProjectIO
    {
        public const ushort DeenoteProjectFileHeader = 0xDEE0;
        public const byte DeenoteProjectFileVersionMark = 1;

        public static UniTask<ProjectModel?> LoadAsync(string projectFilePath, CancellationToken cancellationToken = default)
            => UniTask.RunOnThreadPool(() => Load(projectFilePath), configureAwait: false, cancellationToken);

        public static UniTask SaveAsync(ProjectModel project, string saveFilePath, CancellationToken cancellationToken = default)
        {
            project.ProjectFilePath = saveFilePath;
            var saveProj = project.CloneForSave();
            return UniTask.RunOnThreadPool(() => Save(saveProj, saveFilePath), configureAwait: false, cancellationToken);
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

        private static void Save(ProjectModel project, string saveFilePath)
        {
            using var fs = File.OpenWrite(saveFilePath);
            using var bw = new BinaryWriter(fs);
            bw.Write(DeenoteProjectFileHeader);
            bw.Write(DeenoteProjectFileVersionMark);
            WriteProject(bw, project);
            project.ProjectFilePath = saveFilePath;
        }
    }
}