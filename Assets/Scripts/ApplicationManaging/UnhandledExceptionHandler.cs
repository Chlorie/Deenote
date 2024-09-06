using Deenote.Localization;
using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Deenote.ApplicationManaging
{
    public sealed class UnhandledExceptionHandler
    {
        public UnhandledExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.logMessageReceived += OnUnityLogMessageReceived;
            if (File.Exists(LogFile))
                File.Delete(LogFile);
        }

        private const string LogFile = "exceptions.log";

        private void OnUnityLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type is LogType.Exception)
                HandleMessage(condition, stackTrace);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            HandleMessage(CreateLogMessage(ex), ex.StackTrace);
            return;

            static string CreateLogMessage(Exception exception)
            {
                var sb = new StringBuilder();
                sb.Append(exception.Message);
                var inner = exception.InnerException;
                while (inner != null) {
                    sb.Append($", {inner.Message}");
                    inner = inner.InnerException;
                }
                return sb.ToString();
            }
        }

        private void HandleMessage(string message, string stackTrace)
        {
            using var sw = new StreamWriter(LogFile, append: true);
            var msg = MainSystem.Localization.GetText(LocalizableText.Localized("Misc_UnhandledExceptionMessage"));
            sw.WriteLine(msg, message, stackTrace);
            MainSystem.StatusBar.ShowToastAsync(LocalizableText.Localized("Toast_UnhandledException"), 3f).Forget();
        }
    }
}