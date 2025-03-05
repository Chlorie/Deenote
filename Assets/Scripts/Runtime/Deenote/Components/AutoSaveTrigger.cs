#nullable enable

using Deenote.Library;
using System;
using UnityEngine;

namespace Deenote.Components
{
    public sealed class AutoSaveTrigger : MonoBehaviour
    {
        private const float AutoSaveIntervalTime_s = 5 * 60f;

        private float _timer;

        public event Action? AutoSaving;

        public bool IsEnabled
        {
            get => enabled;
            set => enabled = value;
        }

        private void OnDisable()
        {
            _timer = 0f;
        }

        private void Update()
        {
            if (MathUtils.IncAndTryWrap(ref _timer, Time.unscaledDeltaTime, AutoSaveIntervalTime_s)) {
                AutoSaving?.Invoke();
            }
        }
    }
}