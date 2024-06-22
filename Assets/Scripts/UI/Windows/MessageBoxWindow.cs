using Deenote.Localization;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class MessageBoxWindow : MonoBehaviour
    {
        [SerializeField] Window _window;

        [Header("UI")]
        [SerializeField] LocalizedText _contentText;

        [Header("Prefab")]
        [SerializeField] Transform _buttonParentTransform;
        [SerializeField] MessageBoxButtonController _buttonPrefab;
        private ObjectPool<MessageBoxButtonController> _buttonPool;
        private List<MessageBoxButtonController> _buttons;

        private void Awake()
        {
            _buttonPool = UnityUtils.CreateObjectPool(_buttonPrefab, _buttonParentTransform, 0);
            _buttons = new();
        }

        private MessageBoxButtonController GetButton(LocalizableText text)
        {
            var btn = _buttonPool.Get();
            btn.Initialize(text);
            return btn;
        }
    }
}