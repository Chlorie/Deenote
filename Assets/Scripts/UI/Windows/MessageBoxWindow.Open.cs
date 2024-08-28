using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Utilities;
using System.Collections.Generic;
using System.Threading;

namespace Deenote.UI.Windows
{
    partial class MessageBoxWindow
    {
        private readonly List<UniTask> _buttonClickTasks = new();
        //private readonly List<MessageBoxButtonController> _buttons = new();

        /// <returns>
        /// Returns the index of clicked button, -1 if close button
        /// </returns>
        public async UniTask<int> ShowAsync(LocalizableText title, LocalizableText content, LocalizableText[] buttonTexts)
        {
            Window.IsActivated = true;

            Window.TitleBar.SetTitle(title);
            _contentText.SetText(content);

            using (var resettingButtons = _buttons.Resetting(buttonTexts.Length)) {
                foreach (var text in buttonTexts) {
                    resettingButtons.Add(out var button);
                    button.Initialize(text);
                }
            }
            _buttons.SetSiblingIndicesInOrder();

            var cts = new CancellationTokenSource();

            _buttonClickTasks.Add(Window.CloseButton.OnClickAsync(cts.Token));

            foreach (var btn in _buttons) {
                _buttonClickTasks.Add(btn.Button.OnClickAsync(cts.Token));
            }

            var clickTask = await UniTask.WhenAny(_buttonClickTasks);
            cts.Cancel();
            _buttonClickTasks.Clear();
            _buttons.Clear();

            Window.IsActivated = false;
            return clickTask - 1;
        }
    }
}