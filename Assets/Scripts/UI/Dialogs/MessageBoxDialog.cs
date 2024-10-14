using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using UnityEngine;

namespace Deenote.UI.Dialogs
{
    [RequireComponent(typeof(Dialog))]
    public sealed class MessageBoxDialog : MonoBehaviour
    {
        [SerializeField] Dialog _dialog = default!;

        [SerializeField] LocalizedText _content = default!;
        [SerializeField] Transform _buttonsParentTransform = default!;

        [Header("Prefabs")]
        [SerializeField] Button _buttonPrefab = default!;

        private PooledObjectListView<Button> _buttons;
        private readonly List<UniTask> _buttonClickTasks = new();

        private void Awake()
        {
            _buttons = new PooledObjectListView<Button>(
                UnityUtils.CreateObjectPool(_buttonPrefab, _buttonsParentTransform, defaultCapacity: 4));
        }

        public UniTask<int> OpenAsync(in MessageBoxArgs data)
            => OpenAsync(data.Title, data.Content, data.Buttons);

        /// <returns>
        /// The index of clicked button, -1 if close button
        /// </returns>
        public async UniTask<int> OpenAsync(LocalizableText title, LocalizableText content, ImmutableArray<LocalizableText> buttonTexts)
        {
            using var s_open = _dialog.Open();

            _dialog.SetTitle(title);
            _content.SetText(content);

            using (var resettingButtons = _buttons.Resetting(buttonTexts.Length)) {
                foreach (var text in buttonTexts) {
                    resettingButtons.Add(out var button);
                    button.Text.SetText(text);
                }
            }
            _buttons.SetSiblingIndicesInOrder();

            var cts = new CancellationTokenSource();
            var btns = _buttonClickTasks;
            Debug.Assert(btns.Count == 0);

            btns.Add(_dialog.CloseButton.OnClickAsync(cts.Token));
            foreach (var btn in _buttons) {
                btns.Add(btn.OnClickAsync(cts.Token));
            }

            var clickedIndex = await UniTask.WhenAny(btns);
            cts.Cancel();
            btns.Clear();
            _buttons.Clear();

            return clickedIndex - 1;
        }
    }
}