#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Deenote.UI.Views.Elements
{
    public sealed partial class PianoKeyButton : MonoBehaviour
    {
        [SerializeField] Image _backgroundImage = default!;
        [SerializeField] TMP_Text _text = default!;
        [SerializeField] KeyKind _kind;

        private int _pitch_t;

        private enum KeyKind { White, Black, }
    }

    partial class PianoKeyButton
    {
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
    }
}