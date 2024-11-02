using Deenote.Project.Models.Datas;
using Deenote.UI.Controls;
using Deenote.Utilities;
using TMPro;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class PianoSoundPropertyItem : MonoBehaviour
    {
        [SerializeField] TMP_Text _pitchText = default!;
        [SerializeField] KVInputProperty _volumeProperty = default!;
        [SerializeField] KVInputProperty _durationProperty = default!;
        [SerializeField] KVInputProperty _delayProperty = default!;
        [SerializeField] Button _removeButton = default!;

        public PianoSoundPropertyPanel Parent { get; internal set; } = default!;

        private PianoSoundValueData _data;

        public ref readonly PianoSoundValueData SoundData => ref _data;

        private void Start()
        {
            _volumeProperty.InputField.OnEndEdit.AddListener(val =>
            {
                if (int.TryParse(val, out var ival)) 
                    _data.Velocity = Mathf.Max(0, ival);
                _volumeProperty.InputField.SetValueWithoutNotify(ival.ToString());
                Parent.IsDirty = true;
            });
            _durationProperty.InputField.OnEndEdit.AddListener(val =>
            {
                if (float.TryParse(val, out var fval)) 
                    _data.Duration = Mathf.Max(0f, fval);
                _durationProperty.InputField.SetValueWithoutNotify(fval.ToString("F3"));
                Parent.IsDirty = true;
            });
            _delayProperty.InputField.OnEndEdit.AddListener(val =>
            {
                if (float.TryParse(val, out var fval)) 
                    _data.Delay = Mathf.Max(0f, fval);
                _delayProperty.InputField.SetValueWithoutNotify(fval.ToString("F3"));
                Parent.IsDirty = true;
            });
            _removeButton.OnClick.AddListener(() => Parent.RemoveSoundItem(this));
        }

        internal void Initialize(in PianoSoundValueData sound)
        {
            _data = sound;
            _pitchText.text = $"{_data.ToPitchDisplayString()}({_data.Pitch})";
            _volumeProperty.InputField.SetValueWithoutNotify(_data.Velocity.ToString());
            _durationProperty.InputField.SetValueWithoutNotify(_data.Velocity.ToString("F3"));
            _delayProperty.InputField.SetValueWithoutNotify(_data.Velocity.ToString("F3"));
        }
    }
}