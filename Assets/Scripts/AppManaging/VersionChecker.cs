using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using LibGit2Sharp;
using UnityEngine;
using System.Diagnostics;

public class VersionChecker : MonoBehaviour
{
    public LocalizedText updateHistoryText;
    public string currentVersion;
    private string latestVersion;
    private bool finished = false;
    private List<int> GetVersion(string str)
    {
        string[] strings = str.Split('.');
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
    private string VersionToString(List<int> version)
    {
        string result = null; // Too lazy to use a string builder
        for (int i = 0; i < version.Count; i++)
        {
            result += version[i];
            if (i < version.Count - 1) result += '.';
        }
        return result;
    }
    private void GetLatestVersion()
    {
        IEnumerable<Reference> references = Repository.ListRemoteReferences("https://github.com/Chlorie/Deenote");
        IEnumerable<string> versions = from reference in references
                                       where reference.IsTag
                                       select reference.CanonicalName.Remove(0, 11); // Remove "refs/tags/v"
        List<int> current = GetVersion(currentVersion);
        List<int> latest = new List<int>();
        foreach (string versionString in versions)
        {
            List<int> version = GetVersion(versionString);
            if (UpToDate(version, latest))
            {
                latest = version;
                latestVersion = versionString;
            }
        }
        finished = true;
    }
    public void CheckForUpdate()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return; // No Internet access
        Thread thread = new Thread(GetLatestVersion);
        thread.Start();
    }
    private void CheckVersion()
    {
        finished = false;
        List<int> latest = GetVersion(latestVersion);
        if (UpToDate(GetVersion(currentVersion), latest))
        {
            updateHistoryText.SetStrings("The program is up to date.", "程序已是最新版本");
            return;
        }
        updateHistoryText.SetStrings("New version detected", "检测到新版本");
        FindObjectOfType<UpdateHistory>().Deactivate();
        MessageScreen.Activate(
            new string[] { "New version of Deenote detected", "检测到新版本" },
            new string[]
            {"Current version: " + currentVersion + " | Latest version: " + latestVersion,
            "当前版本: " + currentVersion + " | 最新版本: " + latestVersion},
            new string[] { "Go to release page", "转到发布页面" }, delegate { Process.Start("https://github.com/Chlorie/Deenote/releases/latest"); },
            new string[] { "Go to download page", "转到下载页面" }, GoToDownloadPage,
            new string[] { "Update later", "稍后更新" });
    }
    public void GoToDownloadPage()
    {
        string downloadUrl = "https://github.com/Chlorie/Deenote/releases/download/v" + latestVersion + "/Deenote-" + latestVersion;
        if (IntPtr.Size == 8) // 64-bit
            downloadUrl += ".zip";
        else // 32-bit
            downloadUrl += "-32bit.zip";
        Process.Start(downloadUrl);
    }
    private void Start()
    {
        CheckForUpdate();
    }
    private void Update()
    {
        if (finished) CheckVersion();
    }
}