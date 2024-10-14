using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Deenote.UI.Controls
{
    public sealed class KVNumericSliderProperty : KeyValueProperty
    {
        [SerializeField] TMP_InputField _valueInput = default!;
        [SerializeField] Slider _slider = default!;

        public float Value
        {
            get => _slider.value;
            set => _slider.@value = value;
        }

        public UnityEvent<float> OnValueChanged => _slider.onValueChanged;

        /// <summary>
        /// Invoke when end edit on input field. The function should return null
        /// if the input string is invalid, and the actual value will not change.
        /// <br/>
        /// By default, use <see cref="int.TryParse(string, out int)"/>
        /// </summary>
        public Func<string, float?>? InputParser { get; set; }
        /// <summary>
        /// Convert value to string that display on input field
        /// <br/>
        /// By default, use <see cref="float.ToString()"/>, round to 3 decimal place
        /// </summary>
        public Func<float, string>? DisplayTextSelector { get; set; }

        private void Awake()
        {
            _slider.onValueChanged.AddListener(val =>
            {
                var text = DisplayTextSelector?.Invoke(val) ?? val.ToString("F3");
                _valueInput.SetTextWithoutNotify(text);
            });
            _valueInput.onEndEdit.AddListener(input =>
            {
                float? nval;
                if (InputParser is not null)
                    nval = InputParser(input);
                else if (float.TryParse(input, out var fval))
                    nval = fval;
                else
                    nval = null;

                if (nval is { } val) {
                    _slider.value = val;
                }
                else {
                    var text = DisplayTextSelector?.Invoke(_slider.value) ?? _slider.value.ToString("F3");
                    _valueInput.SetTextWithoutNotify(text);
                }
            });
        }
    }
}