using Newtonsoft.Json.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Deenote.UI.Controls
{
    public sealed class KVNumericStepperProperty : KeyValueProperty
    {
        [SerializeField] Button _decrementButton = default!;
        [SerializeField] Button _incrementButton = default!;
        [SerializeField] TMP_InputField _valueInput = default!;

        private int _value;
        /// <summary>
        /// The actual value stored as <see cref="int"/>, automatically clamp 
        /// into [<see cref="MinValue"/>, <see cref="MaxValue"/>]
        /// </summary>
        public int Value
        {
            get => _value;
            set {
                value = Mathf.Clamp(value, MinValue, MaxValue);
                if (_value != value) {
                    _value = value;

                    OnValueChanged.Invoke(_value);
                    _decrementButton.gameObject.SetActive(_value > MinValue);
                    _incrementButton.gameObject.SetActive(_value < MaxValue);
                }
                // Text may still be different after clamp
                _valueInput.SetTextWithoutNotify(FormatDisplayText(value));
            }
        }

        [field: SerializeField]
        public int MinValue { get; private set; }

        [field: SerializeField]
        public int MaxValue { get; private set; }

        public UnityEvent<int> OnValueChanged { get; } = new();

        /// <summary>
        /// Invoke when end edit on input field. The function should return null
        /// if the input string is invalid, and the actual value will not change.
        /// <br/>
        /// By default, use <see cref="int.TryParse(string, out int)"/>
        /// </summary>
        public Func<string, int?>? InputParser { get; set; }
        /// <summary>
        /// Convert value to string that display on input field
        /// <br/>
        /// By default, use <see cref="int.ToString()"/>
        /// </summary>
        public Func<int, string>? DisplayTextSelector { get; set; }

        private void Awake()
        {
            _decrementButton.OnClick.AddListener(() => Value--);
            _incrementButton.OnClick.AddListener(() => Value++);
            _valueInput.onEndEdit.AddListener(input =>
            {
                int? nval;
                if (InputParser is not null)
                    nval = InputParser(input);
                else if (int.TryParse(input, out var ival))
                    nval = ival;
                else
                    nval = null;

                if (nval is { } val) {
                    Value = val;
                }
                else {
                    _valueInput.SetTextWithoutNotify(FormatDisplayText(Value));
                }
            });
        }

        private void Start()
        {
            _value = Mathf.Clamp(_value, MinValue, MaxValue);
            _decrementButton.gameObject.SetActive(_value > MinValue);
            _incrementButton.gameObject.SetActive(_value < MaxValue);
            _valueInput.SetTextWithoutNotify(FormatDisplayText(_value));
        }

        public void SetValueWithoutNotify(int value)
        {
            _value = Math.Clamp(value, MinValue, MaxValue);
            _valueInput.SetTextWithoutNotify(DisplayTextSelector?.Invoke(Value) ?? Value.ToString());
        }

        private string FormatDisplayText(int value)
            => DisplayTextSelector?.Invoke(Value) ?? Value.ToString();
    }
}