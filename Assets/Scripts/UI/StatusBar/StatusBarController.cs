using Cysharp.Threading.Tasks;
using Deenote.Localization;
using Deenote.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace Deenote.UI.StatusBar
{
    public sealed class StatusBarController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] LocalizedText _messageText;

        [Header("Prefabs")]
        [SerializeField] Transform _toastMessageParentTransform;
        [SerializeField] ToastMessageController _toastMessagePrefab;

        private ObjectPool<ToastMessageController> _toastMessagePool;

        private int _messageToken;

        public async UniTaskVoid SetStatusMessageAsync(LocalizableText message, float duration_s)
        {
            unchecked { _messageToken++; };
            var token = _messageToken;
            _messageText.SetText(message);
            await UniTask.WaitForSeconds(duration_s, _messageText);

            if (_messageToken == token)
                _messageText.SetText(default);
        }

        public void SetStatusMessage(LocalizableText message)
        {
            unchecked { _messageToken++; }
            _messageText.SetText(message);
        }

        public async UniTaskVoid ShowToast(LocalizableText message, float duration_s)
        {
            var toast = _toastMessagePool.Get();
            toast.Initialize(message);
            toast.transform.SetAsLastSibling();

            await UniTask.WaitForSeconds(duration_s);

            _toastMessagePool.Release(toast);
        }

        private void Awake()
        {
            _toastMessagePool = UnityUtils.CreateObjectPool(_toastMessagePrefab, _toastMessageParentTransform, 1, 10);
        }
    }
}