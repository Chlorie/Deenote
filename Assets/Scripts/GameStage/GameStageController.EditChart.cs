using Deenote.Project.Models;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        public void EditChartName(string name)
        {
            Chart.Name = name;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartName);
            _propertiesWindow.NotifyChartNameChanged(name, Chart.Difficulty);
        }

        public void EditChartDifficulty(Difficulty difficulty)
        {
            Chart.Difficulty = difficulty;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartDifficulty);
            _propertiesWindow.NotifyChartDifficultyChanged(difficulty);
            _perspectiveViewWindow.NotifyChartDifficultyChanged(difficulty);
        }

        public void EditChartLevel(string level)
        {
            Chart.Level = level;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartLevel);
            _propertiesWindow.NotifyChartLevelChangd(level);
            _perspectiveViewWindow.NotifyChartLevelChanged(level);
        }

        public void EditChartSpeed(float speed)
        {
            Chart.Data.Speed = speed;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartSpeed);
            _propertiesWindow.NotifyChartSpeedChanged(speed);
        }

        public void EditChartRemapVMin(int remapVMin)
        {
            Chart.Data.RemapMinVelocity = remapVMin;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartRemapMinVolume);
            _propertiesWindow.NotifyChartRemapVMinChanged(remapVMin);
        }

        public void EditChartRemapVMax(int remapVMax)
        {
            Chart.Data.RemapMinVelocity = remapVMax;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartRemapMaxVolume);
            _propertiesWindow.NotifyChartRemapVMaxChanged(remapVMax);
        }
    }
}