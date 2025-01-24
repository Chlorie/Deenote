#nullable enable

using Deenote.Entities.Models;
using Deenote.UIFramework.Controls;
using Deenote.UI.Views.Panels;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class NoteInfoPianoSoundEditListItem : MonoBehaviour
    {
        [SerializeField] TextBlock _pitchText = default!;
        [SerializeField] TextBox _volumeInput = default!;
        [SerializeField] TextBox _durationInput = default!;
        [SerializeField] TextBox _delayInput = default!;
        [SerializeField] Button _removeButton = default!;

        private PianoSoundValueModel _sound;
        public ref readonly PianoSoundValueModel Sound => ref _sound;

        private NoteInfoPianoSoundEditPanel _panel = default!;

        private void Awake()
        {
            _volumeInput.EditSubmitted += val =>
            {
                if (int.TryParse(val, out var ival))
                    _sound.Velocity = Mathf.Max(0, ival);
                _volumeInput.SetValueWithoutNotify(ival.ToString());
                _panel.SetDirty();
            };
            _durationInput.EditSubmitted += val =>
            {
                if (float.TryParse(val, out var fval))
                    _sound.Duration = Mathf.Max(0f, fval);
                _durationInput.SetValueWithoutNotify(fval.ToString("F3"));
                _panel.SetDirty();
            };
            _delayInput.EditSubmitted += val =>
            {
                if (float.TryParse(val, out var fval))
                    _sound.Delay = Mathf.Max(0f, fval);
                _durationInput.SetValueWithoutNotify(fval.ToString("F3"));
                _panel.SetDirty();
            };
            _removeButton.Clicked += () => _panel.RemoveSoundItem(this);
        }

        internal void OnInstantiate(NoteInfoPianoSoundEditPanel panel)
        {
            _panel = panel;
        }

        internal void Initialize(in PianoSoundValueModel sound)
        {
            _sound = sound;
            _pitchText.SetRawText($"{sound.ToPitchDisplayString()}");
            _volumeInput.SetValueWithoutNotify(sound.Velocity.ToString());
            _durationInput.SetValueWithoutNotify(sound.Duration.ToString("F3"));
            _delayInput.SetValueWithoutNotify(sound.Delay.ToString("F3"));
        }
    }
}