#nullable enable

using Deenote.Localization;
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Deenote.Unity.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(LocalizableText))]
    public class LocalizableTextPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var isLocalizedProp = property.FindPropertyRelative("_isLocalized");
            var textOrKeyProp = property.FindPropertyRelative("_textOrKey");

            var propertyNameLabel = new Label(NormalizePropertyName(property.name));
            propertyNameLabel.style.minWidth = 122.5f;

            var isLocalizedToggle = new Toggle();
            isLocalizedToggle.bindingPath = isLocalizedProp.propertyPath;

            var textOrKeyLabel = new Label();
            textOrKeyLabel.style.width = 30f;
            textOrKeyLabel.style.marginLeft = 5f;


            var textOrKeyInput = new TextField();
            textOrKeyInput.bindingPath = textOrKeyProp.propertyPath;
            textOrKeyInput.style.flexGrow = 1;

            isLocalizedToggle.RegisterValueChangedCallback(ev => textOrKeyLabel.text = ev.newValue ? "Key" : "Text");

            var valueContainer = new VisualElement();
            valueContainer.style.flexDirection = FlexDirection.Row;
            valueContainer.style.flexGrow = 1;
            valueContainer.Add(isLocalizedToggle);
            valueContainer.Add(textOrKeyLabel);
            valueContainer.Add(textOrKeyInput);

            var horizontalContainer = new VisualElement();
            horizontalContainer.style.flexDirection = FlexDirection.Row;
            horizontalContainer.Add(propertyNameLabel);
            horizontalContainer.Add(valueContainer);

            return horizontalContainer;
        }

        private static string NormalizePropertyName(ReadOnlySpan<char> propertyName)
        {
            var start = 0;
            while (propertyName[start] == '_')
                start++;

            propertyName = propertyName[start..];
            Span<bool> seperates = stackalloc bool[propertyName.Length];
            int spaceCount = 0;
            for (int i = 1; i < propertyName.Length - 1; i++) {
                if (char.IsUpper(propertyName[i]) && char.IsLower(propertyName[i + 1])) {
                    seperates[i] = true;
                    spaceCount++;
                }
            }
            Span<char> chars = stackalloc char[seperates.Length + spaceCount];
            chars[0] = char.ToUpper(propertyName[0]);
            var index = 1;
            for (int i = 1; i < propertyName.Length; i++) {
                if (seperates[i])
                    chars[index++] = ' ';
                chars[index++] = propertyName[i];
            }

            return chars.ToString();
        }
    }
}