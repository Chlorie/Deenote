#nullable enable

using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.Dialogs.Elements;
using Octokit;
using Semver;
using System;
using System.Diagnostics;
using UnityEngine;
using Application = UnityEngine.Application;

namespace Deenote.ApplicationManaging
{
    public static class VersionChecker
    {
        public static async UniTask CheckUpdateAsync(bool notifyWhenNoInternet, bool notifyWhenUpToDate)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) {
                if (notifyWhenNoInternet)
                    MainSystem.StatusBarView.ShowToastAsync(LocalizableText.Localized("Toast_Version_NoInternet"), 3f)
                        .Forget();
                return;
            }

            GitHubClient client = new(new ProductHeaderValue("Deenote"));
            var releases = await client.Repository.Release.GetAll("Chlorie", "Deenote");
            var latest = SemVersion.Parse(releases[0].TagName.Remove(0, 1), VersionStyles);
            _current ??= SemVersion.Parse(Application.version, VersionStyles);
            if (_current.ComparePrecedenceTo(latest) >= 0) {
                if (notifyWhenUpToDate)
                    MainSystem.StatusBarView.ShowToastAsync(LocalizableText.Localized("Toast_UpToDate"), 3f).Forget();
                return;
            }

            var clicked = await MainSystem.MessageBoxDialog.OpenAsync(_updMsgBoxArgs, _current.ToString(), latest.ToString());
            switch (clicked) {
                case 0:
                    Process.Start(RepoLatestReleaseUrl);
                    break;
                case 1:
                    string suffix = IntPtr.Size == 8 ? "" : "-32bit";
                    Process.Start(string.Format(RepoDownloadUrl, latest, suffix));
                    break;
            }
        }

        private const SemVersionStyles VersionStyles = SemVersionStyles.AllowV | SemVersionStyles.OptionalPatch;
        private const string RepoUrl = "https://github.com/Chlorie/Deenote";
        private const string RepoLatestReleaseUrl = "https://github.com/Chlorie/Deenote/releases/latest";
        private const string RepoDownloadUrl =
            "https://github.com/Chlorie/Deenote/releases/download/v{0}/Deenote-{0}{1}.zip";

        private static readonly MessageBoxArgs _updMsgBoxArgs = new(
            LocalizableText.Localized("NewVersion_MsgBox_Title"),
            LocalizableText.Localized("NewVersion_MsgBox_Content"),
            LocalizableText.Localized("NewVersion_MsgBox_1"),
            LocalizableText.Localized("NewVersion_MsgBox_2"),
            LocalizableText.Localized("NewVersion_MsgBox_N"));
        private static SemVersion? _current;
    }
}