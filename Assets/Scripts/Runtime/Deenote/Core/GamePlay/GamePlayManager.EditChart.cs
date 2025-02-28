#nullable enable

using Deenote.Entities;

namespace Deenote.Core.GamePlay
{
    partial class GamePlayManager
    {
        public void EditChartName(string name)
        {
            ValidateChart();

            CurrentChart.Name = name;
            NotifyFlag(NotificationFlag.ChartName);
        }

        public void EditChartDifficulty(Difficulty difficulty)
        {
            ValidateChart();

            CurrentChart.Difficulty = difficulty;
            NotifyFlag(NotificationFlag.ChartDifficulty);
        }

        public void EditChartLevel(string level)
        {
            ValidateChart();

            CurrentChart.Level = level;
            NotifyFlag(NotificationFlag.ChartLevel);
        }

        public void EditChartSpeed(float speed)
        {
            ValidateChart();

            CurrentChart.Speed = speed;
            NotifyFlag(NotificationFlag.ChartSpeed);
        }

        public void EditChartRemapMinVolume(int remapVMin)
        {
            ValidateChart();

            CurrentChart.RemapMinVolume = remapVMin;
            NotifyFlag(NotificationFlag.ChartRemapMinVolume);
        }

        public void EditChartRemaMaxVolume(int remapVMax)
        {
            ValidateChart();

            CurrentChart.RemapMaxVolume = remapVMax;
            NotifyFlag(NotificationFlag.ChartRemapMaxVolume);
        }
    }
}