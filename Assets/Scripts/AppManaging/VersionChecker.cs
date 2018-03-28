using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LibGit2Sharp;
using UnityEngine;
using System.Diagnostics;

public class VersionChecker : MonoBehaviour
{
    private static VersionChecker _instance;
    [SerializeField] private string _currentVersion;
    private List<int> VersionStringToIntList(string version)
    {
        string[] strings = version.Split('.');
        List<int> result = new List<int>();
        for (int i = 0; i < strings.Length; i++) result.Add(int.Parse(strings[i]));
        return result;
    }
    private bool UpToDate(List<int> current, List<int> latest)
    {
        int currentLength = current.Count, latestLength = latest.Count;
        int min = currentLength < latestLength ? currentLength : latestLength;
        for (int i = 0; i < min; i++)
            if (current[i] < latest[i])
                return false;
            else if (current[i] > latest[i])
                return true;
        return currentLength >= latestLength;
    }
    public static void CheckForUpdate(bool noticeAnyway)
    {
        Task task = _instance.CheckUpdate(noticeAnyway);
    }
    private async Task CheckUpdate(bool noticeAnyway)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            MessageBox.Activate(new[] { "Update check", "更新检测" },
                new[]
                {
                    "No Internet connection. Please make sure that you've connected " +
                    "to the Internet.",
                    "无网络连接。请连接互联网。"
                },
                new MessageBox.ButtonInfo
                {
                    callback = () => { CheckForUpdate(noticeAnyway); },
                    texts = new[] { "Retry", "重试" }
                },
                new MessageBox.ButtonInfo { texts = new[] { "Ignore", "忽略" } });
            return;
        }
        // Check tags in the git repo
        try
        {
            IEnumerable<Reference> references = await Task.Run(() =>
                Repository.ListRemoteReferences("https://github.com/Chlorie/Deenote"));
            IEnumerable<string> versions = from reference in references
                                           where reference.IsTag
                                           select reference.CanonicalName.Remove(0, 11); // Remove "refs/tags/v"
            List<int> current = VersionStringToIntList(_currentVersion);
            List<int> latest = new List<int>();
            string latestString = "";
            foreach (string versionString in versions)
            {
                List<int> version = VersionStringToIntList(versionString);
                if (UpToDate(version, latest))
                {
                    latest = version;
                    latestString = versionString;
                }
            }
            if (UpToDate(current, latest))
            {
                if (noticeAnyway)
                    MessageBox.Activate(new[] { "Update check", "更新检测" },
                        new[] { "Your program is up to date!", "程序已是最新版本！" },
                        new MessageBox.ButtonInfo { texts = new[] { "OK", "好的" } });
            }
            else
            {
                string download = "https://github.com/Chlorie/Deenote/releases/download/v" + latestString +
                    "/Deenote-" + latestString + ((IntPtr.Size == 8) ? ".zip" : "-32bit.zip");
                MessageBox.Activate(new[] { "Update check", "更新检测" },
                    new[]
                    {
                        $"New version detected!\nCurrent: {_currentVersion}  Latest: {latestString}",
                        $"检测到新版本！\n当前版本：{_currentVersion}  最新版本：{latestString}"
                    },
                    new MessageBox.ButtonInfo
                    {
                        callback = () => { Process.Start("https://github.com/Chlorie/Deenote/releases/latest"); },
                        texts = new[] { "Release page", "前往发布页" }
                    },
                    new MessageBox.ButtonInfo
                    {
                        callback = () => { Process.Start(download); },
                        texts = new[] { "Download page", "前往下载页" }
                    },
                    new MessageBox.ButtonInfo { texts = new[] { "Update later", "稍后更新" } });
            }
        }
        catch (Exception exc)
        {
            UnityEngine.Debug.LogError(exc.Message);
        }
    }
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
        {
            Destroy(this);
            UnityEngine.Debug.LogError("Error: Unexpected multiple instances of VersionChecker");
        }
    }
}
