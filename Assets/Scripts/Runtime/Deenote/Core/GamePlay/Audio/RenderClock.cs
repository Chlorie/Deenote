#nullable enable

using System;

namespace Deenote.Core.GamePlay.Audio
{
    /// <summary>
    /// A smooth chart-time source for rendering. It advances with the render
    /// frame rate and follows the exact audio transport with a bounded phase
    /// correction. It must never be used to schedule audio.
    /// </summary>
    internal sealed class RenderClock
    {
        private const double PhaseGain = 10d;
        private const double MaxRateCorrection = 0.05d;
        private const double HardCatchUpThreshold = 0.10d;
        private const double MaxFrameDelta = 0.10d;

        private double _time;
        private long _epoch = long.MinValue;
        private bool _initialized;

        public double Time => _time;

        public double Update(double deltaTime, double targetTime, double rate,
            bool advancing, long epoch)
        {
            if (!_initialized || epoch != _epoch) {
                _time = targetTime;
                _epoch = epoch;
                _initialized = true;
                return _time;
            }

            if (!advancing) {
                _time = targetTime;
                return _time;
            }

            if (deltaTime <= 0d)
                return _time;

            deltaTime = Math.Min(deltaTime, MaxFrameDelta);
            double predicted = _time + deltaTime * rate;
            double error = targetTime - predicted;

            // A very large error means the renderer was stalled or the transport
            // was externally invalidated. Catch up once instead of spending a long
            // time running at a distorted rate.
            if (Math.Abs(error) > HardCatchUpThreshold) {
                _time = targetTime;
                return _time;
            }

            double correction = Math.Clamp(error * PhaseGain,
                -MaxRateCorrection, MaxRateCorrection);
            double next = predicted + deltaTime * correction;

            // Rendering must not move backward during normal playback.
            _time = Math.Max(_time, next);
            return _time;
        }
    }
}
