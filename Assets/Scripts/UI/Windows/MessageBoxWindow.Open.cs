using Cysharp.Threading.Tasks;
using Deenote.Localization;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Deenote.UI.Windows
{
    partial class MessageBoxWindow : MonoBehaviour
    {
        private readonly List<UniTask> _buttonClickTasks = new();

        /// <returns>
        /// Returns the index of clicked button, -1 if close button
        /// </returns>
        public async UniTask<int> ShowAsync(LocalizableText title, LocalizableText content, LocalizableText[] buttonTexts)
        {
            gameObject.SetActive(true);

            _window.SetTitle(title);
            _contentText.SetText(content);

            if (_buttons.Capacity < buttonTexts.Length) {
                _buttons.Capacity = buttonTexts.Length;
                _buttonClickTasks.Capacity = buttonTexts.Length + 1;
            }

            var cts = new CancellationTokenSource();

            _buttonClickTasks.Add(_window.OnCloseButtonClickAsync(cts.Token));

            foreach (var text in buttonTexts) {
                var btn = GetButton(text);
                btn.transform.SetAsLastSibling();
                _buttons.Add(btn);
                _buttonClickTasks.Add(btn.Button.OnClickAsync(cts.Token));
            }

            var clickTask = await UniTask.WhenAny(_buttonClickTasks);
            cts.Cancel();
            _buttonClickTasks.Clear();

            foreach (var button in _buttons) {
                _buttonPool.Release(button);
            }
            _buttons.Clear();

            gameObject.SetActive(false);
            return clickTask - 1;
        }
    }
}