#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Deenote.UI.Controls
{
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class InputField : MonoBehaviour
    {
        [SerializeField] TMP_InputField _input = default!;
        [SerializeField] TMP_Text _placeHolder = default!;

        public TMP_InputField UnityInputField => _input;

        public string PlaceHolderText
        {
            get => _placeHolder.text;
            set => _placeHolder.text = value;
        }

        public string Value
        {
            get => _input.text;
            set => _input.text = value;
        }

        public bool IsInteractable
        {
            get => _input.interactable;
            set => _input.interactable = value;
        }

        public UnityEvent<string> OnEndEdit => _input.onEndEdit;

        public UnityEvent<string> OnValueChanged => _input.onValueChanged;

        public void SetValueWithoutNotify(string value)
            => _input.SetTextWithoutNotify(value);
    }
}