#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Cysharp.Threading.Tasks;
using Deenote.UIFramework.Controls;
using Deenote.UI.Dialogs.Elements;
using Deenote.Library;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Deenote.Library.Collections;

namespace Deenote.UI.Dialogs
{
    public sealed class MessageBox : ModalDialog
    {
        [SerializeField] TextBlock _titleText = default!;
        [SerializeField] TextBlock _contentText = default!;
        [SerializeField] Transform _buttonsParentTransform = default!;

        [Header("Prefabs")]
        [SerializeField] Button _buttonPrefab = default!;
        private PooledObjectListView<Button> _buttons;
        private readonly List<UniTask> _buttonClickTasks = new();
        private Button? _prevHightlightButton;

        protected override void Awake()
        {
            base.Awake();

            _buttons = new(UnityUtils.CreateObjectPool(_buttonPrefab, _buttonsParentTransform, defaultCapacity: 2));
        }

        public UniTask<int> OpenAsync(in MessageBoxArgs data, ReadOnlySpan<string> contentArgs = default)
        {
            OpenSelfModalDialog();
            _titleText.SetText(data.Title);
            _contentText.SetText(data.Content, contentArgs);

            if (_prevHightlightButton is not null) {
                _prevHightlightButton.ColorSet = Button.ButtonColorSet.Default;
                _prevHightlightButton = null;
            }
            using (var resetter = _buttons.Resetting(data.Buttons.Length)) {
                foreach (var text in data.Buttons) {
                    resetter.Add(out var button);
                    button.Text.SetText(text);
                }
            }
            _buttons.SetSiblingIndicesInOrder();
            if ((uint)data.HightlightIndex < (uint)_buttons.Count) {
                _prevHightlightButton = _buttons[data.HightlightIndex];
                _prevHightlightButton.ColorSet = Button.ButtonColorSet.Accent;
            }

            return Impl();

            async UniTask<int> Impl()
            {
                var cts = new CancellationTokenSource();
                var btns = _buttonClickTasks;
                Debug.Assert(btns.Count == 0);

                foreach (var btn in _buttons) {
                    btns.Add(btn.OnClickAsync(cts.Token));
                }

                var clicked = await UniTask.WhenAny(btns);
                cts.Cancel();
                btns.Clear();
                _buttons.Clear();

                CloseSelfModalDialog();
                return clicked;
            }
        }

        public UniTask<int> OpenAsync(in MessageBoxArgs data, string contentArg0, string contentArg1)
        {
            using var so = SpanOwner<string>.Allocate(2);
            var span = so.Span;
            span[0] = contentArg0;
            span[1] = contentArg1;
            return OpenAsync(data, contentArgs: span);
        }
    }
}