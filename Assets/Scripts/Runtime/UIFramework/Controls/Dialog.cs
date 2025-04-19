#nullable enable

using Deenote.Localization;
using System;
using UnityEngine;

namespace Deenote.UIFramework.Controls
{
    public sealed class Dialog : MonoBehaviour
    {
        [SerializeField] TextBlock _titleText = default!;
        [SerializeField] Button _closeButton = default!;

        //public event Action<Dialog, bool>? ActiveChanged;

        public Button CloseButton => _closeButton;

        public void SetTitle(LocalizableText text, ReadOnlySpan<string> args = default)
            => _titleText.SetText(text, args);
    }
}