using Deenote.Project.Models;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        public void EditChartName(string name)
        {
            Chart.Name = name;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartName);
        }

        public void EditChartDifficulty(Difficulty difficulty)
        {
            Chart.Difficulty = difficulty;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartDifficulty);
            if (Chart.Name is null) // If name is null, the actual name is related to difficulty
                _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartName);
        }

        public void EditChartLevel(string level)
        {
            Chart.Level = level;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartLevel);
        }

        public void EditChartSpeed(float speed)
        {
            Chart.Data.Speed = speed;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartSpeed);
        }

        public void EditChartRemapVMin(int remapVMin)
        {
            Chart.Data.RemapMinVelocity = remapVMin;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartRemapMinVolume);
        }

        public void EditChartRemapVMax(int remapVMax)
        {
            Chart.Data.RemapMinVelocity = remapVMax;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartRemapMaxVolume);
        }
    }
}