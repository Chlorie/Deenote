using CommunityToolkit.HighPerformance.Buffers;
using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.UI.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using System;
using System.Collections.Generic;
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

        private bool _awaked;

        // OpenAsync cannot be async method as its parameter contains ROS<string>,
        // and MonoBehaviour.Awake() will invoke in OpenAsyncImpl(), before which we
        // should initialize the buttonTexts with ROS<string>, so we use a manual Awake method
        // instead of MonoBehaviour.Awake()
        private void EnsureAwake()
        {
            if (!_awaked) {
                _buttons = new PooledObjectListView<Button>(
                    UnityUtils.CreateObjectPool(_buttonPrefab, _buttonsParentTransform, defaultCapacity: 4));
                _awaked = true;
            }
        }

        public UniTask<int> OpenAsync(in MessageBoxArgs data, ReadOnlySpan<string> contentArgs = default)
        {
            EnsureAwake();

            _dialog.SetTitle(data.Title);
            _content.SetText(data.Content, contentArgs);

            using (var resettingButtons = _buttons.Resetting(data.Buttons.Length)) {
                foreach (var text in data.Buttons) {
                    resettingButtons.Add(out var button);
                    button.Text.SetText(text);
                }
            }
            _buttons.SetSiblingIndicesInOrder();

            return OpenAsyncImpl();

            async UniTask<int> OpenAsyncImpl()
            {
                using var s_open = _dialog.Open();

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

        public UniTask<int> OpenAsync(in MessageBoxArgs data, string contentArg0, string contentArg1)
        {
            using var so = SpanOwner<string>.Allocate(2);
            var span = so.Span;
            span[0] = contentArg0;
            span[1] = contentArg1;
            return OpenAsync(data, span);
        }
    }
}