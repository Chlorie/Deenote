#nullable enable

using Deenote.UI.Views.Panels;
using UnityEngine;

namespace Deenote.UI.Views.Elements
{
    public sealed class PianoOctavePanel : MonoBehaviour
    {
        public const int OctaveWhiteKeyCount = 7;
        public const int OctaveKeyCount = 12;
        public const int Octabe8KeyCount = 8;

        [SerializeField, Range(-2, 8)] int _number;
        [SerializeField] PianoKeyButton[] _pianoKeys = default!;

        internal void OnAwake(NoteInfoPianoKeysPanel panel)
        {
            Debug.Assert(_number is >= -2 and <= 8, "Invalid octave number");
            Debug.Assert(_pianoKeys.Length == (_number == 8 ? Octabe8KeyCount : OctaveKeyCount),
                "Octave has invalid key count");

            int cPitch = (_number + 2) * _pianoKeys.Length;
            for (int i = 0; i < _pianoKeys.Length; i++) {
                int pitch = cPitch + i;
                Debug.Assert(pitch is >= 0 and < 128, $"Pitch {pitch} out of range");
                _pianoKeys[i].OnAwake(panel, pitch);
            }
        }
    }
}