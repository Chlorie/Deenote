#nullable enable

using Deenote.Project.Models.Datas;
using Deenote.UI.Controls;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class PianoOctaveView : MonoBehaviour
    {
        public const int OctaveWhiteKeyCount = 7;
        public const int OctaveKeyCount = 12;
        public const int Octave8KeyCount = 8;

        [SerializeField, Range(-2, 8)] int _number;
        [SerializeField] Button[] _pianoKeys;

        public PianoSoundPropertyPanel Parent { get; internal set; } = default!;

        private void Start()
        {
            Debug.Assert(_pianoKeys.Length == (_number == 8 ? Octave8KeyCount : OctaveKeyCount),
                "Octave has invalid key count.");

            int cPitch = (_number + 2) * _pianoKeys.Length;
            for (int i = 0; i < _pianoKeys.Length; i++) {
                int pitch = cPitch + i;
                Debug.Assert(pitch >= 0 && pitch < 128,$"Pitch {pitch} out of range.");
                _pianoKeys[i].OnClick.AddListener(() =>
                {
                    MainSystem.PianoSoundManager.PlaySoundAsync(pitch, 95, null, 0f, 1f).Forget();
                    Parent.AddSoundData(new PianoSoundValueData(0f, 0f, pitch, 0));
                });
            }
        }
    }
}