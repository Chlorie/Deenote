using Deenote.Localization;
using Deenote.UI.Windows.Elements;
using Deenote.Utilities;
using Deenote.Utilities.Robustness;
using UnityEngine;

namespace Deenote.UI.Windows
{
    [RequireComponent(typeof(Window))]
    public sealed partial class MessageBoxWindow : MonoBehaviour
    {
        [SerializeField] Window _window;
        public Window Window => _window;

        [Header("UI")]
        [SerializeField] LocalizedText _contentText;

        [Header("Prefab")]
        [SerializeField] Transform _buttonParentTransform;
        [SerializeField] MessageBoxButtonController _buttonPrefab;
        private PooledObjectListView<MessageBoxButtonController> _buttons;

        private void Awake()
        {
            _buttons = new PooledObjectListView<MessageBoxButtonController>(
                UnityUtils.CreateObjectPool(_buttonPrefab, _buttonParentTransform, 4));
        }
    }
}