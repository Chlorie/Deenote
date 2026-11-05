#nullable enable

using System;

namespace Deenote.Core.GamePlay.Audio
{
    /// <summary>
    /// Defines the relationship between the audio DSP clock and chart time.
    /// This class contains no Unity objects and is the authoritative transport
    /// for both the music player and the audio event scheduler.
    /// </summary>
    internal sealed class AudioTransport
    {
        public enum TransportState
        {
            Stopped,
            Primed,
            Playing,
            Paused,
        }

        private double _dspAnchor;
        private double _chartAnchor;
        private double _rate = 1d;

        public TransportState State { get; private set; } = TransportState.Stopped;

        /// <summary>
        /// Incremented whenever the timeline is explicitly rebuilt. Consumers that
        /// own scheduled audio must discard work from an older epoch.
        /// </summary>
        public long Epoch { get; private set; }

        public double Rate => _rate;

        public bool IsRunning => State is TransportState.Primed or TransportState.Playing;

        public bool CanSchedule => IsRunning && _rate > 0d;

        public bool IsAdvancingAt(double dspTime)
        {
            return State == TransportState.Playing ||
                   State == TransportState.Primed && dspTime >= _dspAnchor;
        }

        public double ChartTimeAtDsp(double dspTime)
        {
            if (State is TransportState.Stopped or TransportState.Paused)
                return _chartAnchor;
            if (State == TransportState.Primed && dspTime < _dspAnchor)
                return _chartAnchor;

            return _chartAnchor + (dspTime - _dspAnchor) * _rate;
        }

        /// <summary>
        /// Returns the absolute DSP time for a chart time. Only meaningful while
        /// <see cref="CanSchedule"/> is true.
        /// </summary>
        public double DspTimeAtChart(double chartTime)
        {
            return _dspAnchor + (chartTime - _chartAnchor) / _rate;
        }

        public void Start(double dspTime, double chartTime, double rate)
        {
            _dspAnchor = dspTime;
            _chartAnchor = chartTime;
            _rate = rate;
            State = TransportState.Primed;
            Epoch++;
        }

        public void MarkStartedIfDue(double dspTime)
        {
            if (State == TransportState.Primed && dspTime >= _dspAnchor)
                State = TransportState.Playing;
        }

        public void Pause(double chartTime)
        {
            _chartAnchor = chartTime;
            State = TransportState.Paused;
            Epoch++;
        }

        public void Seek(double chartTime)
        {
            _chartAnchor = chartTime;
            State = TransportState.Paused;
            Epoch++;
        }

        public void Reanchor(double dspTime, double chartTime, double rate)
        {
            _dspAnchor = dspTime;
            _chartAnchor = chartTime;
            _rate = rate;
            State = TransportState.Playing;
            Epoch++;
        }

        public void SetRate(double rate)
        {
            _rate = rate;
            Epoch++;
        }
    }
}
