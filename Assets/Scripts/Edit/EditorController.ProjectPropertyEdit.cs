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
            proj.AudioFileRelativePath = Path.GetRelativePath(_projectManager.CurrentProjectSavePath, filePath);
            proj.AudioData = bytes;
            proj.AudioClip = clip;

            _propertiesWindow.NotifyAudioFileChanged(filePath);
        }

        public void EditProjectMusicName(string name)
        {
            _projectManager.CurrentProject.MusicName = name;
            _propertiesWindow.NotifyProjectMusicNameChanged(name);
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
            _propertiesWindow.NotifyChartNameChanged(name);
        }

        public void EditChartDifficulty(Difficulty difficulty)
        {
            _stage.Chart.Difficulty = difficulty;
            _propertiesWindow.NotifyChartDifficultyChanged(difficulty);
        }
    }
}