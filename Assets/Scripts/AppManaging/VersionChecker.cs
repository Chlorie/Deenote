using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class VersionChecker : MonoBehaviour
{
    public LocalizedText updateHistoryText;
    public string currentVersion;
    private string latestVersion;
    private bool finished = false;
    private string redirectedUrl = null;
    private List<int> GetVersion(string str)
    {
        List<int> result = new List<int>();
        while (str.Length > 0)
        {
            while (str.Length > 0 && (str[0] > '9' || str[0] < '0')) str = str.Remove(0, 1);
            int num = Utility.GetInt(str);
            result.Add(num);
            while (str.Length > 0 && str[0] <= '9' && str[0] >= '0') str = str.Remove(0, 1);
        }
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
    public bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
    private void GetRedirectedUrl()
    {
        ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
        string finalUrl = "https://github.com/Chlorie/Deenote/releases/latest";
        Uri uri = new Uri(finalUrl);
        while (true)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "HEAD";
            request.AllowAutoRedirect = false;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                finalUrl = response.GetResponseHeader("Location");
                uri = new Uri(finalUrl);
            }
            else
                break;
        }
        redirectedUrl = finalUrl;
        finished = true;
    }
    public void CheckForUpdate()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return; // No Internet access
        Thread getUrlThread = new Thread(GetRedirectedUrl);
        getUrlThread.Start();
    }
    private void CheckVersion()
    {
        finished = false;
        int position = redirectedUrl.Length - 1;
        while (position >= 0 && redirectedUrl[position] != '/') position--;
        latestVersion = redirectedUrl.Substring(position + 2);
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
            new string[] { "Go to release page", "转到发布页面" }, delegate { Process.Start(redirectedUrl); },
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