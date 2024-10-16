using Deenote.Localization;
using System;
using UnityEngine;

namespace Deenote.UI.Controls
{
    public sealed class Dialog : MonoBehaviour
    {
        [SerializeField] Button _closeButton = default!;
        [SerializeField] LocalizedText _titleBar = default!;

        public event Action<Dialog, bool> ActiveChanged;

        public Button CloseButton => _closeButton;

        public void SetTitle(LocalizableText text, ReadOnlySpan<string> args = default)
            => _titleBar.SetText(text, args);

        public DialogOpenScope Open()
            => new DialogOpenScope(this);

        public void Close() => gameObject.SetActive(false);

        private void Start()
        {
            _closeButton.OnClick.AddListener(() => gameObject.SetActive(false));
        }

        private void OnEnable()
        {
            ActiveChanged?.Invoke(this, true);
        }

        private void OnDisable()
        {
            ActiveChanged?.Invoke(this, false);
        }

        public readonly struct DialogOpenScope : IDisposable
        {
            private readonly Dialog _dialog;

            public DialogOpenScope(Dialog dialog)
            {
                _dialog = dialog;
                _dialog.gameObject.SetActive(true);
            }

            public void Dispose()
            {
                _dialog.gameObject.SetActive(false);
            }
        }
    }
}