#nullable enable

using Octokit;
using Semver;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Deenote.Core
{
    public static class VersionManager
    {
        private const SemVersionStyles VersionStyles = SemVersionStyles.AllowV | SemVersionStyles.OptionalPatch;
        private const string RepoUrl = "https://github.com/Chlorie/Deenote";
        private const string RepoLatestReleaseUrl = "https://github.com/Chlorie/Deenote/releases/latest";
        private const string RepoDownloadUrl =
            "https://github.com/Chlorie/Deenote/releases/download/v{0}/Deenote-{0}{1}.zip";

        private static SemVersion? _current_bf;
        public static SemVersion CurrentVersion => _current_bf ??= SemVersion.Parse(Application.version, VersionStyles);

        public static async Task<UpdateCheckResult> CheckUpdateAsync()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                return new(UpdateCheckResultKind.NoInternet, null!);
            }

            GitHubClient client = new(new ProductHeaderValue("Deenote"));
            var releases = await client.Repository.Release.GetAll("Chlorie", "Deenote");
            var latest = SemVersion.Parse(releases[0].TagName.Remove(0, 1), VersionStyles);
            if (CurrentVersion.ComparePrecedenceTo(latest) >= 0) {
                return new(UpdateCheckResultKind.UpToDate, CurrentVersion);
            }

            return new(UpdateCheckResultKind.UpdateAvailable, latest);
        }

        public static void OpenReleasePage()
            => Process.Start(RepoLatestReleaseUrl);

        public static void OpenDownloadPage(SemVersion version)
            => Process.Start(string.Format(RepoDownloadUrl, version, IntPtr.Size == 8 ? "" : "-32bit"));

        public enum UpdateCheckResultKind
        {
            NoInternet,
            UpToDate,
            UpdateAvailable
        }

        /// <param name="LatestVersion">
        /// Availabe when Internet connection is available
        /// </param>
        public readonly record struct UpdateCheckResult(
            UpdateCheckResultKind Kind,
            SemVersion LatestVersion);
    }
}