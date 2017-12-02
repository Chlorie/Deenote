using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using UnityEngine;

public class FileOpener : MonoBehaviour
{
    public ProjectController controller;
    public void CheckRegistry()
    {
#if UNITY_EDITOR
        Debug.Log("In editor mode. Not editing registry.");
#elif UNITY_STANDALONE_WIN
        RegistryKey key;
        key = Registry.ClassesRoot.OpenSubKey(".dsproj", true) ?? Registry.ClassesRoot.CreateSubKey(".dsproj");
        key.SetValue("", "DeenoteFile");
        key.Close();
        key = Registry.ClassesRoot.OpenSubKey("DeenoteFile\\shell\\open\\command", true) ??
            Registry.ClassesRoot.CreateSubKey("DeenoteFile\\shell\\open\\command");
        string exeDir = System.Environment.GetCommandLineArgs()[0];
        key.SetValue("", "\"" + exeDir + "\" \"%1\"");
        key.Close();
#endif
    }
    public void CheckCommandLine()
    {
#if UNITY_EDITOR
        Debug.Log("In editor mode. Not checking command line args.");
#elif UNITY_STANDALONE_WIN
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            string fileName = System.Environment.GetCommandLineArgs()[1];
            if ((fileName ?? "") != "") controller.ProjectToLoadSelected(fileName);
        }
#endif
    }
}
