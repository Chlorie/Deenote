using Deenote.GameStage;
using Deenote.Project.Models.Datas;
using Deenote.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Windows.Elements
{
    public sealed class PianoSoundListItemController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] TMP_Text _pitchText;
        [SerializeField] TMP_InputField _pitchInputField;
        [SerializeField] TMP_InputField _volumeInputField;
        [SerializeField] TMP_InputField _durationInputField;
        [SerializeField] TMP_InputField _delayInputField;
        [SerializeField] Button _removeButton;

        [Header("Data")]
        [SerializeField]
        private PianoSoundData _data;
        public PianoSoundData PianoSound => _data;

        private PianoSoundEditWindow _window;

        private void Awake()
        {
            _pitchInputField.onEndEdit.AddListener(OnPitchChanged);
            _volumeInputField.onEndEdit.AddListener(OnVolumeChanged);
            _durationInputField.onEndEdit.AddListener(OnDurationChanged);
            _delayInputField.onEndEdit.AddListener(OnDelayChanged);
            _removeButton.onClick.AddListener(() => _window.RemoveSound(this));

            _data = new(0, 0, 0, 0);
        }

        public void OnCreated(PianoSoundEditWindow window)
        {
            _window = window;
        }

        public void Initialize(in PianoSoundValueData pianoSound)
        {
            _data.SetValues(pianoSound);

            _pitchText.text = _data.ToPitchDisplayString();
            _pitchInputField.SetTextWithoutNotify(_data.Pitch.ToString());
            _volumeInputField.SetTextWithoutNotify(_data.Velocity.ToString());
            _durationInputField.SetTextWithoutNotify(_data.Duration.ToString("F3"));
            _delayInputField.SetTextWithoutNotify(_data.Delay.ToString("F3"));
        }

        #region Events

        private void OnPitchChanged(string value)
        {
            if (int.TryParse(value, out var p))
                SetPitch(p);
            else
                SetPitch(_data.Pitch);
            _window.IsDirty = true;
        }

        private void OnVolumeChanged(string value)
        {
            if (int.TryParse(value, out var p))
                SetVolume(p);
            else
                SetVolume(_data.Velocity);
            _window.IsDirty = true;
        }

        private void OnDurationChanged(string value)
        {
            if (float.TryParse(value, out var d))
                SetDuration(d);
            else
                SetDuration(_data.Duration);
            _window.IsDirty = true;
        }

        private void OnDelayChanged(string value)
        {
            if (float.TryParse(value, out var w))
                SetDelay(w);
            else
                SetDelay(_data.Delay);
            _window.IsDirty = true;
        }

        #endregion

        #region Proxy

        private void SetPitch(int pitch)
        {
            int p = Mathf.Clamp(pitch, PianoSoundManager.MinPitch, PianoSoundManager.MaxPitch);
            _data.Pitch = p;
            _pitchText.text = ProjectUtils.ToPitchDisplayString(p);
            if (p != pitch)
                _pitchInputField.SetTextWithoutNotify(p.ToString());
        }

        private void SetVolume(int volume)
        {
            int v = Mathf.Max(0, volume);
            _data.Velocity = v;
            if (v != volume)
                _volumeInputField.SetTextWithoutNotify(v.ToString());
        }

        private void SetDuration(float duration)
        {
            float d = Mathf.Max(0f, duration);
            _data.Duration = d;
            if (d != duration)
                _durationInputField.SetTextWithoutNotify(d.ToString("F3"));
        }

        private void SetDelay(float delay)
        {
            float w = Mathf.Max(0f, delay);
            _data.Delay = w;
            if (w != delay)
                _delayInputField.SetTextWithoutNotify(delay.ToString("F3"));
        }

        #endregion
    }
}