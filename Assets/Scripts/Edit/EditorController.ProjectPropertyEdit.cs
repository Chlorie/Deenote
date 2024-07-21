using Deenote.Project.Models;
using System.IO;
using UnityEngine;

namespace Deenote.Edit
{
    partial class EditorController
    {
        public void EditProjectAudio(string filePath, byte[] bytes, AudioClip clip)
        {
            Debug.Assert(_projectManager.CurrentProject is not null);

            var proj = _projectManager.CurrentProject;
            proj.AudioFileRelativePath = _projectManager.CurrentProjectSaveDirectory is null
                ? filePath : Path.GetRelativePath(_projectManager.CurrentProjectSaveDirectory, filePath);
            proj.AudioFileData = bytes;
            proj.AudioClip = clip;

            _propertiesWindow.NotifyAudioFileChanged(filePath);
        }

        public void EditProjectMusicName(string name)
        {
            _projectManager.CurrentProject.MusicName = name;
            _propertiesWindow.NotifyProjectMusicNameChanged(name);
            _perspectiveViewWindow.NotifyMusicNameChanged(name);
        }

        public void EditProjectComposer(string composerName)
        {
            _projectManager.CurrentProject.Composer = composerName;
            _propertiesWindow.NotifyProjectComposerChanged(composerName);
        }

        public void EditProjectChartDesigner(string charterName)
        {
            _projectManager.CurrentProject.ChartDesigner = charterName;
            _propertiesWindow.NotifyProjectChartDesignerChanged(charterName);
        }

        public void InsertTempo(Tempo tempo, float endTime)
        {
            _operationHistory.Do(_projectManager.CurrentProject.Tempos.InsertTempo(tempo, endTime)
                .WithDoneAction(() => _stage.Grids.NotifyCurrentProjectTemposChanged()));
        }

        public void EditChartName(string name)
        {
            _stage.Chart.Name = name;
            _propertiesWindow.NotifyChartNameChanged(name, _stage.Chart.Difficulty);
        }

        public void EditChartDifficulty(Difficulty difficulty)
        {
            _stage.Chart.Difficulty = difficulty;
            _propertiesWindow.NotifyChartDifficultyChanged(difficulty);
            _perspectiveViewWindow.NotifyChartDifficultyChanged(difficulty);
        }

        public void EditChartLevel(string level)
        {
            _stage.Chart.Level = level;
            _propertiesWindow.NotifyChartLevelChangd(level);
            _perspectiveViewWindow.NotifyChartLevelChanged(level);
        }

        public void EditChartSpeed(float speed)
        {
            _stage.Chart.Data.Speed = speed;
            _propertiesWindow.NotifyChartSpeedChanged(speed);
        }

        public void EditChartRemapVMin(int remapVMin)
        {
            _stage.Chart.Data.RemapMinVelocity = remapVMin;
            _propertiesWindow.NotifyChartRemapVMinChanged(remapVMin);
        }

        public void EditChartRemapVMax(int remapVMax)
        {
            _stage.Chart.Data.RemapMinVelocity = remapVMax;
            _propertiesWindow.NotifyChartRemapVMaxChanged(remapVMax);
        }
    }
}