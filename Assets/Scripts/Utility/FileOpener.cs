using UnityEngine;

public class FileOpener : MonoBehaviour
{
    public ProjectController controller;
    public void CheckCommandLine()
    {
#if UNITY_EDITOR
        Debug.Log("In editor mode. Not checking command line args.");
#elif UNITY_STANDALONE_WIN
        string[] args = System.Environment.GetCommandLineArgs();
        if (args.Length > 1)
        {
            string fileName = System.Environment.GetCommandLineArgs()[1];
            if ((fileName ?? "") != "") StartCoroutine(controller.ProjectToLoadSelected(fileName));
        }
#endif
    }
}
