using Cysharp.Threading.Tasks;
using Deenote.Localization;
using LibGit2Sharp;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.ApplicationManaging
{
    public sealed class VersionManager : MonoBehaviour
    {
        private const string RepoUrl = "https://github.com/Chlorie/Deenote";
        private const string RepoLatestReleaseUrl = "https://github.com/Chlorie/Deenote/releases/latest";
        private const string RepoDownloadUrl = "https://github.com/Chlorie/Deenote/releases/download/v{0}/Deenote-{0}{1}.zip";

        private static readonly LocalizableText[] _updMsgBtnTxt = new[] {
            LocalizableText.Localized("Message_NewVersion_1"),
            LocalizableText.Localized("Message_NewVersion_2"),
            LocalizableText.Localized("Message_NewVersion_N"),
        };

        public async UniTask CheckForUpdateAsync(bool toastWhenNotInternet, bool toastIfUpdateToDate)
        {
            if (Application.internetReachability is NetworkReachability.NotReachable) {
                if (toastWhenNotInternet)
                    _ = MainSystem.StatusBar.ShowToastAsync(LocalizableText.Localized("Toast_Version_NoInternet"), 3f);
                return;
            }
            // TODO: Temp
# if UNITY_EDITOR
            return;
#endif

            int[] latest = Array.Empty<int>();
            ReadOnlyMemory<char> latestVersion = default;
            foreach (var ver in Repository.ListRemoteReferences(RepoUrl).Where(r => r.IsTag).Select(r => r.CanonicalName.AsMemory(11..))) { // Remove "refs/tags/v"
                int[] version = GetVersion(ver.Span);
                if (!IsUpToDate(version, latest))
                    continue;
                latest = version;
                latestVersion = ver;
            }

            int[] current = GetVersion(MainSystem.Args.DeenoteCurrentVersion);
            if (IsUpToDate(current, latest)) {
                if (toastIfUpdateToDate)
                    _ = MainSystem.StatusBar.ShowToastAsync(LocalizableText.Localized("Toast_UpToDate"), 3f);
                return;
            }

            var clicked = await MainSystem.MessageBox.ShowAsync(
                LocalizableText.Localized("Message_NewVersion_Title"),
                LocalizableText.Localized("Message_NewVersion_Content"),
                _updMsgBtnTxt);
            switch (clicked) {
                case 0:
                    Process.Start(RepoLatestReleaseUrl);
                    break;
                case 1:
                    string proc = Environment.Is64BitProcess ? "" : "-32bit";
                    Process.Start(string.Format(RepoDownloadUrl, latestVersion, proc));
                    break;
                default:
                    return;
            }

            int[] GetVersion(ReadOnlySpan<char> str)
            {
                using var _sp = ListPool<Range>.Get(out var splits);
                int start = 0;
                for (int i = 0; i < str.Length; i++) {
                    if (str[i] == '.') {
                        splits.Add(start..i);
                        start = i + 1;
                    }
                }
                splits.Add(start..);

                int[] result = new int[splits.Count];
                for (int i = 0; i < result.Length; i++) {
                    result[i] = int.Parse(str[splits[i]]);
                }
                return result;
            }

            bool IsUpToDate(int[] current, int[] latest)
            {
                int minLength = Mathf.Min(current.Length, latest.Length);
                for (int i = 0; i < minLength; i++) {
                    if (current[i] < latest[i])
                        return false;
                    else if (current[i] > latest[i])
                        return true;
                }
                return current.Length >= latest.Length;
            }
        }

        private void Start()
        {
            _ = CheckForUpdateAsync(true, false);
        }
    }
}