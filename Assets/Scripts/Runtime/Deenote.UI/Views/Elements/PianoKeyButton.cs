#nullable enable

using Deenote.UI.Views.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Deenote.UI.Views.Elements
{
    public sealed partial class PianoKeyButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] TMP_Text _text = default!;
        [SerializeField] KeyKind _kind;

        public int Pitch { get; private set; }
        private NoteInfoPianoKeysPanel _panel = default!;

        internal void OnAwake(NoteInfoPianoKeysPanel panel, int pitch)
        {
            _panel = panel;
            Pitch = pitch;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button is not PointerEventData.InputButton.Left)
                return;

            _panel.ClickPianoKey(this);
        }

        private void OnValidate()
        {
            if (_kind is KeyKind.White) {
                _backgroundImage.color = Color.white;
                _text.gameObject.SetActive(true);
            }
            else {
                _backgroundImage.color = Color.black;
                _text.gameObject.SetActive(false);
            }
        }

        private enum KeyKind { White, Black, }
    }
}