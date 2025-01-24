#nullable enable

using Deenote.UIFramework.Controls;
using Deenote.UI.Views.Elements;
using System;
using UnityEngine;

namespace Deenote.UI.Views.Panels
{
    public sealed class NoteInfoPianoKeysPanel : MonoBehaviour
    {
        private const int WhiteKeyCount = 75;

        [SerializeField] UnityEngine.UI.ScrollRect _keysScrollRect = default!;
        [SerializeField] PianoOctavePanel[] _octaves = default!;
        [SerializeField] Button[] _quickScrollButtons = default!;

        public event Action<int>? KeyClicked;

        private void Awake()
        {
            Debug.Assert(_octaves.Length == 8 - (-2) + 1, "Piano has invalid octave count");
            Debug.Assert(_quickScrollButtons.Length == _octaves.Length);

            foreach (var octave in _octaves) {
                octave.OnAwake(this);
            }

            for (int i = 0; i < _quickScrollButtons.Length; i++) {
                var cpitch = i * PianoOctavePanel.OctaveKeyCount;
                _quickScrollButtons[i].Clicked += () => ScrollTo(cpitch);
            }
        }

        internal void ClickPianoKey(PianoKeyButton key)
        {
            MainWindow.PianoSoundPlayer.PlaySound(key.Pitch);
            KeyClicked?.Invoke(key.Pitch);
        }

        private void ScrollTo(int pitch)
        {
            int octaveNumber = pitch / PianoOctavePanel.OctaveKeyCount;

            float contentWidth = _keysScrollRect.content.rect.width;
            float viewWidth = _keysScrollRect.viewport.rect.width;

            float destX = (octaveNumber * PianoOctavePanel.OctaveWhiteKeyCount / (float)WhiteKeyCount) * contentWidth;
            float moveRange = contentWidth - viewWidth;

            _keysScrollRect.horizontalNormalizedPosition = Mathf.Clamp01(destX / moveRange);
        }
    }
}