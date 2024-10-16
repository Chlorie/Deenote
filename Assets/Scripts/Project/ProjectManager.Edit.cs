using Deenote.Project.Models;
using Deenote.UI.ComponentModel;
using System;
using System.IO;
using UnityEngine;

namespace Deenote.Project
{
    partial class ProjectManager : INotifyPropertyChange<ProjectManager, ProjectManager.NotifyProperty>
    {
        public void EditProjectAudio(string filePath, byte[] bytes, AudioClip clip)
        {
            Debug.Assert(CurrentProject is not null);
            var proj = CurrentProject!;

            proj.AudioFileRelativePath = Path.GetRelativePath(proj.ProjectFilePath, filePath);
            proj.AudioFileData = bytes;
            proj.AudioClip = clip;

            _propertyChangedNotifier.Invoke(this, NotifyProperty.Audio);
            _propertiesWindow.NotifyAudioFileChanged(filePath);
        }

        public void EditProjectMusicName(string name)
        {
            CurrentProject.MusicName = name;

            _propertyChangedNotifier.Invoke(this, NotifyProperty.MusicName);
            _propertiesWindow.NotifyProjectMusicNameChanged(name);
            _perspectiveViewWindow.NotifyMusicNameChanged(name);
        }

        public void EditProjectComposer(string composerName)
        {
            CurrentProject.Composer = composerName;

            _propertyChangedNotifier.Invoke(this, NotifyProperty.Composer);
            _propertiesWindow.NotifyProjectComposerChanged(composerName);
        }

        public void EditProjectChartDesigner(string charterName)
        {
            CurrentProject.ChartDesigner = charterName;

            _propertyChangedNotifier.Invoke(this, NotifyProperty.ChartDesigner);
            _propertiesWindow.NotifyProjectChartDesignerChanged(charterName);
        }

        public void AddProjectChart(ChartModel chart)
        {
            CurrentProject.Charts.Add(chart);
            _propertyChangedNotifier.Invoke(this, NotifyProperty.ChartList);
        }

        public void RemoveProjectChartAt(int chartIndex)
        {
            CurrentProject.Charts.RemoveAt(chartIndex);
            _propertyChangedNotifier.Invoke(this, NotifyProperty.ChartList);
        }

        private readonly PropertyChangeNotifier<ProjectManager, NotifyProperty> _propertyChangedNotifier = new();

        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<ProjectManager> action)
            => _propertyChangedNotifier.AddListener(flag, action);

        public enum NotifyProperty
        {
            CurrentProject,
            AutoSave,
            SaveAudioDataInProject,

            Audio,
            MusicName,
            Composer,
            ChartDesigner,
            ChartList,
        }
    }
}