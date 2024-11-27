#nullable enable

using Deenote.Project.Models;

namespace Deenote.GameStage
{
    partial class GameStageController
    {
        public void EditChartName(string name)
        {
            if (Chart is null)
                return;

            Chart.Name = name;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartName);
        }

        public void EditChartDifficulty(Difficulty difficulty)
        {
            if (Chart is null)
                return;

            Chart.Difficulty = difficulty;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartDifficulty);
            if (Chart.Name is null) // If name is null, the actual name is related to difficulty
                _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartName);
        }

        public void EditChartLevel(string level)
        {
            if (Chart is null)
                return;

            Chart.Level = level;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartLevel);
        }

        public void EditChartSpeed(float speed)
        {
            if (Chart is null)
                return;

            Chart.Data.Speed = speed;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartSpeed);
        }

        public void EditChartRemapVMin(int remapVMin)
        {
            if (Chart is null)
                return;

            Chart.Data.RemapMinVelocity = remapVMin;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartRemapMinVolume);
        }

        public void EditChartRemapVMax(int remapVMax)
        {
            if (Chart is null)
                return;

            Chart.Data.RemapMinVelocity = remapVMax;
            _propertyChangeNotifier.Invoke(this, NotifyProperty.ChartRemapMaxVolume);
        }
    }
}