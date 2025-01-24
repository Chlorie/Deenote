#nullable enable

using Deenote.Library;
using System;
using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    public sealed class NumericStepper : MonoBehaviour
    {
        [SerializeField] Button _leftButton = default!;
        [SerializeField] TextBox _textBox = default!;
        [SerializeField] Button _rightButton = default!;

        [SerializeField, Min(1)] int _step = 1;
        [SerializeField] int _minValue;
        [SerializeField] int _maxValue;

        private int _value;
        private Func<string, int?>? _inpuParser;
        private Func<int, string>? _displayTextSelector;

        public int Step
        {
            get => _step;
            set => _step = value;
        }
        public int MinValue
        {
            get => _minValue;
            set {
                if (Utils.SetField(ref _minValue, value)) {
                    if (Value < value)
                        Value = value;
                }
            }
        }
        public int MaxValue
        {
            get => _maxValue;
            set {
                if (Utils.SetField(ref _maxValue, value)) {
                    if (Value > value)
                        Value = value;
                }
            }
        }

        public int Value
        {
            get => _value;
            set {
                value = Mathf.Clamp(value, MinValue, MaxValue);
                if (Utils.SetField(ref _value, value)) {
                    SetButtonActive();
                    ValueChanged?.Invoke(value);
                }
                // Text may still be different after clamp
                _textBox.SetValueWithoutNotify(FormatDisplayText(value));
            }
        }

        public void SetInputParser(Func<string, int?> parser)
        {
            _inpuParser = parser;
        }

        public void SetDisplayerTextSelector(Func<int, string> selector)
        {
            _displayTextSelector = selector;
        }

        public void SetValueWithoutNotify(int value)
        {
            _value = value;
            SetButtonActive();
            _textBox.SetValueWithoutNotify(FormatDisplayText(value));
        }

        public event Action<int>? ValueChanged;

        private void Awake()
        {
            _leftButton.Clicked += () => Value -= Step;
            _rightButton.Clicked += () => Value += Step;
            _textBox.EditSubmitted += str =>
            {
                int? nval;
                if (_inpuParser is not null)
                    nval = _inpuParser(str);
                else if (int.TryParse(str, out var ival))
                    nval = ival;
                else
                    nval = null;

                if (nval is { } val)
                    Value = val;
                else
                    Value = Value;
            };
        }

        private void Start()
        {
            _value = Mathf.Clamp(_value, MinValue, MaxValue);
        }

        public void Initialize(int minValue, int maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        private void SetButtonActive()
        {
            _leftButton.gameObject.SetActive(_value > MinValue);
            _rightButton.gameObject.SetActive(_value < MaxValue);
        }

        private string FormatDisplayText(int value)
            => _displayTextSelector?.Invoke(value) ?? value.ToString();
    }
}