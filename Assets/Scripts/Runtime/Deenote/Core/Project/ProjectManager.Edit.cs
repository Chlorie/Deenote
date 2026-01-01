#nullable enable

using Deenote.Entities.Models;
using System.IO;
using UnityEngine;

namespace Deenote.Core.Project
{
    partial class ProjectManager
    {
        public void EditProjectAudio(string filePath, byte[] bytes, AudioClip clip)
        {
            ValidateProject();

            CurrentProject.AudioFileRelativePath = Path.GetRelativePath(CurrentProject.ProjectFilePath, filePath);
            CurrentProject.AudioFileData = bytes;
            _audioClip = clip;
            CurrentProject.AudioLength = clip.length;
            NotifyFlag(NotificationFlag.ProjectAudio);
        }

        public void EditProjectMusicName(string name)
        {
            ValidateProject();

            CurrentProject.MusicName = name;
            NotifyFlag(NotificationFlag.ProjectMusicName);
        }

        public void EditProjectComposer(string composer)
        {
            ValidateProject();

            CurrentProject.Composer = composer;
            NotifyFlag(NotificationFlag.ProjectComposer);
        }

        public void EditProjectChartDesigner(string chartDesigner)
        {
            ValidateProject();

            CurrentProject.ChartDesigner = chartDesigner;
            NotifyFlag(NotificationFlag.ProjectChartDesigner);
        }

        public void AddProjectChart(ChartModel chart)
        {
            ValidateProject();

            CurrentProject.Charts.Add(chart);
            NotifyFlag(NotificationFlag.ProjectCharts);
        }

        public void RemoveProjectChartAt(int chartIndex)
        {
            ValidateProject();

            CurrentProject.Charts.RemoveAt(chartIndex);
            if(CurrentProject.Charts.Count == 0)
                AddProjectChart(new ChartModel());

            NotifyFlag(NotificationFlag.ProjectCharts);
        }
    }
}