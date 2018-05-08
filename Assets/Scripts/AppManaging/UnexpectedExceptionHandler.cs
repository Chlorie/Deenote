using System;
using System.IO;
using UnityEngine;

public class UnexpectedExceptionHandler : MonoBehaviour
{
    public static UnexpectedExceptionHandler Instance { get; private set; }
    private int exceptionCount = 0;
    private void Awake()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnexpectedException;
        Application.logMessageReceived += OnUnityLogReceived;
        if (File.Exists("exceptions.log")) File.Delete("exceptions.log");
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
            switch (LanguageController.Language)
            {
                case 0:
                    OutputMessage($"Unhandled exception: \"{condition}\", in {stackTrace}");
                    break;
                case 1:
                    OutputMessage($"未经处理的异常: \"{condition}\", 位于 {stackTrace}");
                    break;
            }
            exceptionCount++;
            UpdateStatusBar();
        }
    }
    private void OnUnexpectedException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception exception = e.ExceptionObject as Exception;
        OutputMessage(ExceptionString(exception, LanguageController.Language));
        exceptionCount++;
        UpdateStatusBar();
    }
    private void OutputMessage(string message)
    {
        using (StreamWriter streamWriter = new StreamWriter("exceptions.log", true))
            streamWriter.WriteLine(message);
    }
    private void UpdateStatusBar()
    {
        StatusBar.ErrorState = true;
        StatusBar.SetStrings(
            ((exceptionCount == 1) ? "1 unhandled exception is" : (exceptionCount + " unhandled exceptions are")) +
            " detected, full information has been output to exceptions.log",
            $"发现 {exceptionCount} 个未经处理的异常, 完整信息已输出至exceptions.log");
    }
}
