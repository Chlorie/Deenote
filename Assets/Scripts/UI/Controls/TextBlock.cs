#nullable enable

using Deenote.Localization;
using System;
using TMPro;
using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class TextBlock : MonoBehaviour
    {
        [SerializeField] TMP_Text _tmpText = default!;
        [SerializeField] LocalizedText _localizedText = default!;

        public TMP_Text TmpText => _tmpText;
        [Obsolete("新ui在textblock中代理settext")]
        public LocalizedText LocalizedText => _localizedText;

        public string Text => _tmpText.text;

        public void SetText(LocalizableText text, ReadOnlySpan<string> args = default)
            => _localizedText.SetText(text, args);

        public void SetRawText(string text) => _localizedText.SetText(LocalizableText.Raw(text));
    }
}