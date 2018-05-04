using System;
using System.Collections.Generic;
using UnityEngine;

public class UnexpectedExceptionHandler : MonoBehaviour
{
    public static UnexpectedExceptionHandler Instance { get; private set; }
    private List<string[]> messages = new List<string[]>();
    private void Awake()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnexpectedException;
        Application.logMessageReceived += OnUnityLogReceived;
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of UnexpectedExceptionHandler");
        }
    }
    private string ExceptionString(Exception exception, int language)
    {
        string fullMessage = exception.Message;
        Exception inner = exception.InnerException;
        while (inner != null)
        {
            fullMessage += ", " + inner.Message;
            inner = inner.InnerException;
        }
        switch (language)
        {
            case 0:
                return $"Unhandled exception: \"{fullMessage}\", in {exception.StackTrace}";
            case 1:
                return $"未经处理的异常: \"{fullMessage}\", 位于 {exception.StackTrace}";
            default:
                throw new Exception("Language parameter out of range");
        }
    }
    private void OnUnityLogReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            string[] strings = new string[LanguageController.LanguageCount];
            strings[0] = $"Unhandled exception: \"{condition}\", in {stackTrace}";
            strings[1] = $"未经处理的异常: \"{condition}\", 位于 {stackTrace}";
            messages.Add(strings);
            UpdateStatusBar();
        }
    }
    private void OnUnexpectedException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception exception = e.ExceptionObject as Exception;
        string[] strings = new string[LanguageController.LanguageCount];
        for (int i = 0; i < LanguageController.LanguageCount; i++) strings[i] = ExceptionString(exception, i);
        messages.Add(strings);
        UpdateStatusBar();
    }
    private void UpdateStatusBar()
    {
        int exceptionCount = messages.Count;
        StatusBar.ErrorState = true;
        StatusBar.SetStrings(
            (exceptionCount == 1) ? "1 unhandled exception is detected" : (exceptionCount + " unhandled exceptions are detected"),
            $"发现 {exceptionCount} 个未经处理的异常");
    }
}
