#nullable enable

using Deenote.Edit.Operations;
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
            ProjectModel.InitializationHelper.SetAudioClip(proj, clip);

            _propertyChangeNotifier.Invoke(this, NotifyProperty.Audio);
        }

        public void EditProjectMusicName(string name)
        {
            if (CurrentProject is null)
                return;
            CurrentProject.MusicName = name;

            _propertyChangeNotifier.Invoke(this, NotifyProperty.MusicName);
        }

        public void EditProjectComposer(string composerName)
        {
            if (CurrentProject is null)
                return;
            CurrentProject.Composer = composerName;

            _propertyChangeNotifier.Invoke(this, NotifyProperty.Composer);
        }

        public void EditProjectChartDesigner(string charterName)
        {
            if (CurrentProject is null)
                return;
            CurrentProject.ChartDesigner = charterName;

            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartDesigner);
        }

        public void AddProjectChart(ChartModel chart)
        {
            if (CurrentProject is null)
                return;
            CurrentProject.Charts.Add(chart);
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartList);
        }

        public void RemoveProjectChartAt(int chartIndex)
        {
            if (CurrentProject is null)
                return;
            CurrentProject.Charts.RemoveAt(chartIndex);
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartList);
        }

        internal IUndoableOperation? DoInsertTempoOperation(Tempo tempo, float endTime)
        {
            if (CurrentProject is null)
                return null;
            return CurrentProject.Tempos.InsertTempo(tempo, endTime)
                .WithDoneAction(() => _propertyChangeNotifier.Invoke(this, NotifyProperty.Tempos));
        }

        private PropertyChangeNotifier<ProjectManager, NotifyProperty> _propertyChangeNotifier = new();

        public void RegisterPropertyChangeNotification(NotifyProperty flag, Action<ProjectManager> action)
            => _propertyChangeNotifier.AddListener(flag, action);

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
            Tempos,
        }
    }
}